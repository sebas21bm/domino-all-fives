using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;

namespace DominoAllFives.Server.AdditionalTest
{
    [TestFixture]
    public class SessionServiceTests
    {
        [Test]
        public void LoginWithAccount_NullCredentials_ReturnsInvalidCredentialsFault()
        {
            // Arrange
            DominoService service = new DominoService();
            // Act
            TestDelegate login = () => service.LoginWithAccount(null);
            // Assert
            Assert.That(
                login,
                Throws.TypeOf<FaultException<AccountIssue>>()
                    .With.Property("Detail").EqualTo(AccountIssue.InvalidCredentials));
        }

        [Test]
        public void LoginWithAccount_MissingPassword_ReturnsInvalidCredentialsFault()
        {
            // Arrange
            DominoService service = new DominoService();
            Credentials credentials = Credentials.ForLogin("player", null);
            // Act
            TestDelegate login = () => service.LoginWithAccount(credentials);
            // Assert
            Assert.That(
                login,
                Throws.TypeOf<FaultException<AccountIssue>>()
                    .With.Property("Detail").EqualTo(AccountIssue.InvalidCredentials));
        }
    }
}
