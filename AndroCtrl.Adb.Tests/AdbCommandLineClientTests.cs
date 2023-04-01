using System;

using AndroCtrl.Protocols.AndroidDebugBridge.Exceptions;

using Xunit;

namespace AndroCtrl.Protocols.AndroidDebugBridge.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbCommandLineClient"/> class.
    /// </summary>
    public class AdbCommandLineClientTests
    {
        [Fact]
        public void GetVersionTest()
        {
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = new Version(1, 0, 41)
            };

            Assert.Equal(new Version(1, 0, 41), commandLine.GetVersion());
        }

        [Fact]
        public void GetVersionNullTest()
        {
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = null
            };
            Assert.Throws<AdbException>(() => commandLine.GetVersion());
        }

        [Fact]
        public void GetOutdatedVersionTest()
        {
            DummyAdbCommandLineClient commandLine = new()
            {
                Version = new Version(1, 0, 1)
            };

            Assert.Throws<AdbException>(() => commandLine.GetVersion());
        }

        [Fact]
        public void StartServerTest()
        {
            DummyAdbCommandLineClient commandLine = new();
            Assert.False(commandLine.ServerStarted);
            commandLine.StartServer();
            Assert.True(commandLine.ServerStarted);
        }
    }
}