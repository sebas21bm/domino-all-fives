using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.Contracts.Domain;
using DomainProfilePicture = DominoAllFives.Contracts.Domain.ProfilePicture;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Client.WPF.Models;
using System;

namespace DominoAllFives.Client.WPF.Services.Local
{
    /// <summary>
    /// Provides only account operations that have not been migrated to WCF.
    /// Login is implemented exclusively by the server.
    /// </summary>
    public class LocalAccountService
    {
        private readonly RegistrationController _registrationController;
        private readonly ProfilePictureController _profilePictureController;
        private readonly ProfileController _profileController;
        private readonly AccountController _accountController;
        private readonly AccountLanguageController _accountLanguageController;
        private readonly PlayerSession _playerSession;

        public LocalAccountService(
            RegistrationController registrationController,
            ProfilePictureController profilePictureController,
            ProfileController profileController,
            AccountController accountController,
            AccountLanguageController accountLanguageController,
            PlayerSession playerSession)
        {
            _registrationController = registrationController
                ?? throw new ArgumentNullException(nameof(registrationController));

            _profilePictureController = profilePictureController
                ?? throw new ArgumentNullException(nameof(profilePictureController));

            _profileController = profileController
                ?? throw new ArgumentNullException(nameof(profileController));

            _accountLanguageController = accountLanguageController
                ?? throw new ArgumentNullException(nameof(accountLanguageController));
            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));

            _accountController = accountController
                ?? throw new ArgumentNullException(nameof(accountController));

        }

        /// <inheritdoc/>
        public LanguagePreferenceFailureReason ChangePreferredLanguage(string preferredLanguageId)
        {
            // Local adapter only; a remote service must resolve its authenticated actor.
            int? playerId = _playerSession.PlayerId;
            if (!playerId.HasValue)
            {
                return LanguagePreferenceFailureReason.NotSignedIn;
            }
            LanguagePreferenceFailureReason reason =
                _accountLanguageController.ChangePreferredLanguage(
                    playerId.Value, preferredLanguageId);
            if (reason != LanguagePreferenceFailureReason.None)
            {
                return reason;
            }
            if (_playerSession.PlayerId != playerId)
            {
                return LanguagePreferenceFailureReason.NotSignedIn;
            }
            _playerSession.UpdatePreferredLanguage(preferredLanguageId);
            return LanguagePreferenceFailureReason.None;
        }

/// <inheritdoc/>
        public RegistrationResult Register(AccountRegistration registrationData)
        {
            return _registrationController.Register(registrationData);
        }

        /// <inheritdoc/>
        public ProfilePictureFailureReason SetProfilePicture(int playerId, DomainProfilePicture picture)
        {
            return _profilePictureController.SetProfilePicture(playerId, picture);
        }

        /// <inheritdoc/>
        public ProfilePictureFailureReason RemoveProfilePicture(int playerId)
        {
            return _profilePictureController.RemoveProfilePicture(playerId);
        }

        /// <inheritdoc/>
        public PlayerProfile GetProfile(int playerId)
        {
            return _profileController.GetProfile(playerId);
        }

        /// <inheritdoc/>
        public UpdateProfileFailureReason UpdateProfile(int playerId, string username)
        {
            return _profileController.UpdateProfile(playerId, username);
        }

        /// <inheritdoc/>
        public UpdateProfileFailureReason UpdateProfile(int playerId, string username,
            DomainProfilePicture replacementPicture)
        {
            return _profileController.UpdateProfile(playerId, username, replacementPicture);
        }

        /// <inheritdoc/>
        public DeleteFailureReason DeleteAccount(int playerId, string password)
        {
            return _accountController.DeleteAccount(playerId, password);
        }

        /// <inheritdoc/>
        public ChangePasswordFailureReason ChangePassword(int playerId, string currentPassword, string newPassword)
        {
            return _accountController.ChangePassword(playerId, currentPassword, newPassword);
        }

    }
}
