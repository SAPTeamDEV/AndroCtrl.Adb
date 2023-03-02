using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AndroCtrl.Protocols.AndroidDebugBridge
{
    /// <summary>
    /// Provides methods to interact with Adb shell session.
    /// </summary>
    public class ShellSocket
    {
        readonly Regex Regex = new(@"(?<num>[1-9]*)\W*\b(?<host>\w+):(?<directory>.*)\s(?<user>\$|#) $");
        Match Match;
        bool validMatch;

        string message;
        bool showNsg;

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

        public IAdbSocket Socket { get; }

        public string CurrentDirectory => Match.Groups["directory"].Value;

        public ShellAccess Access { get; private set; }

        public ShellSocket(IAdbSocket socket)
        {
            Socket = socket;

            GetPrompt();
        }

        /// <summary>
        /// Reads all available data and converts it to string.
        /// </summary>
        /// <param name="wait">
        /// Determines wait for receiving data from socket.
        /// </param>
        /// <param name="stream">
        /// An instance of <see cref="StreamWriter"/> for writing data to it.
        /// </param>
        /// <returns>
        /// a string created from read bytes.
        /// </returns>
        public string ReadAvailable(bool wait = false, StreamWriter stream = null)
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

                    stream?.Write(result);
                    return result;
                }
                else if (!wait)
                {
                    break;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Reads all data until reach to end of data.
        /// </summary>
        /// <param name="stream">
        /// An instance of <see cref="StreamWriter"/> for writing data to it.
        /// </param>
        /// <returns>
        /// A string containing all received data.
        /// </returns>
        public string ReadToEnd(StreamWriter stream = null)
        {
            string result = "";

            while (true)
            {
                var data = ReadAvailable(stream: stream);
                if (data != string.Empty)
                {
                    result += data;
                }

                if (validMatch && Match.Success)
                {
                    break;
                }
            }

            stream?.Flush();
            return result;
        }

        /// <summary>
        /// Formats and converts command to ASCII encoding and send it.
        /// </summary>
        /// <param name="command">
        /// a shell command without EL.
        /// </param>
        public void SendCommand(string command)
        {
            string formedCommand = command + "\n";
            byte[] data = Encoding.ASCII.GetBytes(formedCommand);
            Socket.Send(data, 0, data.Length);
        }

        /// <summary>
        /// Reads console prompt and returns it, if pending data is available ignores them and wait until receives prompt message.
        /// </summary>
        /// <returns>
        /// Console prompt message.
        /// </returns>
        public string GetPrompt()
        {
            if (showNsg && Socket.Available == 0)
            {
                return Message;
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
