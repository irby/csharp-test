using Authentication.Core.Utilities;
using NUnit.Framework;

namespace Authentication.Core.Tests.Utilities
{
    [TestFixture]
    public class HashUtilTests
    {
        [Test]
        public void HashPassword_WhenProvidedAStringAndSalt_HashesPassword()
        {
            var password = "HelloWorld";
            var salt = HashUtil.GetSalt();

            var result = HashUtil.HashPassword(password, salt);
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }
        
        [Test]
        public void ValidatePassword_WhenProvidedCorrectPassword_ReturnsTrue()
        {
            var password = "HelloWorld";
            var salt = HashUtil.GetSalt();
            var hash = HashUtil.HashPassword(password, salt);

            var result = HashUtil.Validate(password, hash);
            Assert.True(result);
        }
        
        [Test]
        public void ValidatePassword_WhenProvidedIncorrectPassword_ReturnsFalse()
        {
            var password = "HelloWorld";
            var salt = HashUtil.GetSalt();
            var hash = HashUtil.HashPassword(password, salt);

            var result = HashUtil.Validate("helloworld", hash);
            Assert.False(result);
        }
    }
}