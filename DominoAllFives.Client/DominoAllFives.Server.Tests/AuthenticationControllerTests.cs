using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;
using DominoAllFives.BusinessLogic.Security;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.Server.Controllers;
using DominoAllFives.Server.Exceptions;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class AuthenticationControllerTests
    {
        private static readonly DateTime EvaluationTime = new DateTime(2026, 10, 22, 12, 0, 0, DateTimeKind.Utc);
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
                name => { lookupCount++; return null; }, () => EvaluationTime);

            AssertIssue(controller, LoginCredentials.ForLogin(identifier, password), AccountIssue.InvalidCredentials);
            Assert.That(lookupCount, Is.Zero);
        }

        [Test]
        public void LoginWithAccount_NullCredentials_RejectsBeforeLookup()
        {
            AuthenticationController controller = new AuthenticationController(
                name => throw new AssertionException("Unexpected lookup."), () => EvaluationTime);
            AssertIssue(controller, null, AccountIssue.InvalidCredentials);
        }

        [Test]
        public void LoginWithAccount_MissingPlayer_RejectsCredentials()
        {
            AssertIssue(ControllerFor(null), Credentials(), AccountIssue.InvalidCredentials);
        }

        [Test]
        public void LoginWithAccount_DeletedPlayer_RejectsCredentials()
        {
            Player player = PlayerWithAccount();
            player.IsDeleted = true;
            player.IsBanned = true;
            AssertIssue(ControllerFor(player), Credentials(), AccountIssue.InvalidCredentials);
        }

        [Test]
        public void LoginWithAccount_WrongPassword_DoesNotDiscloseRestrictions()
        {
            Player player = PlayerWithAccount();
            player.IsBanned = true;
            AssertIssue(ControllerFor(player), LoginCredentials.ForLogin("player", "wrong"),
                AccountIssue.InvalidCredentials);
        }

        [Test]
        public void LoginWithAccount_BannedPlayer_RejectsBeforeSuspension()
        {
            Player player = PlayerWithAccount();
            player.IsBanned = true;
            player.SuspensionUntil = EvaluationTime.AddDays(1);
            AssertIssue(ControllerFor(player), Credentials(), AccountIssue.Banned);
        }

        [TestCase(DateTimeKind.Utc)]
        [TestCase(DateTimeKind.Local)]
        [TestCase(DateTimeKind.Unspecified)]
        public void LoginWithAccount_ActiveSuspension_RejectsUsingStoredTimestampConvention(DateTimeKind kind)
        {
            Player player = PlayerWithAccount();
            DateTime boundary = kind == DateTimeKind.Utc ? EvaluationTime : EvaluationTime.ToLocalTime();
            player.SuspensionUntil = DateTime.SpecifyKind(boundary.AddMinutes(1), kind);
            AssertIssue(ControllerFor(player), Credentials(), AccountIssue.Suspended);
        }

        [TestCase(-1)]
        [TestCase(0)]
        public void LoginWithAccount_ExpiredSuspension_ReturnsAccount(int minutesFromNow)
        {
            Player player = PlayerWithAccount();
            player.SuspensionUntil = EvaluationTime.AddMinutes(minutesFromNow);
            AssertAccount(ControllerFor(player).LoginWithAccount(Credentials()), player);
        }

        [Test]
        public void LoginWithAccount_ValidCredentials_ReturnsAccountWithoutPassword()
        {
            Player player = PlayerWithAccount();
            string lookedUpIdentifier = null;
            AuthenticationController controller = new AuthenticationController(
                identifier => { lookedUpIdentifier = identifier; return player; }, () => EvaluationTime);

            PlayerAccount account = controller.LoginWithAccount(LoginCredentials.ForLogin("  player  ", "test-password"));
            AssertAccount(account, player);
            Assert.That(lookedUpIdentifier, Is.EqualTo("player"));
        }

        [TestCase(AccountIssue.InvalidCredentials)]
        [TestCase(AccountIssue.Banned)]
        [TestCase(AccountIssue.Suspended)]
        public void LoginWithAccount_BusinessRejection_TranslatesTypedFaultWithoutLogging(AccountIssue issue)
        {
            int diagnostics = 0;
            DominoService service = new DominoService(
                credentials => throw new AccountRuleException(issue), exception => diagnostics++);

            FaultException<AccountIssue> fault = Assert.Throws<FaultException<AccountIssue>>(
                () => service.LoginWithAccount(Credentials()));

            Assert.That(fault.Detail, Is.EqualTo(issue));
            Assert.That(diagnostics, Is.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void LoginWithAccount_PersistenceFailure_LogsOnceAndSanitizesFault(bool updateFailure)
        {
            Exception failure = updateFailure
                ? (Exception)new DbUpdateException("private database details")
                : new EntityException("private database details");
            int diagnostics = 0;
            Exception loggedException = null;
            DominoService service = new DominoService(credentials => throw failure,
                exception => { diagnostics++; loggedException = exception; });

            FaultException fault = Assert.Throws<FaultException>(() => service.LoginWithAccount(Credentials()));

            Assert.That(fault.GetType(), Is.EqualTo(typeof(FaultException)));
            Assert.That(fault.Message, Is.EqualTo("The service could not complete the operation."));
            Assert.That(diagnostics, Is.EqualTo(1));
            Assert.That(loggedException, Is.SameAs(failure));
        }

        [Test]
        public void LoginWithAccount_Success_ReturnsSameAccountWithoutLogging()
        {
            PlayerAccount account = ControllerFor(PlayerWithAccount()).LoginWithAccount(Credentials());
            DominoService service = new DominoService(credentials => account,
                exception => throw new AssertionException("Unexpected diagnostic."));
            Assert.That(service.LoginWithAccount(Credentials()), Is.SameAs(account));
        }

        [Test]
        public void RemoteControllers_AreOwnedOnlyByServerAssembly()
        {
            var serverAssembly = typeof(DominoService).Assembly;
            var businessAssembly = typeof(DominoAllFives.BusinessLogic.Controllers.RegistrationController).Assembly;
            Assert.That(typeof(AuthenticationController).Assembly, Is.SameAs(serverAssembly));
            Assert.That(typeof(RankingController).Assembly, Is.SameAs(serverAssembly));
            Assert.That(businessAssembly.GetType("DominoAllFives.BusinessLogic.Controllers.AuthenticationController"), Is.Null);
            Assert.That(businessAssembly.GetType("DominoAllFives.BusinessLogic.Controllers.RankingController"), Is.Null);
        }

        private Player PlayerWithAccount()
        {
            return new Player { IdPlayer = 7, Username = "player", PreferredLanguageId = "es-MX", PasswordHash = _passwordHash };
        }

        private static AuthenticationController ControllerFor(Player player)
        {
            return new AuthenticationController(identifier => player, () => EvaluationTime);
        }

        private static LoginCredentials Credentials()
        {
            return LoginCredentials.ForLogin("player", "test-password");
        }

        private static void AssertIssue(AuthenticationController controller, LoginCredentials credentials, AccountIssue issue)
        {
            AccountRuleException exception = Assert.Throws<AccountRuleException>(() => controller.LoginWithAccount(credentials));
            Assert.That(exception.Issue, Is.EqualTo(issue));
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
