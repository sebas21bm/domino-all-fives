using System;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Linq;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    public class DomainContractTests
    {
        [Test]
        public void LoginCredentials_ValidInput_NormalizesOnlyIdentifier()
        {
            // Arrange.
            const string password = "  existing-password  ";

            // Act.
            LoginCredentials credentials = LoginCredentials.ForLogin("  player  ", password);

            // Assert.
            Assert.That(credentials.EmailOrUsername, Is.EqualTo("player"));
            Assert.That(credentials.Password, Is.EqualTo(password));
            Assert.That(credentials.EnsureValid(), Is.EqualTo(LoginFailureReason.None));
        }

        [TestCase(null, "password")]
        [TestCase(" ", "password")]
        [TestCase("player", null)]
        [TestCase("player", " ")]
        public void LoginCredentials_BlankInput_PreservesInvalidCredentialsReason(
            string identifier, string password)
        {
            // Arrange and act.
            LoginFailureReason reason = LoginCredentials.ForLogin(identifier, password).EnsureValid();

            // Assert.
            Assert.That(reason, Is.EqualTo(LoginFailureReason.InvalidCredentials));
        }

        [TestCase("x")]
        [TestCase("old-password")]
        public void LoginCredentials_ExistingPassword_DoesNotApplyRegistrationPolicy(string password)
        {
            // Arrange and act.
            LoginCredentials credentials = LoginCredentials.ForLogin("player", password);

            // Assert.
            Assert.That(credentials.Password, Is.EqualTo(password));
        }

        [Test]
        public void PlayerAccount_StoredIdentity_HasNoCredentialOrAuthenticationFlag()
        {
            // Arrange.
            AccountAccessStatus status = AccountAccessStatus.FromStoredState(false, false, null);

            // Act.
            PlayerAccount account = PlayerAccount.FromStoredAccount(7, "player", "pt-BR", status);
            string[] propertyNames = typeof(PlayerAccount).GetProperties()
                .Select(property => property.Name).ToArray();

            // Assert.
            Assert.That(account.PlayerId, Is.EqualTo(7));
            Assert.That(account.Username, Is.EqualTo("player"));
            Assert.That(account.PreferredLanguageId, Is.EqualTo("pt-BR"));
            Assert.That(propertyNames, Does.Not.Contain("Password"));
            Assert.That(propertyNames, Does.Not.Contain("PasswordHash"));
            Assert.That(propertyNames, Does.Not.Contain("EmailOrUsername"));
            Assert.That(propertyNames, Does.Not.Contain("IsAuthenticated"));
            Assert.That(typeof(PlayerAccount).GetProperties()
                .Any(property => property.GetSetMethod() != null), Is.False);
        }

        [TestCase(true, true, LoginFailureReason.InvalidCredentials)]
        [TestCase(false, true, LoginFailureReason.Banned)]
        public void AccountAccessStatus_Restrictions_PreserveFailurePrecedence(
            bool deleted, bool banned, LoginFailureReason expected)
        {
            // Arrange.
            DateTime utcNow = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
            AccountAccessStatus status = AccountAccessStatus.FromStoredState(
                deleted, banned, utcNow.AddHours(1));

            // Act.
            LoginFailureReason reason = status.EnsureCanLogin(utcNow);

            // Assert.
            Assert.That(reason, Is.EqualTo(expected));
        }

        [TestCase(-1, true)]
        [TestCase(0, true)]
        [TestCase(1, false)]
        public void AccountAccessStatus_ExpiryBoundary_PreservesStrictComparison(
            int secondsFromExpiry, bool allowed)
        {
            // Arrange.
            DateTime expiry = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
            AccountAccessStatus status = AccountAccessStatus.FromStoredState(false, false, expiry);
            DateTime evaluationTime = expiry.AddSeconds(-secondsFromExpiry);

            // Act and assert.
            if (allowed)
            {
                Assert.That(status.EnsureCanLogin(evaluationTime), Is.EqualTo(LoginFailureReason.None));
            }
            else
            {
                Assert.That(status.EnsureCanLogin(evaluationTime), Is.EqualTo(LoginFailureReason.Suspended));
            }
        }

        [TestCase("", "bad-email", "x", RegistrationFailureReason.InvalidUsername)]
        [TestCase("player", "bad-email", "x", RegistrationFailureReason.InvalidEmail)]
        [TestCase("player", "player@gmail.com", "x", RegistrationFailureReason.InvalidPassword)]
        public void AccountRegistration_MultipleInvalidFields_PreservesValidationOrder(
            string username, string email, string password, RegistrationFailureReason expected)
        {
            // Arrange and act.
            RegistrationFailureReason reason = AccountRegistration.Create(username, email, password, "es-MX").EnsureValid();

            // Assert.
            Assert.That(reason, Is.EqualTo(expected));
        }

        [Test]
        public void AccountRegistration_ValidValues_PreservesStringsAndLanguage()
        {
            // Arrange.
            const string username = " player ";
            const string email = "player@gmail.com";
            const string password = "Test-Password1!";

            // Act.
            AccountRegistration registration = AccountRegistration.Create(
                username, email, password, "pt-BR");

            // Assert.
            Assert.That(registration.Username, Is.EqualTo(username));
            Assert.That(registration.Email, Is.EqualTo(email));
            Assert.That(registration.Password, Is.EqualTo(password));
            Assert.That(registration.PreferredLanguageId, Is.EqualTo("pt-BR"));
        }

        [Test]
        public void OperationResults_Success_PreservesAccountAndHasNoFailure()
        {
            PlayerAccount account = PlayerAccount.FromStoredAccount(7, "player", "es-MX",
                AccountAccessStatus.FromStoredState(false, false, null));
            LoginResult login = LoginResult.Success(account);
            RegistrationResult registration = RegistrationResult.Success(account);
            Assert.That(login.Account, Is.SameAs(account));
            Assert.That(login.Reason, Is.EqualTo(LoginFailureReason.None));
            Assert.That(registration.Account, Is.SameAs(account));
            Assert.That(registration.Reason, Is.EqualTo(RegistrationFailureReason.None));
            Assert.Throws<ArgumentNullException>(() => LoginResult.Success(null));
            Assert.Throws<ArgumentNullException>(() => RegistrationResult.Success(null));
        }

        [TestCase(LoginFailureReason.InvalidCredentials)]
        [TestCase(LoginFailureReason.Banned)]
        [TestCase(LoginFailureReason.Suspended)]
        public void LoginResult_Rejection_PreservesReasonWithoutAccount(LoginFailureReason reason)
        {
            LoginResult result = LoginResult.Rejected(reason);
            Assert.That(result.Account, Is.Null);
            Assert.That(result.Reason, Is.EqualTo(reason));
        }

        [TestCase(LoginFailureReason.ServiceUnavailable)]
        [TestCase(LoginFailureReason.None)]
        [TestCase((LoginFailureReason)999)]
        public void LoginResult_InvalidRejection_CannotBeConstructed(LoginFailureReason reason)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => LoginResult.Rejected(reason));
        }

        [TestCase(RegistrationFailureReason.InvalidUsername)]
        [TestCase(RegistrationFailureReason.InvalidEmail)]
        [TestCase(RegistrationFailureReason.InvalidPassword)]
        [TestCase(RegistrationFailureReason.UsernameAlreadyExists)]
        [TestCase(RegistrationFailureReason.EmailAlreadyExists)]
        public void RegistrationResult_Rejection_PreservesReasonWithoutAccount(RegistrationFailureReason reason)
        {
            RegistrationResult result = RegistrationResult.Rejected(reason);
            Assert.That(result.Account, Is.Null);
            Assert.That(result.Reason, Is.EqualTo(reason));
        }

        [TestCase(RegistrationFailureReason.InvalidRegistrationData)]
        [TestCase(RegistrationFailureReason.None)]
        [TestCase(RegistrationFailureReason.ServiceUnavailable)]
        [TestCase((RegistrationFailureReason)999)]
        public void RegistrationResult_InvalidRejection_CannotBeConstructed(RegistrationFailureReason reason)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RegistrationResult.Rejected(reason));
        }

        [Test]
        public void Login_InvalidCredentials_RejectsWithoutOpeningDatabase()
        {
            AuthenticationController controller = new AuthenticationController(
                NullLogger<AuthenticationController>.Instance);
            LoginResult result = controller.Login(LoginCredentials.ForLogin("", ""));
            Assert.That(result.Account, Is.Null);
            Assert.That(result.Reason, Is.EqualTo(LoginFailureReason.InvalidCredentials));
            Assert.Throws<ArgumentNullException>(() => controller.Login(null));
        }

        [TestCase("", "bad-email", "x", RegistrationFailureReason.InvalidUsername)]
        [TestCase("player", "bad-email", "x", RegistrationFailureReason.InvalidEmail)]
        [TestCase("player", "player@gmail.com", "x", RegistrationFailureReason.InvalidPassword)]
        public void Register_InvalidInput_RejectsBeforeOpeningDatabase(
            string username, string email, string password, RegistrationFailureReason expected)
        {
            RegistrationController controller = new RegistrationController(
                NullLogger<RegistrationController>.Instance);
            RegistrationResult result = controller.Register(
                AccountRegistration.Create(username, email, password, "es-MX"));
            Assert.That(result.Account, Is.Null);
            Assert.That(result.Reason, Is.EqualTo(expected));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void OperationResults_DataContractRoundTrip_PreservesAccountOrRejection(bool rejected)
        {
            PlayerAccount account = PlayerAccount.FromStoredAccount(7, "player", "pt-BR",
                AccountAccessStatus.FromStoredState(false, false, null));
            LoginResult login = RoundTrip(rejected ? LoginResult.Rejected(LoginFailureReason.Banned)
                : LoginResult.Success(account));
            RegistrationResult registration = RoundTrip(rejected
                ? RegistrationResult.Rejected(RegistrationFailureReason.UsernameAlreadyExists)
                : RegistrationResult.Success(account));
            Assert.That(login.Reason, Is.EqualTo(rejected ? LoginFailureReason.Banned : LoginFailureReason.None));
            Assert.That(registration.Reason, Is.EqualTo(rejected
                ? RegistrationFailureReason.UsernameAlreadyExists : RegistrationFailureReason.None));
            if (rejected)
            {
                Assert.That(login.Account, Is.Null);
                Assert.That(registration.Account, Is.Null);
            }
            else
            {
                Assert.That(login.Account.PlayerId, Is.EqualTo(7));
                Assert.That(registration.Account.Username, Is.EqualTo("player"));
                Assert.That(login.Account.PreferredLanguageId, Is.EqualTo("pt-BR"));
                Assert.That(login.Account.EnsureCanLogin(DateTime.UtcNow), Is.EqualTo(LoginFailureReason.None));
            }
        }

        [TestCase(false, "missing")]
        [TestCase(false, "successWithoutAccount")]
        [TestCase(false, "rejectionWithAccount")]
        [TestCase(true, "missing")]
        [TestCase(true, "successWithoutAccount")]
        [TestCase(true, "rejectionWithAccount")]
        public void OperationResults_MalformedDataContract_RejectsContradictoryOrMissingMembers(
            bool registration, string corruption)
        {
            PlayerAccount account = PlayerAccount.FromStoredAccount(7, "player", "es-MX",
                AccountAccessStatus.FromStoredState(false, false, null));
            object result = registration ? (object)RegistrationResult.Success(account) : LoginResult.Success(account);
            Type type = result.GetType();
            DataContractSerializer serializer = new DataContractSerializer(type);
            XDocument document;
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, result);
                stream.Position = 0;
                document = XDocument.Load(stream);
            }
            XElement accountElement = document.Root.Elements().Single(element => element.Name.LocalName == "Account");
            XElement reasonElement = document.Root.Elements().Single(element => element.Name.LocalName == "Reason");
            if (corruption == "missing") reasonElement.Remove();
            else if (corruption == "successWithoutAccount")
            {
                accountElement.RemoveNodes();
                accountElement.SetAttributeValue(XName.Get("nil", "http://www.w3.org/2001/XMLSchema-instance"), "true");
            }
            else reasonElement.Value = registration ? "UsernameAlreadyExists" : "Banned";
            using (MemoryStream stream = new MemoryStream())
            {
                document.Save(stream);
                stream.Position = 0;
                Assert.Throws<SerializationException>(() => serializer.ReadObject(stream));
            }
        }

        [TestCase("AccessStatus", null)]
        [TestCase("PreferredLanguageId", null)]
        [TestCase("PreferredLanguageId", "12345678901")]
        public void PlayerAccount_InvalidDeserializedState_ThrowsSerializationException(
            string memberName, string invalidValue)
        {
            PlayerAccount account = PlayerAccount.FromStoredAccount(7, "player", "es-MX",
                AccountAccessStatus.FromStoredState(false, false, null));
            DataContractSerializer serializer = new DataContractSerializer(typeof(PlayerAccount));
            XDocument document;
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, account);
                stream.Position = 0;
                document = XDocument.Load(stream);
            }

            // Keep the required member present: IsRequired alone cannot reject these values.
            XElement member = document.Root.Elements().Single(element => element.Name.LocalName == memberName);
            if (invalidValue == null)
            {
                member.RemoveNodes();
                member.SetAttributeValue(XName.Get("nil", "http://www.w3.org/2001/XMLSchema-instance"), "true");
            }
            else
            {
                member.Value = invalidValue;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                document.Save(stream);
                stream.Position = 0;
                Assert.Throws<SerializationException>(() => serializer.ReadObject(stream));
            }
        }

        [TestCase("")]
        [TestCase("fr-FR")]
        [TestCase("1234567890")]
        public void PlayerAccount_Deserialization_PreservesExistingLanguageRules(string language)
        {
            PlayerAccount account = PlayerAccount.FromStoredAccount(7, "player", language,
                AccountAccessStatus.FromStoredState(false, false, null));

            PlayerAccount reconstructed = RoundTrip(account);

            Assert.That(reconstructed, Is.Not.SameAs(account));
            Assert.That(reconstructed.PreferredLanguageId, Is.EqualTo(language));
            Assert.That(reconstructed.AccessStatus, Is.Not.Null);
        }

        private static T RoundTrip<T>(T value)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, value);
                stream.Position = 0;
                return (T)serializer.ReadObject(stream);
            }
        }

        [Test]
        public void Register_NullInput_ThrowsArgumentNullWithoutOpeningDatabase()
        {
            // Arrange.
            RegistrationController controller = new RegistrationController(
                NullLogger<RegistrationController>.Instance);

            // Act.
            ArgumentNullException failure = Assert.Throws<ArgumentNullException>(
                () => controller.Register(null));

            // Assert.
            Assert.That(failure.ParamName, Is.Not.Empty);
        }
    }
}
