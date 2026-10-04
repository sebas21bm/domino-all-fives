using DominoAllFives.DataAccess.Models;

namespace DominoAllFives.DataAccess.Interfaces
{
    /// <summary>
    /// Defines persistence operations for player accounts.
    /// </summary>
    public interface IPlayerRepository
    {
        /// <summary>
        /// Retrieves a player by username or email.
        /// </summary>
        /// <param name="identifier">
        /// The username or email used to identify the player.
        /// </param>
        /// <returns>
        /// The matching player, or null when no player is found.
        /// </returns>
        Player GetByUsernameOrEmail(string identifier);

        /// <summary>
        /// Retrieves a player by identifier.
        /// </summary>
        /// <param name="playerId">
        /// The identifier of the player.
        /// </param>
        /// <returns>
        /// The matching player, or null when no player is found.
        /// </returns>
        Player GetById(int playerId);

        /// <summary>
        /// Determines whether a username is already registered.
        /// </summary>
        /// <param name="username">
        /// The username to verify.
        /// </param>
        /// <returns>
        /// True when the username exists; otherwise, false.
        /// </returns>
        bool UsernameExists(string username);

        /// <summary>
        /// Determines whether an email address is already registered.
        /// </summary>
        /// <param name="email">
        /// The email address to verify.
        /// </param>
        /// <returns>
        /// True when the email exists; otherwise, false.
        /// </returns>
        bool EmailExists(string email);

        /// <summary>
        /// Adds a player to the current persistence context.
        /// </summary>
        /// <param name="playerToAdd">
        /// The player to add.
        /// </param>
        void Add(Player playerToAdd);

        /// <summary>
        /// Updates the profile picture assigned to a player.
        /// </summary>
        /// <param name="playerId">
        /// The identifier of the player.
        /// </param>
        /// <param name="profilePictureFileName">
        /// The name of the stored profile picture file.
        /// </param>
        /// <returns>
        /// True when the player exists and the profile picture
        /// was updated; otherwise, false.
        /// </returns>
        bool UpdateProfilePicture(int playerId, string profilePictureFileName);
    }
}