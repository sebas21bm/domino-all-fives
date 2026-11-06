using System;
using DominoAllFives.Client.WPF.Services.Local;
using DominoAllFives.Contracts.Domain;
using DomainProfilePicture = DominoAllFives.Contracts.Domain.ProfilePicture;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.OldServices;

namespace DominoAllFives.Client.WPF.Services.Local
{
    /// <summary>Retains only account operations that have not yet been migrated to WCF.</summary>
    public sealed class LegacyAccountServiceAdapter : IAccountService
    {
        private readonly Lazy<LocalAccountService> _localService;

        public LegacyAccountServiceAdapter(Func<LocalAccountService> localServiceFactory)
        {
            if (localServiceFactory == null)
            {
                throw new ArgumentNullException(nameof(localServiceFactory));
            }
            _localService = new Lazy<LocalAccountService>(localServiceFactory);
        }

        public LanguagePreferenceStatus ChangePreferredLanguage(string languageId) =>
            _localService.Value.ChangePreferredLanguage(languageId);

        public RegistrationResult Register(AccountRegistration registrationData) =>
            _localService.Value.Register(registrationData);

        public ProfilePictureStatus SetProfilePicture(int playerId, DomainProfilePicture picture) =>
            _localService.Value.SetProfilePicture(playerId, picture);

        public ProfilePictureStatus RemoveProfilePicture(int playerId) =>
            _localService.Value.RemoveProfilePicture(playerId);

        public PlayerProfile GetProfile(int playerId) => _localService.Value.GetProfile(playerId);

        public UpdateProfileStatus UpdateProfile(int playerId, string username) =>
            _localService.Value.UpdateProfile(playerId, username);

        public UpdateProfileStatus UpdateProfile(int playerId, string username,
            DomainProfilePicture replacementPicture) =>
            _localService.Value.UpdateProfile(playerId, username, replacementPicture);

        public DeleteStatus DeleteAccount(int playerId, string password) =>
            _localService.Value.DeleteAccount(playerId, password);

        public ChangePasswordStatus ChangePassword(int playerId, string currentPassword,
            string newPassword) =>
            _localService.Value.ChangePassword(playerId, currentPassword, newPassword);
    }
}
