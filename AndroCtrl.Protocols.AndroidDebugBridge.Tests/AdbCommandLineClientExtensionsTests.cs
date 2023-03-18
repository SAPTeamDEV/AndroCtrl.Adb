using System;
using System.IO;

using AndroCtrl.Protocols.AndroidDebugBridge.Interfaces;

using Moq;

using Xunit;

namespace AndroCtrl.Protocols.AndroidDebugBridge.Tests;

public class AdbCommandLineClientExtensionsTests
{
    [Fact]
    public void EnsureIsValidAdbFileNullValueTest()
    {
        Assert.Throws<ArgumentNullException>(() => AdbCommandLineClientExtensions.EnsureIsValidAdbFile(null, "adb.exe"));
    }

    [Fact]
    public void EnsureIsValidAdbFileInvalidFileTest()
    {
        Mock<IAdbCommandLineClient> clientMock = new Mock<IAdbCommandLineClient>();
        clientMock.Setup(c => c.IsValidAdbFile(It.IsAny<string>())).Returns(false);

        IAdbCommandLineClient client = clientMock.Object;

        Assert.Throws<FileNotFoundException>(() => client.EnsureIsValidAdbFile("xyz.exe"));
    }
}
