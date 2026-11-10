using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.BusinessLogic.Validation;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using Player = DominoAllFives.DataAccess.Models.Player;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Coordinates operations related to player profile information.
    /// </summary>
    public class ProfileController
    {
        private readonly ProfilePictureStorage _profilePictureStorage;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            ProfilePictureStorage profilePictureStorage,
            ILogger<ProfileController> logger)
        {
            _profilePictureStorage = profilePictureStorage
                ?? throw new ArgumentNullException(
                    nameof(profilePictureStorage));

            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the profile information of a player.
        /// </summary>
        /// <param name="playerId">The account identifier.</param>
        /// <returns>The domain value produced by the operation.</returns>
        public PlayerProfile GetProfile(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

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

                        return null;

                    }
                    if (player.PlayerStats == null)
                    {
                        throw new InvalidOperationException("The player has no required statistics.");
                    }

                    return PlayerProfile.FromStoredProfile(
                        player.Username,
                        PlayerStatistics.FromStoredStatistics(
                            player.PlayerStats.GamesWon,
                            player.PlayerStats.TotalPointsScored,
                            player.PlayerStats.GamesPlayed),
                        ProfilePicture.FromStoredContent(
                            _profilePictureStorage.GetProfilePictureData(player.ProfilePicture)));
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to retrieve profile for player {PlayerId}.",
                    playerId);

                throw;
            }
            catch (Exception ex) when (ex is EntityException || ex is DbUpdateException)
            {
                _logger.LogError(
                    ex,
                    "Unable to retrieve profile for player {PlayerId}.",
                    playerId);

                throw;
            }
        }

        /// <summary>
        /// Updates the username while preserving the existing picture.
        /// </summary>
        /// <param name="playerId">The account identifier.</param>
        /// <param name="username">The requested username.</param>
        public UpdateProfileStatus UpdateProfile(int playerId, string username)
        {
            return UpdateProfileCore(playerId, username, false, null);
        }

        /// <summary>
        /// Updates the username and validates an explicit picture replacement.
        /// </summary>
        /// <param name="playerId">The account identifier.</param>
        /// <param name="username">The requested username.</param>
        /// <param name="replacementPicture">The replacement snapshot; null is invalid.</param>
        public UpdateProfileStatus UpdateProfile(int playerId, string username, ProfilePicture replacementPicture)
        {
            if (replacementPicture == null)
            {
                throw new ArgumentNullException(nameof(replacementPicture));
            }
            return UpdateProfileCore(playerId, username, true, replacementPicture);
        }

        private UpdateProfileStatus UpdateProfileCore(int playerId, string username,
            bool replacePicture, ProfilePicture picture)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            string newProfilePictureFileName = null;
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
                        return UpdateProfileStatus.PlayerNotFound;
                    }

                    bool usernameChanged =
                        player.Username != username;

                    if (!usernameChanged &&
                        !replacePicture)
                    {
                        return UpdateProfileStatus.SameUsername;
                    }

                    if (usernameChanged)
                    {
                        UpdateProfileStatus
                            usernameValidationFailure =
                                ValidateUsername(
                                    username,
                                    playerRepository);

                        if (usernameValidationFailure !=
                            UpdateProfileStatus.None)
                        {
                            return usernameValidationFailure;
                        }
                    }

                    if (replacePicture)
                    {
                        UpdateProfileStatus
                            profilePictureValidationFailure =
                                ValidateProfilePicture(
                                    picture);

                        if (profilePictureValidationFailure !=
                            UpdateProfileStatus.None)
                        {
                            return profilePictureValidationFailure;
                        }
                    }

                    string previousProfilePictureFileName =
                        player.ProfilePicture;

                    if (replacePicture)
                    {
                        newProfilePictureFileName =
                            _profilePictureStorage
                                .GenerateFileName(
                                    playerId,
                                    picture.OriginalFileName);

                        _profilePictureStorage.SaveProfilePicture(
                            newProfilePictureFileName,
                            picture.ImageData);
                    }

                    if (usernameChanged)
                    {
                        bool usernameUpdated =
                            playerRepository.UpdateUsername(
                                playerId,
                                username);

                        if (!usernameUpdated)
                        {
                            DeleteNewProfilePictureIfNecessary(
                                newProfilePictureFileName);

                            return UpdateProfileStatus.PlayerNotFound;
                        }
                    }

                    if (replacePicture)
                    {
                        bool profilePictureUpdated =
                            playerRepository.UpdateProfilePicture(
                                playerId,
                                newProfilePictureFileName);

                        if (!profilePictureUpdated)
                        {
                            DeleteNewProfilePictureIfNecessary(
                                newProfilePictureFileName);

                            return UpdateProfileStatus.PlayerNotFound;
                        }
                    }

                    context.SaveChanges();
                    changesCommitted = true;

                    if (replacePicture)
                    {
                        DeletePreviousProfilePicture(
                            previousProfilePictureFileName);
                    }

                    return UpdateProfileStatus.None;
                }
            }
            catch (Exception ex)
            {
                if (!changesCommitted)
                {
                    DeleteNewProfilePictureIfNecessary(newProfilePictureFileName);
                }

                _logger.LogError(
                    ex,
                    "Unable to update profile for player {PlayerId}.",
                    playerId);
                throw;
            }
        }

        private UpdateProfileStatus ValidateUsername(
            string username,
            IPlayerRepository playerRepository)
        {
            if (!AccountValidator.IsUsernameValid(username))
            {
                return UpdateProfileStatus
                    .InvalidUsername;
            }

            if (playerRepository.UsernameExists(
                username))
            {
                return UpdateProfileStatus
                    .UsernameAlreadyExists;
            }

            return UpdateProfileStatus.None;
        }

        private UpdateProfileStatus
            ValidateProfilePicture(
                ProfilePicture picture)
        {
            if (picture == null || picture.ImageData == null ||
                picture.ImageData.Length == 0)
            {
                return UpdateProfileStatus
                    .InvalidProfilePicture;
            }

            if (!ProfilePictureValidator.HasValidExtension(
                picture.OriginalFileName))
            {
                return UpdateProfileStatus
                    .InvalidProfilePictureFormat;
            }

            if (!ProfilePictureValidator.HasValidFileSize(
                picture.ImageData))
            {
                return UpdateProfileStatus
                    .ProfilePictureTooLarge;
            }

            return UpdateProfileStatus.None;
        }

        private void DeleteNewProfilePictureIfNecessary(
            string profilePictureFileName)
        {
            if (string.IsNullOrWhiteSpace(
                profilePictureFileName))
            {
                return;
            }

            _profilePictureStorage.TryDeleteProfilePicture(
                profilePictureFileName);
        }

        private void DeletePreviousProfilePicture(
            string profilePictureFileName)
        {
            if (string.IsNullOrWhiteSpace(
                profilePictureFileName))
            {
                return;
            }

            bool wasDeleted =
                _profilePictureStorage
                    .TryDeleteProfilePicture(
                        profilePictureFileName);

            if (!wasDeleted)
            {
                _logger.LogWarning(
                    "The previous profile picture file " +
                    "{ProfilePictureFileName} could not be deleted.",
                    profilePictureFileName);
            }
        }

    }
}
