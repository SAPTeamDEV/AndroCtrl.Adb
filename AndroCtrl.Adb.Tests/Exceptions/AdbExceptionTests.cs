using SAPTeam.AndroCtrl.Adb.Exceptions;

using SAPTeam.AndroCtrl.Adb.Tests;

using Xunit;

namespace SAPTeam.AndroCtrl.Adb.Tests.Exceptions
{
    public class AdbExceptionTests
    {
        [Fact]
        public void TestEmptyConstructor()
        {
            ExceptionTester<AdbException>.TestEmptyConstructor(() => new AdbException());
        }

        [Fact]
        public void TestMessageConstructor()
        {
            ExceptionTester<AdbException>.TestMessageConstructor((message) => new AdbException(message));
        }

        [Fact]
        public void TestMessageAndInnerConstructor()
        {
            ExceptionTester<AdbException>.TestMessageAndInnerConstructor((message, inner) => new AdbException(message, inner));
        }

#if !NETCOREAPP1_1
        [Fact]
        public void TestSerializationConstructor()
        {
            ExceptionTester<AdbException>.TestSerializationConstructor((info, context) => new AdbException(info, context));
        }
#endif
    }
}