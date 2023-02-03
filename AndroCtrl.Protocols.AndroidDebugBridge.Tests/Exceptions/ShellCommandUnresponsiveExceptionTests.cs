using AndroCtrl.Protocols.AndroidDebugBridge.Exceptions;

using Xunit;

namespace AndroCtrl.Protocols.AndroidDebugBridge.Tests.Exceptions;

public class ShellCommandUnresponsiveExceptionTests
{
    [Fact]
    public void TestEmptyConstructor()
    {
        ExceptionTester<ShellCommandUnresponsiveException>.TestEmptyConstructor(() => new ShellCommandUnresponsiveException());
    }

    [Fact]
    public void TestMessageConstructor()
    {
        ExceptionTester<ShellCommandUnresponsiveException>.TestMessageConstructor((message) => new ShellCommandUnresponsiveException(message));
    }

    [Fact]
    public void TestMessageAndInnerConstructor()
    {
        ExceptionTester<ShellCommandUnresponsiveException>.TestMessageAndInnerConstructor((message, inner) => new ShellCommandUnresponsiveException(message, inner));
    }
}
