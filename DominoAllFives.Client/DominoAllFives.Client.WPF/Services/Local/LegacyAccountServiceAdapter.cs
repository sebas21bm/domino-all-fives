using System;
using DominoAllFives.Client.WPF.Services.Local;
using DominoAllFives.Contracts.Domain;
using DomainProfilePicture = DominoAllFives.Contracts.Domain.ProfilePicture;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;

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

        public LanguagePreferenceFailureReason ChangePreferredLanguage(string languageId) =>
            _localService.Value.ChangePreferredLanguage(languageId);

        public RegistrationResult Register(AccountRegistration registrationData) =>
            _localService.Value.Register(registrationData);

        public ProfilePictureFailureReason SetProfilePicture(int playerId, DomainProfilePicture picture) =>
            _localService.Value.SetProfilePicture(playerId, picture);

        public ProfilePictureFailureReason RemoveProfilePicture(int playerId) =>
            _localService.Value.RemoveProfilePicture(playerId);

        public PlayerProfile GetProfile(int playerId) => _localService.Value.GetProfile(playerId);

        public UpdateProfileFailureReason UpdateProfile(int playerId, string username) =>
            _localService.Value.UpdateProfile(playerId, username);

        public UpdateProfileFailureReason UpdateProfile(int playerId, string username,
            DomainProfilePicture replacementPicture) =>
            _localService.Value.UpdateProfile(playerId, username, replacementPicture);

        public DeleteFailureReason DeleteAccount(int playerId, string password) =>
            _localService.Value.DeleteAccount(playerId, password);

        public ChangePasswordFailureReason ChangePassword(int playerId, string currentPassword,
            string newPassword) =>
            _localService.Value.ChangePassword(playerId, currentPassword, newPassword);
    }
}
