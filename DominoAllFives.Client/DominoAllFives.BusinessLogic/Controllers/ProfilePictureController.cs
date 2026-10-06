using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.IO;

using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.BusinessLogic.Validation;
using DominoAllFives.Contracts.DTOs;
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
        /// <param name="profilePicture">
        /// Contains the player identifier, original file name,
        /// and image data.
        /// </param>
        /// <returns>
        /// The result of the profile picture operation.
        /// </returns>
        public ProfilePictureResultDto SetProfilePicture(
            ProfilePictureDto profilePicture)
        {
            if (profilePicture == null ||
                profilePicture.PlayerId <= 0 ||
                profilePicture.ImageData == null ||
                profilePicture.ImageData.Length == 0)
            {
                return CreateFailureResult(
                    ProfilePictureFailureReason.InvalidImage);
            }

            if (!ProfilePictureValidator.HasValidExtension(
                    profilePicture.FileName))
            {
                return CreateFailureResult(
                    ProfilePictureFailureReason.InvalidFormat);
            }

            if (!ProfilePictureValidator.HasValidFileSize(
                    profilePicture.ImageData))
            {
                return CreateFailureResult(
                    ProfilePictureFailureReason.FileTooLarge);
            }

            string previousProfilePicture = string.Empty;
            string profilePictureFileName = string.Empty;

            try
            {
                using (DominoAllFivesEntities context =
                    new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository =
                        new PlayerRepository(context);

                    Player player =
                        playerRepository.GetById(
                            profilePicture.PlayerId);

                    if (player == null)
                    {
                        return CreateFailureResult(
                            ProfilePictureFailureReason.PlayerNotFound);
                    }

                    profilePictureFileName =
                    _profilePictureStorage.GenerateFileName(
                        profilePicture.PlayerId,
                        profilePicture.FileName);

                    _profilePictureStorage.SaveProfilePicture(
                        profilePictureFileName,
                        profilePicture.ImageData);

                    previousProfilePicture =
                        player.ProfilePicture;

                    bool wasUpdated =
                        playerRepository.UpdateProfilePicture(
                            profilePicture.PlayerId,
                            profilePictureFileName);

                    if (!wasUpdated)
                    {
                        _profilePictureStorage.TryDeleteProfilePicture(
                            profilePictureFileName);

                        return CreateFailureResult(
                            ProfilePictureFailureReason.PlayerNotFound);
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex) when (
                ex is UnauthorizedAccessException ||
                ex is PathTooLongException ||
                ex is DirectoryNotFoundException ||
                ex is IOException)
            {
                _logger.LogError(
                    ex,
                    "Unable to store profile picture for player {PlayerId}.",
                    profilePicture.PlayerId);

                _profilePictureStorage.TryDeleteProfilePicture(
                    profilePictureFileName);

                return CreateFailureResult(
                    ProfilePictureFailureReason.ServiceUnavailable);
            }
            catch (Exception ex) when (
                ex is EntityException ||
                ex is DbUpdateException)
            {
                _logger.LogError(
                    ex,
                    "Unable to update profile picture for player {PlayerId}.",
                    profilePicture.PlayerId);

                _profilePictureStorage.TryDeleteProfilePicture(
                    profilePictureFileName);

                return CreateFailureResult(
                    ProfilePictureFailureReason.ServiceUnavailable);
            }

            DeletePreviousProfilePicture(
                previousProfilePicture,
                profilePictureFileName);

            return CreateSuccessResult();
        }

        /// <summary>
        /// Removes the profile picture assigned to a player.
        /// </summary>
        /// <param name="playerId">
        /// The identifier of the player whose profile picture
        /// will be removed.
        /// </param>
        /// <returns>
        /// The result of the profile picture operation.
        /// </returns>
        public ProfilePictureResultDto RemoveProfilePicture(
            int playerId)
        {
            if (playerId <= 0)
            {
                return CreateFailureResult(
                    ProfilePictureFailureReason.PlayerNotFound);
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
                        return CreateFailureResult(
                            ProfilePictureFailureReason.PlayerNotFound);
                    }

                    previousProfilePicture =
                        player.ProfilePicture;

                    if (string.IsNullOrWhiteSpace(
                            previousProfilePicture))
                    {
                        return CreateSuccessResult();
                    }

                    bool wasUpdated =
                        playerRepository.UpdateProfilePicture(
                            playerId,
                            null);

                    if (!wasUpdated)
                    {
                        return CreateFailureResult(
                            ProfilePictureFailureReason.PlayerNotFound);
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex) when (
                ex is EntityException ||
                ex is DbUpdateException)
            {
                _logger.LogError(
                    ex,
                    "Unable to remove profile picture for player {PlayerId}.",
                    playerId);

                return CreateFailureResult(
                    ProfilePictureFailureReason.ServiceUnavailable);
            }

            _profilePictureStorage.TryDeleteProfilePicture(
                previousProfilePicture);

            return CreateSuccessResult();
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

        /// <summary>
        /// Creates a successful profile picture result.
        /// </summary>
        private ProfilePictureResultDto CreateSuccessResult()
        {
            return new ProfilePictureResultDto
            {
                IsSuccessful = true,
                FailureReason = ProfilePictureFailureReason.None
            };
        }

        /// <summary>
        /// Creates a failed profile picture result.
        /// </summary>
        private ProfilePictureResultDto CreateFailureResult(
            ProfilePictureFailureReason failureReason)
        {
            return new ProfilePictureResultDto
            {
                IsSuccessful = false,
                FailureReason = failureReason
            };
        }
    }
}