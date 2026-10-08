using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.BusinessLogic.Validation;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.IO;

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
        public ProfileDto GetProfile(int playerId)
        {
            if (playerId <= 0)
            {
                return CreateProfileFailureResult(
                    RetrieveInformationFailureReason.NotFound);
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

                    if (player == null ||
                        player.PlayerStats == null)
                    {
                        return CreateProfileFailureResult(
                            RetrieveInformationFailureReason.NotFound);
                    }

                    return new ProfileDto
                    {
                        Username = player.Username,
                        ProfilePictureData =
                            _profilePictureStorage
                                .GetProfilePictureData(
                                    player.ProfilePicture),
                        Wins = player.PlayerStats.GamesWon,
                        TotalPoints =
                            player.PlayerStats.TotalPointsScored,
                        GamesPlayed =
                            player.PlayerStats.GamesPlayed,
                        FailureReason =
                            RetrieveInformationFailureReason.None
                    };
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to retrieve profile for player {PlayerId}.",
                    playerId);

                return CreateProfileFailureResult(
                    RetrieveInformationFailureReason
                        .ServiceUnavailable);
            }
            catch (EntityException ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to retrieve profile for player {PlayerId}.",
                    playerId);

                return CreateProfileFailureResult(
                    RetrieveInformationFailureReason
                        .ServiceUnavailable);
            }
        }

        /// <summary>
        /// Updates the editable profile information of a player.
        /// </summary>
        public UpdateProfileResultDto UpdateProfile(
            UpdateProfileDto profileData)
        {
            if (profileData == null ||
                profileData.PlayerId <= 0)
            {
                return CreateUpdateFailureResult(
                    UpdateProfileFailureReason.PlayerNotFound);
            }

            string newProfilePictureFileName = null;

            try
            {
                using (DominoAllFivesEntities context =
                    new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository =
                        new PlayerRepository(context);

                    Player player =
                        playerRepository.GetById(
                            profileData.PlayerId);

                    if (player == null)
                    {
                        return CreateUpdateFailureResult(
                            UpdateProfileFailureReason
                                .PlayerNotFound);
                    }

                    bool usernameChanged =
                        player.Username != profileData.Username;

                    bool profilePictureChanged =
                        profileData.ProfilePictureChanged;

                    if (!usernameChanged &&
                        !profilePictureChanged)
                    {
                        return CreateUpdateFailureResult(
                            UpdateProfileFailureReason
                                .SameUsername);
                    }

                    if (usernameChanged)
                    {
                        UpdateProfileFailureReason
                            usernameValidationFailure =
                                ValidateUsername(
                                    profileData,
                                    playerRepository);

                        if (usernameValidationFailure !=
                            UpdateProfileFailureReason.None)
                        {
                            return CreateUpdateFailureResult(
                                usernameValidationFailure);
                        }
                    }

                    if (profilePictureChanged)
                    {
                        UpdateProfileFailureReason
                            profilePictureValidationFailure =
                                ValidateProfilePicture(
                                    profileData);

                        if (profilePictureValidationFailure !=
                            UpdateProfileFailureReason.None)
                        {
                            return CreateUpdateFailureResult(
                                profilePictureValidationFailure);
                        }
                    }

                    string previousProfilePictureFileName =
                        player.ProfilePicture;

                    if (profilePictureChanged)
                    {
                        newProfilePictureFileName =
                            _profilePictureStorage
                                .GenerateFileName(
                                    profileData.PlayerId,
                                    profileData
                                        .ProfilePictureFileName);

                        _profilePictureStorage.SaveProfilePicture(
                            newProfilePictureFileName,
                            profileData.ProfilePictureData);
                    }

                    if (usernameChanged)
                    {
                        bool usernameUpdated =
                            playerRepository.UpdateUsername(
                                profileData.PlayerId,
                                profileData.Username);

                        if (!usernameUpdated)
                        {
                            DeleteNewProfilePictureIfNecessary(
                                newProfilePictureFileName);

                            return CreateUpdateFailureResult(
                                UpdateProfileFailureReason
                                    .PlayerNotFound);
                        }
                    }

                    if (profilePictureChanged)
                    {
                        bool profilePictureUpdated =
                            playerRepository.UpdateProfilePicture(
                                profileData.PlayerId,
                                newProfilePictureFileName);

                        if (!profilePictureUpdated)
                        {
                            DeleteNewProfilePictureIfNecessary(
                                newProfilePictureFileName);

                            return CreateUpdateFailureResult(
                                UpdateProfileFailureReason
                                    .PlayerNotFound);
                        }
                    }

                    context.SaveChanges();

                    if (profilePictureChanged)
                    {
                        DeletePreviousProfilePicture(
                            previousProfilePictureFileName);
                    }

                    return CreateUpdateSuccessResult();
                }
            }
            catch (SqlException ex)
            {
                DeleteNewProfilePictureIfNecessary(
                    newProfilePictureFileName);

                _logger.LogError(
                    ex,
                    "Unable to update profile for player {PlayerId}.",
                    profileData.PlayerId);

                return CreateUpdateFailureResult(
                    UpdateProfileFailureReason
                        .ServiceUnavailable);
            }
            catch (EntityException ex)
            {
                DeleteNewProfilePictureIfNecessary(
                    newProfilePictureFileName);

                _logger.LogError(
                    ex,
                    "Unable to update profile for player {PlayerId}.",
                    profileData.PlayerId);

                return CreateUpdateFailureResult(
                    UpdateProfileFailureReason
                        .ServiceUnavailable);
            }
            catch (Exception ex) when (
                ex is UnauthorizedAccessException ||
                ex is PathTooLongException ||
                ex is DirectoryNotFoundException ||
                ex is IOException)
            {
                DeleteNewProfilePictureIfNecessary(
                    newProfilePictureFileName);

                _logger.LogError(
                    ex,
                    "Unable to update profile picture for player {PlayerId}.",
                    profileData.PlayerId);

                return CreateUpdateFailureResult(
                    UpdateProfileFailureReason
                        .ServiceUnavailable);
            }
        }

        private UpdateProfileFailureReason ValidateUsername(
            UpdateProfileDto profileData,
            IPlayerRepository playerRepository)
        {
            if (!UsernameValidator.IsValid(
                profileData.Username))
            {
                return UpdateProfileFailureReason
                    .InvalidUsername;
            }

            if (playerRepository.UsernameExists(
                profileData.Username))
            {
                return UpdateProfileFailureReason
                    .UsernameAlreadyExists;
            }

            return UpdateProfileFailureReason.None;
        }

        private UpdateProfileFailureReason
            ValidateProfilePicture(
                UpdateProfileDto profileData)
        {
            if (profileData.ProfilePictureData == null ||
                profileData.ProfilePictureData.Length == 0)
            {
                return UpdateProfileFailureReason
                    .InvalidProfilePicture;
            }

            if (!ProfilePictureValidator.HasValidExtension(
                profileData.ProfilePictureFileName))
            {
                return UpdateProfileFailureReason
                    .InvalidProfilePictureFormat;
            }

            if (!ProfilePictureValidator.HasValidFileSize(
                profileData.ProfilePictureData))
            {
                return UpdateProfileFailureReason
                    .ProfilePictureTooLarge;
            }

            return UpdateProfileFailureReason.None;
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

        private ProfileDto CreateProfileFailureResult(
            RetrieveInformationFailureReason reason)
        {
            return new ProfileDto
            {
                Username = string.Empty,
                ProfilePictureData = Array.Empty<byte>(),
                Wins = 0,
                TotalPoints = 0,
                GamesPlayed = 0,
                FailureReason = reason
            };
        }

        private UpdateProfileResultDto
            CreateUpdateSuccessResult()
        {
            return new UpdateProfileResultDto
            {
                IsSuccessful = true,
                FailureReason =
                    UpdateProfileFailureReason.None
            };
        }

        private UpdateProfileResultDto
            CreateUpdateFailureResult(
                UpdateProfileFailureReason reason)
        {
            return new UpdateProfileResultDto
            {
                IsSuccessful = false,
                FailureReason = reason
            };
        }
    }
}