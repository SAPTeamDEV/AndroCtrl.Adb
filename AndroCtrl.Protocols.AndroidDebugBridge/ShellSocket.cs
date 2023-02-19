using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AndroCtrl.Protocols.AndroidDebugBridge
{
    /// <summary>
    /// Provides methods to interact with Adb shell session.
    /// </summary>
    public class ShellSocket
    {
        public IAdbSocket Socket { get; }

        public ShellSocket(IAdbSocket socket)
        {
            Socket = socket;
        }

        /// <summary>
        /// Reads all available data and converts it to string.
        /// </summary>
        /// <param name="blocking">
        /// determines wait for receiving data from socket.
        /// </param>
        /// <returns>
        /// a string created from read bytes.
        /// </returns>
        public string ReadAvailable(bool blocking)
        {
            while (true)
            {
                int count = Socket.Available;

                if (count > 0)
                {
                    var resp = new byte[count];
                    Socket.Read(resp);
                    return Encoding.ASCII.GetString(resp);
                }
                else if (blocking)
                {
                    Thread.Sleep(1);
                }
                else
                {
                    break;
                }
            }

            return string.Empty;
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
    }
}
