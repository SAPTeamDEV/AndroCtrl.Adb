using System.IO;

namespace AndroCtrl.Protocols.AndroidDebugBridge.Interfaces
{
    /// <summary>
    /// Provides Interface for interact with Adb shell session.
    /// </summary>
    public interface IShellSocket
    {
        /// <summary>
        /// Gets current session access level.
        /// </summary>
        ShellAccess Access { get; }

        /// <summary>
        /// Represents current directory of this session.
        /// </summary>
        string CurrentDirectory { get; }

        /// <summary>
        /// Contains last Console prompt.
        /// Note: this message Might be outdated, for Fresh console message use <see cref="GetPrompt"/>
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Underlying <see cref="AdbSocket"/> instance.
        /// </summary>
        IAdbSocket Socket { get; }

        /// <summary>
        /// Reads console prompt and returns it, if pending data is available ignores them and wait until receives prompt message.
        /// </summary>
        /// <returns>
        /// Console prompt message.
        /// </returns>
        string GetPrompt(bool invalidation);

        /// <summary>
        /// Sends a command and wait for Receiving data.
        /// </summary>
        /// <param name="command">
        /// a shell command without LF.
        /// </param>
        /// <param name="writer">
        /// An instance of <see cref="TextWriter"/> for writing data to it.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> that contains response without prompt.
        /// </returns>
        string Interact(string command, TextWriter writer);

        /// <summary>
        /// Reads all available data and converts it to string.
        /// </summary>
        /// <param name="wait">
        /// Determines wait for receiving data from socket.
        /// </param>
        /// <param name="writer">
        /// An instance of <see cref="TextWriter"/> for writing data to it.
        /// </param>
        /// <returns>
        /// a string created from read bytes.
        /// </returns>
        string ReadAvailable(bool wait = false, TextWriter writer = null);

        /// <summary>
        /// Reads all data until reach to end of data.
        /// </summary>
        /// <param name="noPrompt">
        /// Determines that console prompt included with response or not.
        /// </param>
        /// <param name="writer">
        /// An instance of <see cref="TextWriter"/> for writing data to it.
        /// </param>
        /// <returns>
        /// A string containing all received data.
        /// </returns>
        string ReadToEnd(bool noPrompt = false, TextWriter writer = null);

        /// <summary>
        /// Formats and converts command to ASCII encoding and send it.
        /// </summary>
        /// <param name="command">
        /// a shell command without EL.
        /// </param>
        void SendCommand(string command);
    }
}