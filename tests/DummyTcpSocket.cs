using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using SAPTeam.AndroCtrl.Adb.Interfaces;

namespace SAPTeam.AndroCtrl.Adb.Tests
{
    internal class DummyTcpSocket : ITcpSocket
    {
        /// <summary>
        /// The stream from which the <see cref="DummyTcpSocket"/> reads.
        /// </summary>
        public MemoryStream InputStream
        { get; set; } = new MemoryStream();

        /// <summary>
        /// The stream to which the <see cref="DummyTcpSocket"/> writes.
        /// </summary>
        public MemoryStream OutputStream
        { get; set; } = new MemoryStream();

        public bool Connected
        { get; set; } = true;

        public int ReceiveBufferSize
        { get; set; } = 1024;

        public int Available => throw new NotImplementedException();

        public void Close()
        {
            Connected = false;
        }

        public void Connect(EndPoint endPoint)
        {
            Connected = true;
        }

        public void Dispose()
        {
            Connected = false;
        }

        public Stream GetStream()
        {
            return OutputStream;
        }

        public int Receive(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return InputStream.Read(buffer, 0, size);
        }

        public Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            int value = InputStream.Read(buffer, offset, size);
            return Task.FromResult(value);
        }

        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            OutputStream.Write(buffer, offset, size);
            return size;
        }

        public byte[] GetBytesSent()
        {
            return OutputStream.ToArray();
        }

        public void Reconnect()
        {
            throw new NotImplementedException();
        }
    }
}