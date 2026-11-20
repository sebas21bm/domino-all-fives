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
    /// Coordinates operations related to player profile information.
    /// </summary>
    public class ProfileController
    {
        private readonly ProfilePictureStorage _profilePictureStorage;

        /// <summary>Initializes profile operations with storage for optional pictures.</summary>
        /// <param name = "profilePictureStorage">The profile picture storage.</param>
        /// <exception cref = "ArgumentNullException">The storage is null.</exception>
        public ProfileController(ProfilePictureStorage profilePictureStorage)
        {
            _profilePictureStorage = profilePictureStorage ?? throw new ArgumentNullException(
                nameof(profilePictureStorage));
        }

        /// <summary>
        /// Gets the profile information of a player.
        /// </summary>
        /// <param name = "playerId">The account identifier.</param>
        /// <returns>The domain value produced by the operation.</returns>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// The player identifier is invalid.
        /// </exception>
        /// <exception cref = "ProfileRuleException">The player is unavailable.</exception>
        /// <exception cref = "InvalidOperationException">
        /// The required statistics are missing.
        /// </exception>
        public PlayerProfile GetProfile(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            using (DominoAllFivesEntities context = new DominoAllFivesEntities())
            {
                IPlayerRepository playerRepository = new PlayerRepository(context);
                Player player = playerRepository.GetById(playerId);
                if (player == null || player.IsDeleted)
                {
                    throw new ProfileRuleException(ProfileIssue.PlayerNotFound);
                }

                if (player.PlayerStats == null)
                {
                    throw new InvalidOperationException("The player has no required statistics.");
                }

                PlayerStatistics statistics = PlayerStatistics.FromStoredStatistics(
                    player.PlayerStats.GamesWon,
                    player.PlayerStats.TotalPointsScored,
                    player.PlayerStats.GamesPlayed);
                byte[] pictureData = _profilePictureStorage.GetProfilePictureData(
                    player.ProfilePicture);
                ProfilePicture picture = ProfilePicture.FromStoredContent(pictureData);
                return PlayerProfile.FromStoredProfile(player.Username, statistics, picture);
            }
        }

        /// <summary>
        /// Updates the username while preserving the existing picture.
        /// </summary>
        /// <param name = "playerId">The account identifier.</param>
        /// <param name = "username">The requested username.</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// The player identifier is invalid.
        /// </exception>
        /// <exception cref = "ProfileRuleException">
        /// The requested update was rejected.
        /// </exception>
        public void UpdateProfileName(int playerId, string username)
        {
            UpdateProfileCore(playerId, username, false, null);
        }

        /// <summary>
        /// Updates the username and validates an explicit picture replacement.
        /// </summary>
        /// <param name = "playerId">The account identifier.</param>
        /// <param name = "username">The requested username.</param>
        /// <param name = "replacementPicture">The replacement snapshot; null is invalid.</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// The player identifier is invalid.
        /// </exception>
        /// <exception cref = "ArgumentNullException">The replacement picture is null.</exception>
        /// <exception cref = "ProfileRuleException">
        /// The requested update was rejected.
        /// </exception>
        public void UpdateProfileWithPicture(
            int playerId,
            string username,
            ProfilePicture replacementPicture)
        {
            if (replacementPicture == null)
            {
                throw new ArgumentNullException(nameof(replacementPicture));
            }

            UpdateProfileCore(playerId, username, true, replacementPicture);
        }

        private void UpdateProfileCore(
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
                    UpdateStoredProfile(context, update);
                }
            }
            finally
            {
                if (!update.ChangesCommitted)
                {
                    _profilePictureStorage.TryDeleteProfilePicture(update.NewPictureFileName);
                }
            }
        }

        private void UpdateStoredProfile(
            DominoAllFivesEntities context,
            ProfileUpdate update)
        {
            IPlayerRepository repository = new PlayerRepository(context);
            Player player = repository.GetById(update.PlayerId);
            if (player == null || player.IsDeleted)
            {
                throw new ProfileRuleException(ProfileIssue.PlayerNotFound);
            }

            ValidateRequestedChanges(player, repository, update);

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

            ApplyStoredValues(repository, player, update);

            context.SaveChanges();
            update.ChangesCommitted = true;
            if (update.ReplacePicture)
            {
                _profilePictureStorage.TryDeleteProfilePicture(previousPictureFileName);
            }
        }

        private static void ValidateRequestedChanges(
            Player player,
            IPlayerRepository repository,
            ProfileUpdate update)
        {
            bool usernameChanged = player.Username != update.Username;
            if (!usernameChanged && !update.ReplacePicture)
            {
                throw new ProfileRuleException(ProfileIssue.SameUsername);
            }

            if (usernameChanged)
            {
                ValidateUsername(update.Username, repository);
            }

            if (update.ReplacePicture)
            {
                ValidateProfilePicture(update.Picture);
            }
        }

        private static void ApplyStoredValues(
            IPlayerRepository repository,
            Player player,
            ProfileUpdate update)
        {
            if (player.Username != update.Username
                && !repository.UpdateUsername(update.PlayerId, update.Username))
            {
                throw new ProfileRuleException(ProfileIssue.PlayerNotFound);
            }

            if (update.ReplacePicture
                && !repository.UpdateProfilePicture(update.PlayerId, update.NewPictureFileName))
            {
                throw new ProfileRuleException(ProfileIssue.PlayerNotFound);
            }
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

        private static void ValidateUsername(
            string username,
            IPlayerRepository playerRepository)
        {
            if (!AccountValidator.IsUsernameValid(username))
            {
                throw new ProfileRuleException(ProfileIssue.InvalidUsername);
            }

            if (playerRepository.UsernameExists(username))
            {
                throw new ProfileRuleException(ProfileIssue.UsernameAlreadyExists);
            }
        }

        private static void ValidateProfilePicture(ProfilePicture picture)
        {
            if (picture == null || picture.ImageData == null || picture.ImageData.Length == 0)
            {
                throw new ProfileRuleException(ProfileIssue.InvalidProfilePicture);
            }

            if (!ProfilePictureValidator.HasValidExtension(picture.OriginalFileName))
            {
                throw new ProfileRuleException(
                    ProfileIssue.InvalidProfilePictureFormat);
            }

            if (!ProfilePictureValidator.HasValidFileSize(picture.ImageData))
            {
                throw new ProfileRuleException(ProfileIssue.ProfilePictureTooLarge);
            }
        }
    }
}
