using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Services;
using System;

namespace DominoAllFives.Client.WPF.Services.Local
{
    /// <summary>
    /// Represents a local implementation
    /// providing account-related operations,
    /// it'll be later replaced with a remote implementation
    /// that communicates with the server.
    /// </summary>
    public class LocalAccountService : IAccountService
    {
        private readonly AuthenticationController _authenticationController;
        private readonly RegistrationController _registrationController;
        private readonly ProfilePictureController _profilePictureController;
        private readonly ProfileController _profileController;

        public LocalAccountService(
            AuthenticationController authenticationController,
            RegistrationController registrationController,
            ProfilePictureController profilePictureController,
            ProfileController profileController)
        {
            _authenticationController = authenticationController
                ?? throw new ArgumentNullException(
                    nameof(authenticationController));

            _registrationController = registrationController
                ?? throw new ArgumentNullException(
                    nameof(registrationController));

            _profilePictureController = profilePictureController
                ?? throw new ArgumentNullException(
                    nameof(profilePictureController));

            _profileController = profileController
                ?? throw new ArgumentNullException(
                    nameof(profileController));
        }

        /// <inheritdoc/>
        public LoginResultDto Login(LoginRequestDto loginRequest)
        {
            return _authenticationController.Login(loginRequest);
        }

        /// <inheritdoc/>
        public RegistrationResultDto Register(RegisterAccountDto registrationData)
        {
            return _registrationController.Register(registrationData);
        }

        /// <inheritdoc/>
        public ProfilePictureResultDto SetProfilePicture(ProfilePictureDto profilePicture)
        {
            return _profilePictureController.SetProfilePicture(profilePicture);
        }

        /// <inheritdoc/>
        public ProfilePictureResultDto RemoveProfilePicture(int playerId)
        {
            return _profilePictureController.RemoveProfilePicture(playerId);
        }

        /// <inheritdoc/>
        public ProfileDto GetProfile(int playerId)
        {
            return _profileController.GetProfile(playerId);
        }
    }
}
