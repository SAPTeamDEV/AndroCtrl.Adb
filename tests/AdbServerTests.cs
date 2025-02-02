﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using SAPTeam.AndroCtrl.Adb.Exceptions;
using SAPTeam.AndroCtrl.Adb.Interfaces;

using Moq;

using Xunit;

namespace SAPTeam.AndroCtrl.Adb.Tests
{
    public class AdbServerTests
    {
        private readonly Func<string, IAdbCommandLineClient> adbCommandLineClientFactory;
        private readonly DummyAdbSocket socket;
        private readonly DummyAdbCommandLineClient commandLineClient;
        private Func<EndPoint, IAdbSocket> adbSocketFactory;
        private AdbClient adbClient;
        private AdbServer adbServer;

        public AdbServerTests()
        {
            socket = new DummyAdbSocket();
            adbSocketFactory = (endPoint) => socket;

            commandLineClient = new DummyAdbCommandLineClient();
            adbCommandLineClientFactory = (version) => commandLineClient;

            adbClient = new AdbClient(AdbClient.DefaultEndPoint, adbSocketFactory);
            adbServer = new AdbServer(adbClient, adbCommandLineClientFactory);
        }

        [Fact]
        public void GetStatusNotRunningTest()
        {
            Mock<IAdbClient> adbClientMock = new Mock<IAdbClient>();
            adbClientMock.Setup(c => c.GetAdbVersion())
                .Throws(new SocketException(AdbServer.ConnectionRefused));

            AdbServer adbServer = new AdbServer(adbClientMock.Object, adbCommandLineClientFactory);

            AdbServerStatus status = adbServer.GetStatus();
            Assert.False(status.IsRunning);
            Assert.Null(status.Version);
        }

        [Fact]
        public void GetStatusRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0020");

            AdbServerStatus status = adbServer.GetStatus();

            Assert.Empty(socket.Responses);
            Assert.Empty(socket.ResponseMessages);
            Assert.Single(socket.Requests);
            Assert.Equal("host:version", socket.Requests[0]);

            Assert.True(status.IsRunning);
            Assert.Equal(new Version(1, 0, 32), status.Version);
        }

        [Fact]
        public void GetStatusOtherSocketExceptionTest()
        {
            adbSocketFactory = (endPoint) =>
            {
                throw new SocketException();
            };

            adbClient = new AdbClient(AdbClient.DefaultEndPoint, adbSocketFactory);
            adbServer = new AdbServer(adbClient, adbCommandLineClientFactory);

            Assert.Throws<SocketException>(() => adbServer.GetStatus());
        }

        [Fact]
        public void GetStatusOtherExceptionTest()
        {
            adbSocketFactory = (endPoint) =>
            {
                throw new Exception();
            };

            adbClient = new AdbClient(AdbClient.DefaultEndPoint, adbSocketFactory);
            adbServer = new AdbServer(adbClient, adbCommandLineClientFactory);

            Assert.Throws<Exception>(() => adbServer.GetStatus());
        }

        /*
        [Fact]
        public void StartServerAlreadyRunningTest()
        {
            commandLineClient.Version = new Version(1, 0, 41);
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0020");

            StartServerResult result = adbServer.StartServer(null, false);

            Assert.Equal(StartServerResult.AlreadyRunning, result);

            Assert.Single(socket.Requests);
            Assert.Equal("host:version", socket.Requests[0]);
        }
        */

        [Fact]
        public void StartServerOutdatedRunningNoExecutableTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0010");

            Assert.Throws<AdbException>(() => adbServer.StartServer(null, false));
        }

        [Fact]
        public void StartServerNotRunningNoExecutableTest()
        {
            adbSocketFactory = (endPoint) =>
            {
                throw new SocketException(AdbServer.ConnectionRefused);
            };

            adbClient = new AdbClient(AdbClient.DefaultEndPoint, adbSocketFactory);
            adbServer = new AdbServer(adbClient, adbCommandLineClientFactory);

            Assert.Throws<AdbException>(() => adbServer.StartServer(null, false));
        }

        [Fact]
        public void StartServerOutdatedRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("0010");

            commandLineClient.Version = new Version(1, 0, 41);

            Assert.False(commandLineClient.ServerStarted);
            _ = adbServer.StartServer(ServerName, false);

            Assert.True(commandLineClient.ServerStarted);

            Assert.Equal(2, socket.Requests.Count);
            Assert.Equal("host:version", socket.Requests[0]);
            Assert.Equal("host:kill", socket.Requests[1]);
        }

        [Fact]
        public void StartServerNotRunningTest()
        {
            adbSocketFactory = (endPoint) =>
            {
                throw new SocketException(AdbServer.ConnectionRefused);
            };

            adbClient = new AdbClient(AdbClient.DefaultEndPoint, adbSocketFactory);
            adbServer = new AdbServer(adbClient, adbCommandLineClientFactory);

            commandLineClient.Version = new Version(1, 0, 41);

            Assert.False(commandLineClient.ServerStarted);

            StartServerResult result = adbServer.StartServer(ServerName, false);

            Assert.True(commandLineClient.ServerStarted);
        }

        [Fact]
        public void StartServerIntermediateRestartRequestedRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("001f");

            commandLineClient.Version = new Version(1, 0, 41);

            Assert.False(commandLineClient.ServerStarted);
            _ = adbServer.StartServer(ServerName, true);

            Assert.True(commandLineClient.ServerStarted);

            Assert.Equal(2, socket.Requests.Count);
            Assert.Equal("host:version", socket.Requests[0]);
            Assert.Equal("host:kill", socket.Requests[1]);
        }

        /*
        [Fact]
        public void StartServerIntermediateRestartNotRequestedRunningTest()
        {
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.ResponseMessages.Enqueue("001f");

            commandLineClient.Version = new Version(1, 0, 41);

            Assert.False(commandLineClient.ServerStarted);
            _ = adbServer.StartServer(ServerName, false);

            Assert.False(commandLineClient.ServerStarted);

            Assert.Single(socket.Requests);
            Assert.Equal("host:version", socket.Requests[0]);
        }
        */

        [Fact]
        public void ConstructorAdbClientNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => new AdbServer(null, adbCommandLineClientFactory));
        }

        private static string ServerName => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "adb.exe" : "adb";
    }
}