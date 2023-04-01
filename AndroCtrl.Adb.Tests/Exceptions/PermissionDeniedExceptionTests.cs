using SAPTeam.AndroCtrl.Adb.Exceptions;

using SAPTeam.AndroCtrl.Adb.Tests;

using Xunit;

namespace SAPTeam.AndroCtrl.Adb.Tests.Exceptions
{
    public class PermissionDeniedExceptionTests
    {
        [Fact]
        public void TestEmptyConstructor()
        {
            ExceptionTester<PermissionDeniedException>.TestEmptyConstructor(() => new PermissionDeniedException());
        }

        [Fact]
        public void TestMessageConstructor()
        {
            ExceptionTester<PermissionDeniedException>.TestMessageConstructor((message) => new PermissionDeniedException(message));
        }

        [Fact]
        public void TestMessageAndInnerConstructor()
        {
            ExceptionTester<PermissionDeniedException>.TestMessageAndInnerConstructor((message, inner) => new PermissionDeniedException(message, inner));
        }

#if !NETCOREAPP1_1
        [Fact]
        public void TestSerializationConstructor()
        {
            ExceptionTester<PermissionDeniedException>.TestSerializationConstructor((info, context) => new PermissionDeniedException(info, context));
        }
#endif
    }
}