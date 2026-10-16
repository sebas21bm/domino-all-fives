using System;
using System.Threading;
using System.Windows.Controls;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class AccountOperationTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void AccountOperations_InvalidId_ThrowBeforeBusinessValidation(int playerId)
        {
            AccountController controller = new AccountController(NullLogger<AccountController>.Instance);
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.DeleteAccount(playerId, ""));
            Assert.Throws<ArgumentOutOfRangeException>(() => controller.ChangePassword(playerId, "", ""));
        }

        [TestCase(1, "", DeleteFailureReason.PasswordNotMatch)]
        [TestCase(1, null, DeleteFailureReason.PasswordNotMatch)]
        public void DeleteAccount_InvalidParameters_PreservesReasonBeforeDatabase(
            int playerId, string password, DeleteFailureReason expected)
        {
            // Arrange.
            AccountController controller = new AccountController(
                NullLogger<AccountController>.Instance);

            // Act.
            DeleteFailureReason reason = controller.DeleteAccount(playerId, password);

            // Assert.
            Assert.That(reason, Is.EqualTo(expected));
        }

        [TestCase(1, "", "", ChangePasswordFailureReason.IncorrectCurrentPassword)]
        [TestCase(1, "old-password", "x", ChangePasswordFailureReason.InvalidNewPassword)]
        public void ChangePassword_MultipleInvalidFields_PreservesReasonBeforeDatabase(
            int playerId, string current, string replacement, ChangePasswordFailureReason expected)
        {
            // Arrange.
            AccountController controller = new AccountController(
                NullLogger<AccountController>.Instance);

            // Act.
            ChangePasswordFailureReason reason = controller.ChangePassword(playerId, current, replacement);

            // Assert.
            Assert.That(reason, Is.EqualTo(expected));
        }

        [TestCase(ChangePasswordFailureReason.IncorrectCurrentPassword,
            "MessageAccount_msgWrongCurrentPassword")]
        [TestCase(ChangePasswordFailureReason.InvalidNewPassword,
            "MessageAccount_msgUnsafePassword")]
        [TestCase(ChangePasswordFailureReason.AccountNotFound,
            "MessageAccount_msgAccountNotFound")]
        [TestCase(ChangePasswordFailureReason.SameAsCurrentPassword,
            "MessageAccount_msgSamePassword")]
        [TestCase(ChangePasswordFailureReason.ServiceUnavailable, "Global_msgConnectionError")]
        public void ChangePassword_BusinessRejectionOrTechnicalFailure_PreservesDialogAndInput(
            ChangePasswordFailureReason reason, string expectedMessage)
        {
            // Arrange.
            OperationService service = new OperationService { PasswordFailure = reason };
            DialogRecorder dialogs = new DialogRecorder();
            PlayerSession session = CreateSession();
            bool callbackInvoked = false;
            ChangePasswordViewModel model = new ChangePasswordViewModel(
                service, dialogs, NullLogger<ChangePasswordViewModel>.Instance, session,
                () => callbackInvoked = true, () => { }, true);
            PasswordBox[] boxes = CreatePasswordBoxes();

            // Act.
            model.ChangePasswordCommand.Execute(boxes);

            // Assert.
            Assert.That(dialogs.Last.MessageKey, Is.EqualTo(expectedMessage));
            Assert.That(dialogs.Last.OnAccept, Is.Null);
            Assert.That(boxes[0].Password, Is.EqualTo("old-password"));
            Assert.That(boxes[1].Password, Is.EqualTo("New-Password2!"));
            Assert.That(callbackInvoked, Is.False);
            Assert.That(session.IsAuthenticated, Is.True);
        }

        [Test]
        public void ChangePassword_NormalReturn_ClearsBoxesAndDefersSuccessCallback()
        {
            // Arrange.
            OperationService service = new OperationService();
            DialogRecorder dialogs = new DialogRecorder();
            bool callbackInvoked = false;
            ChangePasswordViewModel model = new ChangePasswordViewModel(
                service, dialogs, NullLogger<ChangePasswordViewModel>.Instance, CreateSession(),
                () => callbackInvoked = true, () => { }, true);
            PasswordBox[] boxes = CreatePasswordBoxes();

            // Act.
            model.ChangePasswordCommand.Execute(boxes);

            // Assert.
            Assert.That(service.LastPlayerId, Is.EqualTo(9));
            Assert.That(dialogs.Last.Type, Is.EqualTo(DialogType.Success));
            Assert.That(boxes[0].Password, Is.Empty);
            Assert.That(boxes[1].Password, Is.Empty);
            Assert.That(boxes[2].Password, Is.Empty);
            Assert.That(callbackInvoked, Is.False);
            dialogs.Last.OnAccept();
            Assert.That(callbackInvoked, Is.True);
        }

        [TestCase(DeleteFailureReason.PasswordNotMatch, "MessageAccount_msgWrongCurrentPassword")]
        [TestCase(DeleteFailureReason.AccountNotFound, "MessageAccount_msgAccountNotFound")]
        [TestCase(DeleteFailureReason.ServiceUnavailable, "Global_msgConnectionError")]
        public void DeleteAccount_BusinessRejectionOrTechnicalFailure_PreservesDialogInputAndSession(
            DeleteFailureReason reason, string expectedMessage)
        {
            // Arrange.
            OperationService service = new OperationService { DeleteFailure = reason };
            DialogRecorder dialogs = new DialogRecorder();
            PlayerSession session = CreateSession();
            bool callbackInvoked = false;
            DeleteAccountViewModel model = new DeleteAccountViewModel(
                dialogs, service, NullLogger<DeleteAccountViewModel>.Instance, session,
                () => callbackInvoked = true, () => { });
            PasswordBox box = new PasswordBox { Password = "old-password" };

            // Act.
            model.ConfirmDeleteCommand.Execute(box);

            // Assert.
            Assert.That(dialogs.Last.MessageKey, Is.EqualTo(expectedMessage));
            Assert.That(box.Password, Is.EqualTo("old-password"));
            Assert.That(session.IsAuthenticated, Is.True);
            Assert.That(callbackInvoked, Is.False);
        }

        [Test]
        public void DeleteAccount_NormalReturn_InvokesLogoutOnlyAfterSuccessDialog()
        {
            // Arrange.
            OperationService service = new OperationService();
            DialogRecorder dialogs = new DialogRecorder();
            PlayerSession session = CreateSession();
            DeleteAccountViewModel model = new DeleteAccountViewModel(
                dialogs, service, NullLogger<DeleteAccountViewModel>.Instance, session,
                session.Clear, () => { });
            PasswordBox box = new PasswordBox { Password = "old-password" };

            // Act.
            model.ConfirmDeleteCommand.Execute(box);

            // Assert.
            Assert.That(service.LastPlayerId, Is.EqualTo(9));
            Assert.That(box.Password, Is.Empty);
            Assert.That(session.IsAuthenticated, Is.True);
            Assert.That(dialogs.Last.Type, Is.EqualTo(DialogType.Success));
            dialogs.Last.OnAccept();
            Assert.That(session.IsAuthenticated, Is.False);
        }

        private static PlayerSession CreateSession()
        {
            PlayerSession session = new PlayerSession();
            session.Start(9, "player", "es-MX");
            return session;
        }

        private static PasswordBox[] CreatePasswordBoxes()
        {
            return new[]
            {
                new PasswordBox { Password = "old-password" },
                new PasswordBox { Password = "New-Password2!" },
                new PasswordBox { Password = "New-Password2!" }
            };
        }

        private sealed class DialogRecorder : IDialogService
        {
            public DialogRequest Last { get; private set; }

            public void ShowDialog(DialogRequest request)
            {
                Last = request;
            }
        }

        private sealed class OperationService : IAccountService
        {
            public ChangePasswordFailureReason? PasswordFailure { get; set; }
            public DeleteFailureReason? DeleteFailure { get; set; }
            public int LastPlayerId { get; private set; }

            public ChangePasswordFailureReason ChangePassword(int playerId, string currentPassword, string newPassword)
            {
                LastPlayerId = playerId;
                if (PasswordFailure == ChangePasswordFailureReason.ServiceUnavailable)
                {
                    throw new System.Data.Entity.Core.EntityException("Synthetic technical failure.");
                }
                return PasswordFailure ?? ChangePasswordFailureReason.None;
            }

            public DeleteFailureReason DeleteAccount(int playerId, string password)
            {
                LastPlayerId = playerId;
                if (DeleteFailure == DeleteFailureReason.ServiceUnavailable)
                {
                    throw new System.Data.Entity.Core.EntityException("Synthetic technical failure.");
                }
                return DeleteFailure ?? DeleteFailureReason.None;
            }

            public LoginResult Login(LoginCredentials credentials) =>
                throw new NotSupportedException();
            public RegistrationResult Register(AccountRegistration registration) =>
                throw new NotSupportedException();
            public LanguagePreferenceFailureReason ChangePreferredLanguage(string languageId) =>
                throw new NotSupportedException();
            public ProfilePictureFailureReason SetProfilePicture(int playerId, ProfilePicture picture) =>
                throw new NotSupportedException();
            public ProfilePictureFailureReason RemoveProfilePicture(int playerId) =>
                throw new NotSupportedException();
            public PlayerProfile GetProfile(int playerId) => throw new NotSupportedException();
            public UpdateProfileFailureReason UpdateProfile(int playerId, string username) =>
                throw new NotSupportedException();
            public UpdateProfileFailureReason UpdateProfile(int playerId, string username, ProfilePicture replacementPicture) =>
                throw new NotSupportedException();
        }
    }
}
