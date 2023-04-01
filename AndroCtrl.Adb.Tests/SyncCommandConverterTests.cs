using System;

using Xunit;

namespace AndroCtrl.Protocols.AndroidDebugBridge.Tests
{
    public class SyncCommandConverterTests
    {
        [Fact]
        public void GetCommandNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => SyncCommandConverter.GetCommand(null));
        }

        [Fact]
        public void GetCommandInvalidNumberOfBytesTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SyncCommandConverter.GetCommand(Array.Empty<byte>()));
        }

        [Fact]
        public void GetCommandInvalidCommandTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SyncCommandConverter.GetCommand(new byte[] { (byte)'Q', (byte)'M', (byte)'T', (byte)'V' }));
        }

        [Fact]
        public void GetBytesInvalidCommandTest()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SyncCommandConverter.GetBytes((SyncCommand)99));
        }
    }
}