using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.BusinessLogic.Validation;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Coordinates profile picture operations for player accounts.
    /// </summary>
    public class ProfilePictureController
    {
        private readonly ProfilePictureStorage _profilePictureStorage;
        private readonly ILogger<ProfilePictureController> _logger;

        public ProfilePictureController(
            ProfilePictureStorage profilePictureStorage,
            ILogger<ProfilePictureController> logger)
        {
            _profilePictureStorage = profilePictureStorage
                ?? throw new ArgumentNullException(
                    nameof(profilePictureStorage));

            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Sets the profile picture of a player.
        /// </summary>
        /// <param name="playerId">The account identifier.</param>
        /// <param name="picture">The selected snapshot; null or absent content is invalid.</param>
        public ProfilePictureFailureReason SetProfilePicture(int playerId, ProfilePicture picture)
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
            string fileName = picture?.OriginalFileName;
            if (imageData == null ||
                imageData.Length == 0)
            {
                return ProfilePictureFailureReason.InvalidImage;
            }

            if (!ProfilePictureValidator.HasValidExtension(
                    fileName))
            {
                return ProfilePictureFailureReason.InvalidFormat;
            }

            if (!ProfilePictureValidator.HasValidFileSize(
                    imageData))
            {
                return ProfilePictureFailureReason.FileTooLarge;
            }

            string previousProfilePicture = string.Empty;
            string profilePictureFileName = string.Empty;
            bool changesCommitted = false;

            try
            {
                using (DominoAllFivesEntities context =
                    new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository =
                        new PlayerRepository(context);

                    Player player =
                        playerRepository.GetById(
                            playerId);

                    if (player == null)
                    {
                        return ProfilePictureFailureReason.PlayerNotFound;
                    }

                    profilePictureFileName =
                    _profilePictureStorage.GenerateFileName(
                        playerId,
                        fileName);

                    _profilePictureStorage.SaveProfilePicture(
                        profilePictureFileName,
                        imageData);

                    previousProfilePicture =
                        player.ProfilePicture;

                    bool wasUpdated =
                        playerRepository.UpdateProfilePicture(
                            playerId,
                            profilePictureFileName);

                    if (!wasUpdated)
                    {
                        _profilePictureStorage.TryDeleteProfilePicture(
                            profilePictureFileName);

                        return ProfilePictureFailureReason.PlayerNotFound;
                    }

                    context.SaveChanges();
                    changesCommitted = true;
                }
            }
            catch (Exception ex)
            {
                if (!changesCommitted)
                {
                    _profilePictureStorage.TryDeleteProfilePicture(profilePictureFileName);
                }

                _logger.LogError(
                    ex,
                    "Unable to store profile picture for player {PlayerId}.",
                    playerId);
                throw;
            }

            DeletePreviousProfilePicture(
                previousProfilePicture,
                profilePictureFileName);

            return ProfilePictureFailureReason.None;
        }

        /// <summary>
        /// Removes the profile picture assigned to a player.
        /// </summary>
        /// <param name="playerId">
        /// The identifier of the player whose profile picture
        /// will be removed.
        /// </param>
        public ProfilePictureFailureReason RemoveProfilePicture(
            int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            string previousProfilePicture = string.Empty;

            try
            {
                using (DominoAllFivesEntities context =
                    new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository =
                        new PlayerRepository(context);

                    Player player =
                        playerRepository.GetById(playerId);

                    if (player == null)
                    {
                        return ProfilePictureFailureReason.PlayerNotFound;
                    }

                    previousProfilePicture =
                        player.ProfilePicture;

                    if (string.IsNullOrWhiteSpace(
                            previousProfilePicture))
                    {
                        return ProfilePictureFailureReason.None;
                    }

                    bool wasUpdated =
                        playerRepository.UpdateProfilePicture(
                            playerId,
                            null);

                    if (!wasUpdated)
                    {
                        return ProfilePictureFailureReason.PlayerNotFound;
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex) when (
                ex is SqlException ||
                ex is EntityException ||
                ex is DbUpdateException)
            {
                _logger.LogError(
                    ex,
                    "Unable to remove profile picture for player {PlayerId}.",
                    playerId);

                throw;
            }

            _profilePictureStorage.TryDeleteProfilePicture(
                previousProfilePicture);

            return ProfilePictureFailureReason.None;
        }

        /// <summary>
        /// Deletes a previously assigned profile picture.
        /// </summary>
        private void DeletePreviousProfilePicture(
            string previousProfilePicture,
            string currentProfilePicture)
        {
            if (string.IsNullOrWhiteSpace(
                    previousProfilePicture) ||
                string.Equals(
                    previousProfilePicture,
                    currentProfilePicture,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _profilePictureStorage.TryDeleteProfilePicture(
                previousProfilePicture);
        }

    }
}
