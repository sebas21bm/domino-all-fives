using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.OldServices
{
    /// <summary>
    /// Represents the service responsible for handling account-related operations,
    /// such as registration and profile picture management pending migration.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Updates the current local account language.
        /// </summary>
        LanguagePreferenceStatus ChangePreferredLanguage(string preferredLanguageId);

        /// <summary>
        /// Registers a new user account with the provided registration data.
        /// </summary>
        /// <param name="registrationData">The registration required data.</param>
        /// <returns>The registration result.</returns>
        RegistrationResult Register(AccountRegistration registrationData);

        /// <summary>
        /// Sets the profile picture for the current user.
        /// </summary>
        /// <param name="playerId">The player whose picture is updated.</param>
        /// <param name="picture">The selected image snapshot validated by the operation.</param>
        ProfilePictureStatus SetProfilePicture(int playerId, ProfilePicture picture);

        /// <summary>
        /// Removes the profile picture for the specified player.
        /// </summary>
        /// <param name="playerId">The ID of the player.</param>
        ProfilePictureStatus RemoveProfilePicture(int playerId);

        /// <summary>
        /// Retrieves the profile information for the specified player.
        /// </summary>
        /// <param name="playerId">The ID of the player.</param>
        /// <returns>The profile information, or null only when the player does not exist.</returns>
        PlayerProfile GetProfile(int playerId);

        /// <summary>
        /// Updates the username while preserving the existing profile picture.
        /// </summary>
        /// <param name="playerId">The account identifier.</param>
        /// <param name="username">The requested username.</param>
        UpdateProfileStatus UpdateProfile(int playerId, string username);

        /// <summary>
        /// Updates the username and replaces the profile picture.
        /// </summary>
        /// <param name="playerId">The account identifier.</param>
        /// <param name="username">The requested username.</param>
        /// <param name="replacementPicture">The replacement snapshot; null is invalid.</param>
        UpdateProfileStatus UpdateProfile(int playerId, string username, ProfilePicture replacementPicture);

        /// <summary>
        /// Logically deletes a player account after verifying its password.
        /// </summary>
        /// <param name="playerId">The account identifier.</param>
        /// <param name="password">The current password to verify.</param>
        DeleteStatus DeleteAccount(int playerId, string password);


        /// <summary>
        /// Changes a player account password.
        /// </summary>
        /// <param name="playerId">The account identifier.</param>
        /// <param name="currentPassword">The current password to verify.</param>
        /// <param name="newPassword">The new password.</param>
        ChangePasswordStatus ChangePassword(int playerId, string currentPassword, string newPassword);

    }
}
