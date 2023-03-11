using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

        string message;
        bool showNsg;

        /// <inheritdoc/>
        public string Message
        {
            get
            {
                showNsg = false;
                return message;
            }

            private set
            {
                message = value;
                showNsg = true;
            }
        }

        /// <inheritdoc/>
        public IAdbSocket Socket { get; }

        /// <inheritdoc/>
        public string CurrentDirectory => Match.Groups["directory"].Value;

        /// <inheritdoc/>
        public ShellAccess Access { get; private set; }

        public ShellSocket(IAdbSocket socket)
        {
            Socket = socket;

            GetPrompt();
        }

        /// <inheritdoc/>
        public string ReadAvailable(bool wait = false, TextWriter writer = null)
        {
            while (true)
            {
                int count = Socket.Available;

                if (count > 0)
                {
                    var resp = new byte[count];
                    Socket.Read(resp);
                    Invalidate();
                    string result = Encoding.ASCII.GetString(resp);
                    if (result[^2] is '$' or '#')
                    {
                        CheckPrompt(result);
                    }

                    writer?.Write(result);
                    return result;
                }
                else if (!wait)
                {
                    break;
                }
            }

            return string.Empty;
        }

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
            if (showNsg && Socket.Available == 0)
            {
                string msg = Message;
                if (!invalidation)
                {
                    showNsg = true;
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
            showNsg = false;
            validMatch = false;
        }

        void CheckPrompt(string result)
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
        }
    }
}
