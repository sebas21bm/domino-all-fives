using System;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.BusinessLogic.Validation;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using Player = DominoAllFives.DataAccess.Models.Player;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Coordinates profile picture operations for player accounts.
    /// </summary>
    public class ProfilePictureController
    {
        private readonly ProfilePictureStorage _profilePictureStorage;

        /// <summary>Initializes profile picture operations with their persistent storage.</summary>
        /// <param name = "profilePictureStorage">The profile picture storage.</param>
        /// <exception cref = "ArgumentNullException">The storage is null.</exception>
        public ProfilePictureController(ProfilePictureStorage profilePictureStorage)
        {
            _profilePictureStorage = profilePictureStorage ?? throw new ArgumentNullException(
                nameof(profilePictureStorage));
        }

        /// <summary>
        /// Sets the profile picture of a player.
        /// </summary>
        /// <param name = "playerId">The account identifier.</param>
        /// <param name = "picture">
        /// The selected snapshot; null or absent content is invalid.
        /// </param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// The player identifier is invalid.
        /// </exception>
        /// <exception cref = "ArgumentNullException">The selected picture is null.</exception>
        /// <exception cref = "ProfileRuleException">
        /// The selected picture was rejected.
        /// </exception>
        public void SetProfilePicture(int playerId, ProfilePicture picture)
        {
            ValidatePictureSelection(playerId, picture);

            byte[] imageData = picture.ImageData;
            string fileName = picture.OriginalFileName;
            string previousProfilePicture = string.Empty;
            string profilePictureFileName = string.Empty;
            bool changesCommitted = false;
            try
            {
                using (DominoAllFivesEntities context = new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository = new PlayerRepository(context);
                    Player player = playerRepository.GetById(playerId);
                    if (player == null || player.IsDeleted)
                    {
                        throw new ProfileRuleException(ProfileIssue.PlayerNotFound);
                    }

                    profilePictureFileName = _profilePictureStorage.GenerateFileName(
                        playerId,
                        fileName);
                    _profilePictureStorage.SaveProfilePicture(profilePictureFileName, imageData);
                    previousProfilePicture = player.ProfilePicture;
                    bool wasUpdated =
                        playerRepository.UpdateProfilePicture(playerId, profilePictureFileName);
                    if (!wasUpdated)
                    {
                        throw new ProfileRuleException(ProfileIssue.PlayerNotFound);
                    }

                    context.SaveChanges();
                    changesCommitted = true;
                }
            }
            finally
            {
                if (!changesCommitted)
                {
                    _profilePictureStorage.TryDeleteProfilePicture(profilePictureFileName);
                }
            }

            DeletePreviousProfilePicture(previousProfilePicture, profilePictureFileName);
        }

        /// <summary>
        /// Removes the profile picture assigned to a player.
        /// </summary>
        /// <param name = "playerId">
        /// The identifier of the player whose profile picture
        /// will be removed.
        /// </param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// The player identifier is invalid.
        /// </exception>
        /// <exception cref = "ProfileRuleException">The player is unavailable.</exception>
        public void RemoveProfilePicture(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            string previousProfilePicture = string.Empty;
            using (DominoAllFivesEntities context = new DominoAllFivesEntities())
            {
                IPlayerRepository playerRepository = new PlayerRepository(context);
                Player player = playerRepository.GetById(playerId);
                if (player == null || player.IsDeleted)
                {
                    throw new ProfileRuleException(ProfileIssue.PlayerNotFound);
                }

                previousProfilePicture = player.ProfilePicture;
                if (string.IsNullOrWhiteSpace(previousProfilePicture))
                {
                    return;
                }

                bool wasUpdated = playerRepository.UpdateProfilePicture(playerId, null);
                if (!wasUpdated)
                {
                    throw new ProfileRuleException(ProfileIssue.PlayerNotFound);
                }

                context.SaveChanges();
            }

            _profilePictureStorage.TryDeleteProfilePicture(previousProfilePicture);
        }

        /// <summary>
        /// Deletes a previously assigned profile picture.
        /// </summary>
        private void DeletePreviousProfilePicture(
            string previousProfilePicture,
            string currentProfilePicture)
        {
            if (string.IsNullOrWhiteSpace(previousProfilePicture)
                || string.Equals(
                previousProfilePicture,
                currentProfilePicture,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _profilePictureStorage.TryDeleteProfilePicture(previousProfilePicture);
        }

        private static void ValidatePictureSelection(
            int playerId,
            ProfilePicture picture)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            if (picture == null)
            {
                throw new ArgumentNullException(nameof(picture));
            }

            byte[] imageData = picture.ImageData;
            if (imageData == null || imageData.Length == 0)
            {
                throw new ProfileRuleException(ProfileIssue.InvalidProfilePicture);
            }

            if (!ProfilePictureValidator.HasValidExtension(picture.OriginalFileName))
            {
                throw new ProfileRuleException(
                    ProfileIssue.InvalidProfilePictureFormat);
            }

            if (!ProfilePictureValidator.HasValidFileSize(imageData))
            {
                throw new ProfileRuleException(ProfileIssue.ProfilePictureTooLarge);
            }
        }
    }
}
