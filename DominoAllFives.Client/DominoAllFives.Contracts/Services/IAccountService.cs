using DominoAllFives.Contracts.DTOs;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Represents the service responsible for handling account-related operations,
    /// such as login, registration, and profile picture management.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Logs in a user with the provided login request data.
        /// </summary>
        /// <param name="loginRequest">The login request data.</param>
        /// <returns>The login result.</returns>
        LoginResultDto Login(LoginRequestDto loginRequest);

        /// <summary>
        /// Registers a new user account with the provided registration data.
        /// </summary>
        /// <param name="registrationData">The registration required data.</param>
        /// <returns>The registration result.</returns>
        RegistrationResultDto Register(RegisterAccountDto registrationData);

        /// <summary>
        /// Sets the profile picture for the current user.
        /// </summary>
        /// <param name="profilePicture">The profile picture data.</param>
        /// <returns>The result of setting the profile picture.</returns>
        ProfilePictureResultDto SetProfilePicture(ProfilePictureDto profilePicture);

        /// <summary>
        /// Removes the profile picture for the specified player.
        /// </summary>
        /// <param name="playerId">The ID of the player.</param>
        /// <returns>The result of removing the profile picture.</returns>
        ProfilePictureResultDto RemoveProfilePicture(int playerId);

        /// <summary>
        /// Retrieves the profile information for the specified player.
        /// </summary>
        /// <param name="playerId">The ID of the player.</param>
        /// <returns>The profile information including username and statistics.</returns>
        ProfileDto GetProfile(int playerId);

        /// <summary>
        /// Updates the editable profile information of a player.
        /// </summary>
        /// <param name="profileData">
        /// The profile information to update.
        /// </param>
        /// <returns>
        /// The result of the profile update operation.
        /// </returns>
        UpdateProfileResultDto UpdateProfile(UpdateProfileDto profileData);

        /// <summary>
        /// Logically deletes a player account after verifying its password.
        /// </summary>
        /// <param name="request">
        /// The account deletion request.
        /// </param>
        /// <returns>
        /// The result of the account deletion operation.
        /// </returns>
        DeleteAccountResultDto DeleteAccount(DeleteAccountRequestDto request);


        /// <summary>
        /// Changes a player account password.
        /// </summary>
        /// <param name="request">
        /// The password change request.
        /// </param>
        /// <returns>
        /// The result of the password change operation.
        /// </returns>
        ChangePasswordResultDto ChangePassword(ChangePasswordRequestDto request);

    }
}
