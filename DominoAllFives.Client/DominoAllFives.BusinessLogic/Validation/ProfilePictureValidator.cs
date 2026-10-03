using System;
using System.Collections.Generic;
using System.IO;

namespace DominoAllFives.BusinessLogic.Validation
{
    /// <summary>
    /// Validates profile picture business rules.
    /// </summary>
    public static class ProfilePictureValidator
    {
        private const long MaximumFileSizeInBytes =
            1024 * 1024;

        private static readonly HashSet<string>
            allowedExtensions = 
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

        /// <summary>
        /// Determines whether a profile picture has an allowed
        /// file extension.
        /// </summary>
        /// <param name="filePath">
        /// The path of the profile picture.
        /// </param>
        /// <returns>
        /// True when the extension is allowed; otherwise, false.
        /// </returns>
        public static bool HasValidExtension(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            string extension =
                Path.GetExtension(filePath);

            return allowedExtensions.Contains(extension);
        }

        /// <summary>
        /// Determines whether a profile picture is within the
        /// maximum allowed file size.
        /// </summary>
        /// <param name="filePath">
        /// The path of the profile picture.
        /// </param>
        /// <returns>
        /// True when the file does not exceed one megabyte;
        /// otherwise, false.
        /// </returns>
        public static bool HasValidFileSize(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) ||
                !File.Exists(filePath))
            {
                return false;
            }

            FileInfo fileInformation =
                new FileInfo(filePath);

            return fileInformation.Length <=
                   MaximumFileSizeInBytes;
        }

        /// <summary>
        /// Determines whether the selected profile picture
        /// satisfies all profile picture business rules.
        /// </summary>
        /// <param name="filePath">
        /// The path of the selected profile picture.
        /// </param>
        /// <returns>
        /// True when the profile picture is valid;
        /// otherwise, false.
        /// </returns>
        public static bool IsValid(string filePath)
        {
            return HasValidExtension(filePath) &&
                   HasValidFileSize(filePath);
        }
    }
}