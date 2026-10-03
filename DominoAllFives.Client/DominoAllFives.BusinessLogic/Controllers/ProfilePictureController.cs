using System;
using System.IO;

using DominoAllFives.BusinessLogic.Validation;
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
        /// Assigns a profile picture to a player account.
        /// </summary>
        /// <param name="playerId">
        /// The identifier of the player.
        /// </param>
        /// <param name="sourceFilePath">
        /// The path of the selected profile picture.
        /// </param>
        /// <returns>
        /// True when the profile picture was successfully stored
        /// and assigned; otherwise, false.
        /// </returns>
        public bool SetProfilePicture(
            int playerId,
            string sourceFilePath)
        {
            if (playerId <= 0 ||
                !ProfilePictureValidator.IsValid(sourceFilePath))
            {
                return false;
            }

            string destinationFilePath = null;

            try
            {
                string profilePictureFileName =
                    GenerateProfilePictureFileName(
                        playerId,
                        sourceFilePath);

                string profilePicturesDirectory =
                    GetProfilePicturesDirectory();

                Directory.CreateDirectory(
                    profilePicturesDirectory);

                destinationFilePath =
                    Path.Combine(
                        profilePicturesDirectory,
                        profilePictureFileName);

                File.Copy(
                    sourceFilePath,
                    destinationFilePath,
                    false);

                using (DominoAllFivesEntities context =
                    new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository =
                        new PlayerRepository(context);

                    Player player =
                        playerRepository.GetById(playerId);

                    if (player == null)
                    {
                        DeleteFileIfExists(
                            destinationFilePath);

                        return false;
                    }

                    string previousProfilePicture =
                        player.ProfilePicture;

                    player.ProfilePicture =
                        profilePictureFileName;

                    context.SaveChanges();

                    DeletePreviousProfilePicture(
                        previousProfilePicture,
                        profilePictureFileName);
                }

                return true;
            }
            catch
            {
                DeleteFileIfExists(
                    destinationFilePath);

                return false;
            }
        }

        /// <summary>
        /// Removes the profile picture associated with a player.
        /// </summary>
        /// <param name="playerId">
        /// The identifier of the player.
        /// </param>
        /// <returns>
        /// True when the profile picture was successfully removed;
        /// otherwise, false.
        /// </returns>
        public bool RemoveProfilePicture(int playerId)
        {
            if (playerId <= 0)
            {
                return false;
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
                        return false;
                    }

                    string previousProfilePicture =
                        player.ProfilePicture;

                    player.ProfilePicture = null;

                    context.SaveChanges();

                    DeleteStoredProfilePicture(
                        previousProfilePicture);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateProfilePictureFileName(
            int playerId,
            string sourceFilePath)
        {
            string extension =
                Path.GetExtension(sourceFilePath)
                    .ToLowerInvariant();

            return string.Format(
                "player_{0}_{1}{2}",
                playerId,
                Guid.NewGuid().ToString("N"),
                extension);
        }

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

        private void DeleteFileIfExists(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) ||
                !File.Exists(filePath))
            {
                return;
            }

            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // File cleanup failure does not replace
                // the original operation result.
            }
        }
    }
}