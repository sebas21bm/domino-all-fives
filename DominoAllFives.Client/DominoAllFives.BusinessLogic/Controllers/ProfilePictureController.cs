using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.IO;
using DominoAllFives.BusinessLogic.Validation;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;


namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Coordinates profile picture operations for player accounts.
    /// </summary>
    public class ProfilePictureController
    {
        private const string ApplicationFolderName =
            "DominoAllFives";
        private const string ProfilePicturesFolderName =
            "ProfilePictures";

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
        public ProfilePictureResultDto SetProfilePicture(ProfilePictureDto profilePicture)
        {
            if (profilePicture == null ||
                profilePicture.PlayerId <= 0 ||
                profilePicture.ImageData == null ||
                profilePicture.ImageData.Length == 0)
            {
                return CreateFailureResult(ProfilePictureFailureReason.InvalidImage);
            }

            if (!ProfilePictureValidator.HasValidExtension(profilePicture.FileName))
            {
                return CreateFailureResult(ProfilePictureFailureReason.InvalidFormat);
            }

            if (!ProfilePictureValidator.HasValidFileSize(profilePicture.ImageData))
            {
                return CreateFailureResult(ProfilePictureFailureReason.FileTooLarge);
            }

            string destinationFilePath = null;
            string previousProfilePicture = null;
            string profilePictureFileName = null;


            try
            {
                using (DominoAllFivesEntities context =
                    new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository =
                        new PlayerRepository(context);

                    Player player = playerRepository.GetById(profilePicture.PlayerId);

                    if (player == null)
                    {
                        return CreateFailureResult(ProfilePictureFailureReason.PlayerNotFound);
                    }
                    
                    profilePictureFileName = 
                        GenerateProfilePictureFileName(profilePicture.PlayerId,
                                                    profilePicture.FileName);

                    string profilePicturesDirectory = GetProfilePicturesDirectory();

                    if (!Directory.Exists(profilePicturesDirectory))
                    {
                        Directory.CreateDirectory(profilePicturesDirectory);
                    }

                    destinationFilePath =
                        Path.Combine(profilePicturesDirectory, profilePictureFileName);

                    File.WriteAllBytes(destinationFilePath, profilePicture.ImageData);

                    previousProfilePicture = player.ProfilePicture;

                    bool wasUpdated = playerRepository.UpdateProfilePicture(
                                                                        profilePicture.PlayerId,
                                                                        profilePictureFileName);

                    if (!wasUpdated)
                    {
                        DeleteFileIfExists(destinationFilePath);
                        return CreateFailureResult(ProfilePictureFailureReason.PlayerNotFound);
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || 
                                       ex is PathTooLongException || 
                                       ex is DirectoryNotFoundException || 
                                       ex is IOException ||
                                       ex is EntityException ||
                                       ex is DbUpdateException)
            {
                TryDeleteFile(destinationFilePath);

                return CreateFailureResult(ProfilePictureFailureReason.ServiceUnavailable);
            }

            DeletePreviousProfilePicture(previousProfilePicture, profilePictureFileName);

            return new ProfilePictureResultDto
            {
                IsSuccessful = true,
                FailureReason = ProfilePictureFailureReason.None
            };
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

            string previousProfilePicture = null;

            try
            {
                using (DominoAllFivesEntities context =
                    new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository =
                        new PlayerRepository(context);

                    Player player = playerRepository.GetById(playerId);

                    if (player == null)
                    {
                        return CreateFailureResult(
                            ProfilePictureFailureReason.PlayerNotFound);
                    }

                    previousProfilePicture = player.ProfilePicture;

                    if (string.IsNullOrWhiteSpace(previousProfilePicture))
                    {
                        return CreateSuccessResult();
                    }

                    bool wasUpdated = playerRepository.UpdateProfilePicture(playerId, null);

                    if (!wasUpdated)
                    {
                        return CreateFailureResult(ProfilePictureFailureReason.PlayerNotFound);
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex) when (
                ex is EntityException ||
                ex is DbUpdateException)
            {
                return CreateFailureResult(ProfilePictureFailureReason.ServiceUnavailable);
            }

            DeleteStoredProfilePicture(previousProfilePicture);

            return CreateSuccessResult();
        }

        /// <summary>
        /// Generates a unique file name for a profile picture.
        /// </summary>
        private string GenerateProfilePictureFileName(
            int playerId,
            string originalFileName)
        {
            string extension =
                Path.GetExtension(originalFileName)
                    .ToLowerInvariant();

            return string.Format(
                "player_{0}_{1}{2}",
                playerId,
                Guid.NewGuid().ToString("N"),
                extension);
        }

        /// <summary>
        /// Gets the directory used to store profile pictures.
        /// </summary>
        private string GetProfilePicturesDirectory()
        {
            string localApplicationData =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData);

            return Path.Combine(
                localApplicationData,
                ApplicationFolderName,
                ProfilePicturesFolderName);
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

            DeleteStoredProfilePicture(
                previousProfilePicture);
        }

        /// <summary>
        /// Deletes a stored profile picture.
        /// </summary>
        private void DeleteStoredProfilePicture(
            string profilePictureFileName)
        {
            if (string.IsNullOrWhiteSpace(
                    profilePictureFileName))
            {
                return;
            }

            string profilePictureFilePath =
                Path.Combine(
                    GetProfilePicturesDirectory(),
                    profilePictureFileName);

            DeleteFileIfExists(
                profilePictureFilePath);
        }

        /// <summary>
        /// Deletes a file when it exists.
        /// </summary>
        private void DeleteFileIfExists(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) ||
                !File.Exists(filePath))
            {
                return;
            }
            
            File.Delete(filePath);
        }

        /// <summary>
        /// Attempts to delete a file used during cleanup.
        /// </summary>
        private bool TryDeleteFile(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) ||
                !File.Exists(filePath))
            {
                return true;
            }

            try
            {
                File.Delete(filePath);
                return true;
            }
            catch (Exception ex) when (ex is IOException ||
                                       ex is UnauthorizedAccessException)
            {
                return false;
            }
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