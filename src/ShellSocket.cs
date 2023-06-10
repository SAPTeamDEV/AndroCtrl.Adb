// <copyright file="ShellSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, SAP Team">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, Alireza Poodineh. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

using SAPTeam.AndroCtrl.Adb.Interfaces;
using SAPTeam.AndroCtrl.Adb.Receivers;

namespace SAPTeam.AndroCtrl.Adb
{
    /// <summary>
    /// Provides methods to interact with Adb shell session.
    /// </summary>
    public class ShellSocket : IShellSocket, IDisposable
    {
        readonly Regex Regex = new Regex(@"(?<num>[1-9]*)\W*\b(?<host>\w+):(?<directory>.*)\s(?<user>\$|#) $");
        Match Match;
        bool validMatch;

        StreamReader reader;

        List<string> lines = new List<string>();
        string message;
        string command;
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

        private bool HasPendingData => lines.Count > 0;

        /// <inheritdoc/>
        public IAdbSocket Socket { get; }

        /// <inheritdoc/>
        public bool Connected => Socket.Connected;

        /// <inheritdoc/>
        public string CurrentDirectory => Match.Groups["directory"].Value;

        /// <inheritdoc/>
        public ShellAccess Access { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellSocket"/> class.
        /// </summary>
        /// <param name="socket">
        /// The shell socket of specified device.
        /// </param>
        public ShellSocket(IAdbSocket socket)
        {
            Socket = socket;
            reader = new StreamReader(socket.GetShellStream());

            GetPrompt();
        }

        string CheckLine(TextWriter writer, bool newLine = true)
        {
            string line = lines[0];
            lines.RemoveAt(0);

            bool isPrompt = CheckPrompt(line);

            if (newLine && !isPrompt)
            {
                line += Environment.NewLine;
            }

            writer?.Write(line);
            return line;
        }

        void CheckAlive()
        {
            if (!Socket.Connected)
            {
                throw new SocketException((int)SocketError.NotConnected);
            }
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
                CheckAlive();

                int count = Socket.Available;

                if (count > 0)
                {
                    var resp = new byte[count];
                    Socket.Read(resp);
                    Invalidate();
                    string result = Encoding.ASCII.GetString(resp);

                    lines = result.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (!string.IsNullOrEmpty(command))
                    {
                        lines.Remove(command);
                    }

                    if (lines.Count == 0)
                    {
                        break;
                    }

                    return CheckLine(writer, lines.Count > 1);
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
                if (data != string.Empty && (!noPrompt || noPrompt && !validMatch))
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
            CheckAlive();
            this.command = command;
            string formedCommand = command + "\n";
            byte[] data = Encoding.ASCII.GetBytes(formedCommand);
            Socket.Send(data, 0, data.Length);
            if (command == "exit" && Access == ShellAccess.Adb)
            {
                Socket.Dispose();
            }
        }

        /// <inheritdoc/>
        public string Interact(string command, TextWriter writer = null)
        {
            // Clear pending data
            GetPrompt();

            // Send command
            SendCommand(command);

            // Receive data without prompt
            return ReadToEnd(true, writer);
        }

        /// <inheritdoc/>
        public string Interact(string command, IShellOutputReceiver[] receivers)
        {
            string result = Interact(command);

            foreach (var receiver in receivers)
            {
                foreach (var line in result.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    receiver.AddOutput(line);
                }

                receiver.Flush();
            }

            return result;
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
                if (Match.Groups["user"].Value == "#")
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

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            Socket.Dispose();
        }
    }
}
