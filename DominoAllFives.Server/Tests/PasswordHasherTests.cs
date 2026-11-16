using System;
using DominoAllFives.BusinessLogic.Security;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class PasswordHasherTests
    {
        [Test]
        public void VerifyPassword_MatchingPassword_ReturnsTrue()
        {
            // Arrange
            string hash = PasswordHasher.HashPassword("secret");
            // Act
            bool matches = PasswordHasher.VerifyPassword("secret", hash);
            // Assert
            Assert.That(matches, Is.True);
        }

        [Test]
        public void VerifyPassword_DifferentPassword_ReturnsFalse()
        {
            // Arrange
            string hash = PasswordHasher.HashPassword("secret");
            // Act
            bool matches = PasswordHasher.VerifyPassword("different", hash);
            // Assert
            Assert.That(matches, Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("invalid")]
        [TestCase("OTHER$100000$c2FsdA==$aGFzaA==")]
        [TestCase("PBKDF2-SHA256$invalid$c2FsdA==$aGFzaA==")]
        [TestCase("PBKDF2-SHA256$100000$invalid$aGFzaA==")]
        public void VerifyPassword_InvalidStoredHash_ReturnsFalse(string storedHash)
        {
            // Arrange
            const string Password = "secret";
            // Act
            bool matches = PasswordHasher.VerifyPassword(Password, storedHash);
            // Assert
            Assert.That(matches, Is.False);
        }

        [Test]
        public void HashPassword_NullPassword_ThrowsArgumentNullException()
        {
            // Arrange
            string password = null;
            // Act
            TestDelegate hashPassword = () => PasswordHasher.HashPassword(password);
            // Assert
            Assert.Throws<ArgumentNullException>(hashPassword);
        }

        [TestCase("")]
        [TestCase("   ")]
        public void HashPassword_BlankPassword_ThrowsArgumentException(string password)
        {
            // Arrange
            string invalidPassword = password;
            // Act
            TestDelegate hashPassword = () => PasswordHasher.HashPassword(invalidPassword);
            // Assert
            Assert.Throws<ArgumentException>(hashPassword);
        }
    }
}
