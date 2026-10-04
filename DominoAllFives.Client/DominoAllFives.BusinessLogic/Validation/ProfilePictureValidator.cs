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
            AllowedExtensions =
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
        public static bool HasValidExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            string extension =
                Path.GetExtension(fileName);

            return AllowedExtensions.Contains(extension);
        }

        /// <summary>
        /// Determines whether a profile picture is within the
        /// maximum allowed file size.
        /// </summary>
        public static bool HasValidFileSize(byte[] imageData)
        {
            return imageData != null && 
                   imageData.Length > 0 && 
                   imageData.Length <= MaximumFileSizeInBytes;
        }

        /// <summary>
        /// Determines whether the selected profile picture
        /// satisfies all profile picture business rules.
        /// </summary>
        public static bool IsValid(string fileName, byte[] imageData)
        {
            return HasValidExtension(fileName) &&
                   HasValidFileSize(imageData);
        }
    }
}