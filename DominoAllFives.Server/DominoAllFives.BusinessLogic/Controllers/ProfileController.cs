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
using Player = DominoAllFives.DataAccess.Models.Player;
using DominoAllFives.DataAccess.Repositories;
using Microsoft.Extensions.Logging;

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
            _profilePictureStorage = profilePictureStorage ?? throw new ArgumentNullException(
                nameof(
                profilePictureStorage));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the profile information of a player.
        /// </summary>
        /// <param name = "playerId">The account identifier.</param>
        /// <returns>The domain value produced by the operation.</returns>
        public PlayerProfile GetProfile(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            try
            {
                using (DominoAllFivesEntities context = new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository = new PlayerRepository(context);
                    Player player = playerRepository.GetById(playerId);
                    if (player == null)
                    {
                        return null;
                    }

                    if (player.PlayerStats == null)
                    {
                        throw new InvalidOperationException(
                            "The player has no required statistics.");
                    }

                    return PlayerProfile.FromStoredProfile(
                        player.Username,
                        PlayerStatistics.FromStoredStatistics(
                        player.PlayerStats.GamesWon,
                        player.PlayerStats.TotalPointsScored,
                        player.PlayerStats.GamesPlayed),
                        ProfilePicture.FromStoredContent(
                        _profilePictureStorage.GetProfilePictureData(
                        player.ProfilePicture)));
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Unable to retrieve profile for player {PlayerId}.", playerId);
                throw;
            }
            catch (EntityException ex)
            {
                _logger.LogError(ex, "Unable to retrieve profile for player {PlayerId}.", playerId);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Unable to retrieve profile for player {PlayerId}.", playerId);
                throw;
            }
        }

        /// <summary>
        /// Updates the username while preserving the existing picture.
        /// </summary>
        /// <param name = "playerId">The account identifier.</param>
        /// <param name = "username">The requested username.</param>
        public UpdateProfileStatus UpdateProfile(int playerId, string username)
        {
            return UpdateProfileCore(playerId, username, false, null);
        }

        /// <summary>
        /// Updates the username and validates an explicit picture replacement.
        /// </summary>
        /// <param name = "playerId">The account identifier.</param>
        /// <param name = "username">The requested username.</param>
        /// <param name = "replacementPicture">The replacement snapshot; null is invalid.</param>
        public UpdateProfileStatus UpdateProfile(
            int playerId,
            string username,
            ProfilePicture replacementPicture)
        {
            if (replacementPicture == null)
            {
                throw new ArgumentNullException(nameof(replacementPicture));
            }

            return UpdateProfileCore(playerId, username, true, replacementPicture);
        }

        private UpdateProfileStatus UpdateProfileCore(
            int playerId,
            string username,
            bool replacePicture,
            ProfilePicture picture)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            ProfileUpdate update = new ProfileUpdate(playerId, username, replacePicture, picture);
            try
            {
                using (DominoAllFivesEntities context = new DominoAllFivesEntities())
                {
                    return UpdateStoredProfile(context, update);
                }
            }
            catch (SqlException ex)
            {
                LogUpdateFailure(ex, playerId);
                throw;
            }
            catch (EntityException ex)
            {
                LogUpdateFailure(ex, playerId);
                throw;
            }
            catch (DbUpdateException ex)
            {
                LogUpdateFailure(ex, playerId);
                throw;
            }
            finally
            {
                if (!update.ChangesCommitted)
                {
                    DeleteNewProfilePictureIfNecessary(update.NewPictureFileName);
                }
            }
        }

        private UpdateProfileStatus UpdateStoredProfile(
            DominoAllFivesEntities context,
            ProfileUpdate update)
        {
            IPlayerRepository repository = new PlayerRepository(context);
            Player player = repository.GetById(update.PlayerId);
            if (player == null)
            {
                return UpdateProfileStatus.PlayerNotFound;
            }

            UpdateProfileStatus validation = ValidateRequestedChanges(player, repository, update);
            if (validation != UpdateProfileStatus.None)
            {
                return validation;
            }

            string previousPictureFileName = player.ProfilePicture;
            if (update.ReplacePicture)
            {
                update.NewPictureFileName = _profilePictureStorage.GenerateFileName(
                    update.PlayerId,
                    update.Picture.OriginalFileName);
                _profilePictureStorage.SaveProfilePicture(
                    update.NewPictureFileName,
                    update.Picture.ImageData);
            }

            if (!ApplyStoredValues(repository, player, update))
            {
                return UpdateProfileStatus.PlayerNotFound;
            }

            context.SaveChanges();
            update.ChangesCommitted = true;
            if (update.ReplacePicture)
            {
                DeletePreviousProfilePicture(previousPictureFileName);
            }

            return UpdateProfileStatus.None;
        }

        private UpdateProfileStatus ValidateRequestedChanges(
            Player player,
            IPlayerRepository repository,
            ProfileUpdate update)
        {
            bool usernameChanged = player.Username != update.Username;
            if (!usernameChanged && !update.ReplacePicture)
            {
                return UpdateProfileStatus.SameUsername;
            }

            if (usernameChanged)
            {
                UpdateProfileStatus status = ValidateUsername(update.Username, repository);
                if (status != UpdateProfileStatus.None)
                {
                    return status;
                }
            }

            return update.ReplacePicture
                ? ValidateProfilePicture(
                update.Picture) : UpdateProfileStatus.None;
        }

        private static bool ApplyStoredValues(
            IPlayerRepository repository,
            Player player,
            ProfileUpdate update)
        {
            if (player.Username != update.Username
                && !repository.UpdateUsername(
                update.PlayerId,
                update.Username))
            {
                return false;
            }

            if (update.ReplacePicture
                && !repository.UpdateProfilePicture(
                update.PlayerId,
                update.NewPictureFileName))
            {
                return false;
            }

            return true;
        }

        private void LogUpdateFailure(Exception ex, int playerId)
        {
            _logger.LogError(ex, "Unable to update profile for player {PlayerId}.", playerId);
        }

        private sealed class ProfileUpdate
        {
            public int PlayerId { get; }
            public string Username { get; }
            public bool ReplacePicture { get; }
            public ProfilePicture Picture { get; }
            public string NewPictureFileName { get; set; }
            public bool ChangesCommitted { get; set; }

            public ProfileUpdate(
                int playerId,
                string username,
                bool replacePicture,
                ProfilePicture picture)
            {
                PlayerId = playerId;
                Username = username;
                ReplacePicture = replacePicture;
                Picture = picture;
            }
        }

        private UpdateProfileStatus ValidateUsername(
            string username,
            IPlayerRepository playerRepository)
        {
            if (!AccountValidator.IsUsernameValid(username))
            {
                return UpdateProfileStatus.InvalidUsername;
            }

            if (playerRepository.UsernameExists(username))
            {
                return UpdateProfileStatus.UsernameAlreadyExists;
            }

            return UpdateProfileStatus.None;
        }

        private UpdateProfileStatus ValidateProfilePicture(ProfilePicture picture)
        {
            if (picture == null || picture.ImageData == null || picture.ImageData.Length == 0)
            {
                return UpdateProfileStatus.InvalidProfilePicture;
            }

            if (!ProfilePictureValidator.HasValidExtension(picture.OriginalFileName))
            {
                return UpdateProfileStatus.InvalidProfilePictureFormat;
            }

            if (!ProfilePictureValidator.HasValidFileSize(picture.ImageData))
            {
                return UpdateProfileStatus.ProfilePictureTooLarge;
            }

            return UpdateProfileStatus.None;
        }

        private void DeleteNewProfilePictureIfNecessary(string profilePictureFileName)
        {
            if (string.IsNullOrWhiteSpace(profilePictureFileName))
            {
                return;
            }

            _profilePictureStorage.TryDeleteProfilePicture(profilePictureFileName);
        }

        private void DeletePreviousProfilePicture(string profilePictureFileName)
        {
            if (string.IsNullOrWhiteSpace(profilePictureFileName))
            {
                return;
            }

            bool wasDeleted =
                _profilePictureStorage.TryDeleteProfilePicture(
                profilePictureFileName);
            if (!wasDeleted)
            {
                _logger.LogWarning(
                    "The previous profile picture file "
                        + "{ProfilePictureFileName} could not be deleted.",
                    profilePictureFileName);
            }
        }
    }
}
