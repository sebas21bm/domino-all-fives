using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class CredentialsTests
    {
        [Test]
        public void ForLogin_PaddedIdentifier_TrimsIdentifier()
        {
            // Arrange
            const string Identifier = "  player  ";
            // Act
            Credentials credentials = Credentials.ForLogin(Identifier, "secret");
            // Assert
            Assert.That(credentials.EmailOrUsername, Is.EqualTo("player"));
        }

        [Test]
        public void ForLogin_PaddedPassword_PreservesPassword()
        {
            // Arrange
            const string Password = "  secret  ";
            // Act
            Credentials credentials = Credentials.ForLogin("player", Password);
            // Assert
            Assert.That(credentials.Password, Is.EqualTo(Password));
        }

        [Test]
        public void Validate_CompleteCredentials_ReturnsNone()
        {
            // Arrange
            Credentials credentials = Credentials.ForLogin("player", "secret");
            // Act
            LoginStatus status = credentials.Validate();
            // Assert
            Assert.That(status, Is.EqualTo(LoginStatus.None));
        }

        [TestCase(null, "secret")]
        [TestCase("", "secret")]
        [TestCase("   ", "secret")]
        [TestCase("player", null)]
        [TestCase("player", "")]
        [TestCase("player", "   ")]
        public void Validate_IncompleteCredentials_ReturnsInvalidCredentials(
            string identifier,
            string password)
        {
            // Arrange
            Credentials credentials = Credentials.ForLogin(identifier, password);
            // Act
            LoginStatus status = credentials.Validate();
            // Assert
            Assert.That(status, Is.EqualTo(LoginStatus.InvalidCredentials));
        }
    }
}
