using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class AuthenticationControllerTests
    {
        [Test]
        public void LoginWithAccount_NullCredentials_ThrowsInvalidCredentials()
        {
            // Arrange
            AuthenticationController controller = new AuthenticationController();
            // Act
            TestDelegate login = () => controller.LoginWithAccount(null);
            // Assert
            Assert.That(
                login,
                Throws.TypeOf<AccountRuleException>()
                    .With.Property("Issue").EqualTo(AccountIssue.InvalidCredentials));
        }

        [Test]
        public void LoginWithAccount_MissingPassword_ThrowsInvalidCredentials()
        {
            // Arrange
            AuthenticationController controller = new AuthenticationController();
            Credentials credentials = Credentials.ForLogin("player", null);
            // Act
            TestDelegate login = () => controller.LoginWithAccount(credentials);
            // Assert
            Assert.That(
                login,
                Throws.TypeOf<AccountRuleException>()
                    .With.Property("Issue").EqualTo(AccountIssue.InvalidCredentials));
        }
    }
}
