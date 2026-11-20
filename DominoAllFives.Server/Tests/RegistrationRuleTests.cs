using System;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Validation;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class RegistrationRuleTests
    {
        private const string ValidUsername = "player";
        private const string ValidEmail = "player@gmail.com";
        private const string ValidPassword = "StrongPass1!";
        private const string PreferredLanguage = "es-MX";

        [TestCase(RegistrationIssue.InvalidRegistrationData)]
        [TestCase(RegistrationIssue.InvalidUsername)]
        [TestCase(RegistrationIssue.InvalidEmail)]
        [TestCase(RegistrationIssue.InvalidPassword)]
        [TestCase(RegistrationIssue.UsernameAlreadyExists)]
        [TestCase(RegistrationIssue.EmailAlreadyExists)]
        public void Constructor_DefinedRegistrationIssue_PreservesIssue(RegistrationIssue issue)
        {
            // Arrange
            RegistrationIssue expectedIssue = issue;
            // Act
            RegistrationRuleException exception = new RegistrationRuleException(issue);
            // Assert
            Assert.That(exception.Issue, Is.EqualTo(expectedIssue));
        }

        [TestCase(-1)]
        [TestCase(int.MaxValue)]
        public void Constructor_UndefinedRegistrationIssue_ThrowsArgumentOutOfRangeException(
            int value)
        {
            // Arrange
            RegistrationIssue issue = (RegistrationIssue)value;
            // Act
            TestDelegate create = CreateException;
            // Assert
            Assert.That(create, Throws.TypeOf<ArgumentOutOfRangeException>());

            void CreateException()
            {
                new RegistrationRuleException(issue);
            }
        }

        [Test]
        public void Validate_ValidRegistration_DoesNotThrow()
        {
            // Arrange
            AccountRegistration registration = AccountRegistration.Create(
                ValidUsername,
                ValidEmail,
                ValidPassword,
                PreferredLanguage);
            // Act
            TestDelegate validate = ValidateRegistration;
            // Assert
            Assert.That(validate, Throws.Nothing);

            void ValidateRegistration()
            {
                RegistrationValidator.Validate(registration);
            }
        }

        [Test]
        public void Validate_NullRegistration_ThrowsArgumentNullException()
        {
            // Arrange
            AccountRegistration registration = null;
            // Act
            TestDelegate validate = ValidateRegistration;
            // Assert
            Assert.That(validate, Throws.TypeOf<ArgumentNullException>());

            void ValidateRegistration()
            {
                RegistrationValidator.Validate(registration);
            }
        }

        [TestCase(null, ValidEmail, ValidPassword, RegistrationIssue.InvalidUsername)]
        [TestCase("   ", "invalid-email", "weak", RegistrationIssue.InvalidUsername)]
        [TestCase(ValidUsername, null, ValidPassword, RegistrationIssue.InvalidEmail)]
        [TestCase(ValidUsername, "invalid-email", "weak", RegistrationIssue.InvalidEmail)]
        [TestCase(
            ValidUsername,
            "player@example.com",
            ValidPassword,
            RegistrationIssue.InvalidEmail)]
        [TestCase(ValidUsername, ValidEmail, null, RegistrationIssue.InvalidPassword)]
        [TestCase(ValidUsername, ValidEmail, "weak", RegistrationIssue.InvalidPassword)]
        public void Validate_InvalidFields_ThrowsOrderedRegistrationRule(
            string username,
            string email,
            string password,
            RegistrationIssue expectedIssue)
        {
            // Arrange
            AccountRegistration registration = AccountRegistration.Create(
                username,
                email,
                password,
                PreferredLanguage);
            // Act
            TestDelegate validate = ValidateRegistration;
            // Assert
            Assert.That(validate, Throws.TypeOf<RegistrationRuleException>()
                .With.Property("Issue").EqualTo(expectedIssue));

            void ValidateRegistration()
            {
                RegistrationValidator.Validate(registration);
            }
        }

        [TestCase("   ", "invalid-email", "weak", RegistrationIssue.InvalidUsername)]
        [TestCase(ValidUsername, "invalid-email", "weak", RegistrationIssue.InvalidEmail)]
        [TestCase(ValidUsername, ValidEmail, "weak", RegistrationIssue.InvalidPassword)]
        public void Register_InvalidFields_ThrowsRegistrationRuleBeforePersistence(
            string username,
            string email,
            string password,
            RegistrationIssue expectedIssue)
        {
            // Arrange
            RegistrationController controller = new RegistrationController();
            AccountRegistration registration = AccountRegistration.Create(
                username,
                email,
                password,
                PreferredLanguage);
            // Act
            TestDelegate register = RegisterAccount;
            // Assert
            Assert.That(register, Throws.TypeOf<RegistrationRuleException>()
                .With.Property("Issue").EqualTo(expectedIssue));

            void RegisterAccount()
            {
                controller.Register(registration);
            }
        }

        [Test]
        public void Register_NullRegistration_ThrowsArgumentNullExceptionBeforePersistence()
        {
            // Arrange
            RegistrationController controller = new RegistrationController();
            // Act
            TestDelegate register = RegisterAccount;
            // Assert
            Assert.That(register, Throws.TypeOf<ArgumentNullException>());

            void RegisterAccount()
            {
                controller.Register(null);
            }
        }
    }
}
