using System;
using System.IO;

using Microsoft.Extensions.Logging;

namespace DominoAllFives.BusinessLogic.Storage
{
    /// <summary>
    /// Manages the local storage of player profile pictures.
    /// </summary>
    public class ProfilePictureStorage
    {
        private const string ApplicationFolderName = "DominoAllFives";
        private const string ProfilePicturesFolderName = "ProfilePictures";

        private readonly ILogger<ProfilePictureStorage> _logger;

        public ProfilePictureStorage(
            ILogger<ProfilePictureStorage> logger)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the data of a stored profile picture.
        /// </summary>
        /// <param name="profilePictureFileName">
        /// The name of the stored profile picture file.
        /// </param>
        /// <returns>
        /// The profile picture data, or an empty array when the picture
        /// is not available.
        /// </returns>
        public byte[] GetProfilePictureData(
            string profilePictureFileName)
        {
            if (string.IsNullOrWhiteSpace(profilePictureFileName))
            {
                return Array.Empty<byte>();
            }

            string profilePicturePath =
                GetProfilePicturePath(profilePictureFileName);

            if (!File.Exists(profilePicturePath))
            {
                return Array.Empty<byte>();
            }

            try
            {
                return File.ReadAllBytes(profilePicturePath);
            }
            catch (Exception ex) when (
                ex is UnauthorizedAccessException ||
                ex is PathTooLongException ||
                ex is DirectoryNotFoundException ||
                ex is IOException)
            {
                _logger.LogWarning(
                    ex,
                    "Unable to read profile picture file {ProfilePictureFileName}.",
                    profilePictureFileName);

                return Array.Empty<byte>();
            }
        }

        /// <summary>
        /// Saves profile picture data using the specified file name.
        /// </summary>
        /// <param name="profilePictureFileName">
        /// The name used to store the profile picture.
        /// </param>
        /// <param name="imageData">
        /// The profile picture data.
        /// </param>
        public void SaveProfilePicture(
            string profilePictureFileName,
            byte[] imageData)
        {
            string profilePicturesDirectory =
                GetProfilePicturesDirectory();

            if (!Directory.Exists(profilePicturesDirectory))
            {
                Directory.CreateDirectory(profilePicturesDirectory);
            }

            string profilePicturePath =
                GetProfilePicturePath(profilePictureFileName);

            File.WriteAllBytes(
                profilePicturePath,
                imageData);
        }

        /// <summary>
        /// Deletes a stored profile picture when it exists.
        /// </summary>
        /// <param name="profilePictureFileName">
        /// The name of the stored profile picture file.
        /// </param>
        public void DeleteProfilePicture(
            string profilePictureFileName)
        {
            if (string.IsNullOrWhiteSpace(profilePictureFileName))
            {
                return;
            }

            string profilePicturePath =
                GetProfilePicturePath(profilePictureFileName);

            if (!File.Exists(profilePicturePath))
            {
                return;
            }

            File.Delete(profilePicturePath);
        }

        /// <summary>
        /// Attempts to delete a stored profile picture during a cleanup operation.
        /// </summary>
        /// <param name="profilePictureFileName">
        /// The name of the stored profile picture file.
        /// </param>
        /// <returns>
        /// True when the file does not exist or was successfully deleted;
        /// otherwise, false.
        /// </returns>
        public bool TryDeleteProfilePicture(
            string profilePictureFileName)
        {
            if (string.IsNullOrWhiteSpace(profilePictureFileName))
            {
                return true;
            }

            try
            {
                DeleteProfilePicture(profilePictureFileName);
                return true;
            }
            catch (Exception ex) when (
                ex is UnauthorizedAccessException ||
                ex is PathTooLongException ||
                ex is DirectoryNotFoundException ||
                ex is IOException)
            {
                _logger.LogWarning(
                    ex,
                    "Unable to delete profile picture file {ProfilePictureFileName}.",
                    profilePictureFileName);

                return false;
            }
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

        private string GetProfilePicturePath(
            string profilePictureFileName)
        {
            return Path.Combine(
                GetProfilePicturesDirectory(),
                profilePictureFileName);
        }
    }
}