using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;
using DominoAllFives.BusinessLogic.Security;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Models;
using Player = DominoAllFives.DataAccess.Models.Player;
using NUnit.Framework;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class AuthenticationControllerTests
    {
        private static readonly DateTime evaluationTime = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
        private string _passwordHash;

        [OneTimeSetUp]
        public void CreatePasswordHash()
        {
            _passwordHash = PasswordHasher.HashPassword("test-password");
        }

        [TestCase(null, "password")]
        [TestCase("", "password")]
        [TestCase("   ", "password")]
        [TestCase("player", null)]
        [TestCase("player", "")]
        [TestCase("player", "   ")]
        public void LoginWithAccount_MissingCredentials_RejectsBeforeLookup(string identifier, string password)
        {
            int lookupCount = 0;
            AuthenticationController controller = new AuthenticationController(
                name => { lookupCount++; return null; }, () => evaluationTime);

            AssertIssue(controller, Credentials.ForLogin(identifier, password), AccountIssue.InvalidCredentials);
            Assert.That(lookupCount, Is.Zero);
        }

        [Test]
        public void LoginWithAccount_NullCredentials_RejectsBeforeLookup()
        {
            AuthenticationController controller = new AuthenticationController(
                name => throw new AssertionException("Unexpected lookup."), () => evaluationTime);
            AssertIssue(controller, null, AccountIssue.InvalidCredentials);
        }

        [Test]
        public void LoginWithAccount_MissingPlayer_RejectsCredentials()
        {
            AssertIssue(ControllerFor(null), CreateCredentials(), AccountIssue.InvalidCredentials);
        }

        [Test]
        public void LoginWithAccount_DeletedPlayer_RejectsCredentials()
        {
            Player player = PlayerWithAccount();
            player.IsDeleted = true;
            player.IsBanned = true;
            AssertIssue(ControllerFor(player), CreateCredentials(), AccountIssue.InvalidCredentials);
        }

        [Test]
        public void LoginWithAccount_WrongPassword_DoesNotDiscloseRestrictions()
        {
            Player player = PlayerWithAccount();
            player.IsBanned = true;
            AssertIssue(ControllerFor(player), Credentials.ForLogin("player", "wrong"),
                AccountIssue.InvalidCredentials);
        }

        [Test]
        public void LoginWithAccount_BannedPlayer_RejectsBeforeSuspension()
        {
            Player player = PlayerWithAccount();
            player.IsBanned = true;
            player.SuspensionUntil = evaluationTime.AddDays(1);
            AssertIssue(ControllerFor(player), CreateCredentials(), AccountIssue.Banned);
        }

        [TestCase(DateTimeKind.Utc)]
        [TestCase(DateTimeKind.Local)]
        [TestCase(DateTimeKind.Unspecified)]
        public void LoginWithAccount_ActiveSuspension_RejectsUsingStoredTimestampConvention(DateTimeKind kind)
        {
            Player player = PlayerWithAccount();
            DateTime boundary = kind == DateTimeKind.Utc ? evaluationTime : evaluationTime.ToLocalTime();
            player.SuspensionUntil = DateTime.SpecifyKind(boundary.AddMinutes(1), kind);
            AssertIssue(ControllerFor(player), CreateCredentials(), AccountIssue.Suspended);
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void LoginWithAccount_ExpiredSuspension_ReturnsAccount(int minutesFromNow)
        {
            Player player = PlayerWithAccount();
            player.SuspensionUntil = evaluationTime.AddMinutes(minutesFromNow);
            AssertAccount(ControllerFor(player).LoginWithAccount(CreateCredentials()), player);
        }

        [Test]
        public void LoginWithAccount_ValidCredentials_ReturnsAccountWithoutPassword()
        {
            Player player = PlayerWithAccount();
            string lookedUpIdentifier = null;
            AuthenticationController controller = new AuthenticationController(
                identifier => { lookedUpIdentifier = identifier; return player; }, () => evaluationTime);

            PlayerAccount account = controller.LoginWithAccount(Credentials.ForLogin("  player  ", "test-password"));
            AssertAccount(account, player);
            Assert.That(lookedUpIdentifier, Is.EqualTo("player"));
        }

        [TestCase(AccountIssue.InvalidCredentials)]
        [TestCase(AccountIssue.Banned)]
        [TestCase(AccountIssue.Suspended)]
        public void LoginWithAccount_BusinessRejection_LogsWarningAndTranslatesTypedFault(AccountIssue issue)
        {
            // Arrange
            TestLogger<DominoService> logger = new TestLogger<DominoService>();
            AccountRuleException rejection = new AccountRuleException(issue);
            DominoService service = new DominoService(
                credentials => throw rejection, logger);

            // Act

            FaultException<AccountIssue> fault = Assert.Throws<FaultException<AccountIssue>>(
                () => service.LoginWithAccount(CreateCredentials()));

            // Assert
            Assert.That(fault.Detail, Is.EqualTo(issue));
            Assert.That(logger.Levels, Is.EqualTo(new[] { LogLevel.Warning }));
            Assert.That(logger.Exceptions[0], Is.SameAs(rejection));
            Assert.That(logger.Messages[0], Does.Contain(issue.ToString()));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LoginWithAccount_PersistenceFailure_LogsOnceAndSanitizesFault(bool updateFailure)
        {
            // Arrange
            Exception failure = updateFailure
                ? (Exception)new DbUpdateException("private database details")
                : new EntityException("private database details");
            TestLogger<DominoService> logger = new TestLogger<DominoService>();
            DominoService service = new DominoService(credentials => throw failure, logger);

            // Act
            FaultException fault = Assert.Throws<FaultException>(() => service.LoginWithAccount(CreateCredentials()));

            // Assert
            Assert.That(fault.GetType(), Is.EqualTo(typeof(FaultException)));
            Assert.That(fault.Message, Is.EqualTo("The service could not complete the operation."));
            Assert.That(logger.Levels, Is.EqualTo(new[] { LogLevel.Error }));
            Assert.That(logger.Exceptions[0], Is.SameAs(failure));
        }

        [Test]
        public void LoginWithAccount_Success_ReturnsSameAccountWithoutLogging()
        {
            // Arrange
            PlayerAccount account = ControllerFor(PlayerWithAccount()).LoginWithAccount(CreateCredentials());
            TestLogger<DominoService> logger = new TestLogger<DominoService>();
            DominoService service = new DominoService(credentials => account, logger);

            // Act
            PlayerAccount result = service.LoginWithAccount(CreateCredentials());

            // Assert
            Assert.That(result, Is.SameAs(account));
            Assert.That(logger.Levels, Is.Empty);
        }

        [Test]
        public void RemoteControllers_AreOwnedByBusinessLogicAssembly()
        {
            var businessAssembly = typeof(DominoAllFives.BusinessLogic.Controllers.RegistrationController).Assembly;
            Assert.That(typeof(AuthenticationController).Assembly, Is.SameAs(businessAssembly));
            Assert.That(typeof(RankingController).Assembly, Is.SameAs(businessAssembly));
            Assert.That(typeof(LobbyController).Assembly, Is.SameAs(businessAssembly));
        }

        private Player PlayerWithAccount()
        {
            return new Player { IdPlayer = 7, Username = "player", PreferredLanguageId = "es-MX", PasswordHash = _passwordHash };
        }

        private static AuthenticationController ControllerFor(Player player)
        {
            return new AuthenticationController(identifier => player, () => evaluationTime);
        }

        private static Credentials CreateCredentials()
        {
            return Credentials.ForLogin("player", "test-password");
        }

        private static void AssertIssue(AuthenticationController controller, Credentials credentials, AccountIssue issue)
        {
            AccountRuleException ex = Assert.Throws<AccountRuleException>(() => controller.LoginWithAccount(credentials));
            Assert.That(ex.Issue, Is.EqualTo(issue));
        }

        private static void AssertAccount(PlayerAccount account, Player player)
        {
            Assert.That(account.PlayerId, Is.EqualTo(player.IdPlayer));
            Assert.That(account.Username, Is.EqualTo(player.Username));
            Assert.That(account.PreferredLanguageId, Is.EqualTo(player.PreferredLanguageId));
            Assert.That(account.AccessStatus.IsBanned, Is.False);
            Assert.That(account.AccessStatus.IsDeleted, Is.False);
            Assert.That(account.AccessStatus.SuspensionUntil, Is.EqualTo(player.SuspensionUntil));
        }
    }
}
