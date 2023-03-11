using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using AndroCtrl.Protocols.AndroidDebugBridge.Interfaces;

namespace AndroCtrl.Protocols.AndroidDebugBridge
{
    /// <summary>
    /// Provides methods to interact with Adb shell session.
    /// </summary>
    public class ShellSocket : IShellSocket
    {
        readonly Regex Regex = new(@"(?<num>[1-9]*)\W*\b(?<host>\w+):(?<directory>.*)\s(?<user>\$|#) $");
        Match Match;
        bool validMatch;

        StreamReader reader;

        List<string> lines = new();
        string message;
        bool showMsg;

        /// <inheritdoc/>
        public string Message
        {
            get
            {
                showMsg = false;
                return message;
            }

            private set
            {
                message = value;
                showMsg = true;
            }
        }

        public bool HasPendingData => lines.Count > 0;

        /// <inheritdoc/>
        public IAdbSocket Socket { get; }

        /// <inheritdoc/>
        public string CurrentDirectory => Match.Groups["directory"].Value;

        /// <inheritdoc/>
        public ShellAccess Access { get; private set; }

        public ShellSocket(IAdbSocket socket)
        {
            Socket = socket;
            reader = new(socket.GetShellStream());

            GetPrompt();
        }

        string CheckLine(TextWriter writer)
        {
            string line = lines[0];
            lines.RemoveAt(0);

            if (!CheckPrompt(line))
            {
                line += Environment.NewLine;
            }

            writer?.Write(line);
            return line;
        }

        /// <inheritdoc/>
        public string ReadAvailable(bool wait = false, TextWriter writer = null)
        {
            if (HasPendingData)
            {
                return CheckLine(writer);
            }

            while (true)
            {
                int count = Socket.Available;

                if (count > 0)
                {
                    var resp = new byte[count];
                    Socket.Read(resp);
                    Invalidate();
                    string result = Encoding.ASCII.GetString(resp);
                    lines = result.Split(Environment.NewLine).ToList();
                    return CheckLine(writer);
                }
                else if (!wait)
                {
                    break;
                }
            }

            return string.Empty;
        }

        /*
        /// <inheritdoc/>
        public string ReadAvailable(bool wait = false, TextWriter writer = null)
        {
            while (true)
            {
                if (Socket.Available == 0 && wait)
                {
                    continue;
                }

                string line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                else
                {
                    if (line[^2] is '$' or '#')
                    {
                        CheckPrompt(line);
                    }

                    writer?.Write(line);
                    return line;
                }
            }

            return String.Empty;
        }
        */

        /// <inheritdoc/>
        public string ReadToEnd(bool noPrompt = false, TextWriter writer = null)
        {
            string result = "";

            while (true)
            {
                var data = ReadAvailable(true);
                if (data != string.Empty && (!noPrompt || (noPrompt && !validMatch)))
                {
                    result += data;
                    writer?.Write(data);
                }

                if (validMatch)
                {
                    break;
                }
            }

            writer?.Flush();
            return result;
        }

        /// <inheritdoc/>
        public void SendCommand(string command)
        {
            string formedCommand = command + "\n";
            byte[] data = Encoding.ASCII.GetBytes(formedCommand);
            Socket.Send(data, 0, data.Length);
        }

        /// <inheritdoc/>
        public string Interact(string command, TextWriter writer)
        {
            // Clear pending data
            GetPrompt();

            // Send command
            SendCommand(command);

            // Receive data without prompt
            return ReadToEnd(true, writer);
        }

        /// <inheritdoc/>
        public string GetPrompt(bool invalidation = true)
        {
            if (showMsg && Socket.Available == 0)
            {
                string msg = Message;
                if (!invalidation)
                {
                    showMsg = true;
                }

                return msg;
            }
            else
            {
                ReadToEnd();
                return message;
            }
        }

        void Invalidate()
        {
            showMsg = false;
            validMatch = false;
        }

        bool CheckPrompt(string result)
        {
            Match m = Regex.Match(result);

            if (m.Success)
            {
                Match = m;

                Message = result;
                if (Match.Groups["access"].Value == "#")
                {
                    Access = ShellAccess.Root;
                }
                else
                {
                    Access = ShellAccess.Adb;
                }

                validMatch = true;
            }

            return m.Success;
        }
    }
}
