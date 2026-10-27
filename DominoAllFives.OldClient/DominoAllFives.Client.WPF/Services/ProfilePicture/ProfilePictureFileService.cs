using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.IO;

namespace DominoAllFives.Client.WPF.Services
{
    /// <summary>
    /// Provides operations for selecting and reading
    /// profile picture files.
    /// </summary>
    public class ProfilePictureFileService
    {
        private readonly ILogger<ProfilePictureFileService> _logger;

        public ProfilePictureFileService(
            ILogger<ProfilePictureFileService> logger)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Allows the user to select a profile picture
        /// and reads its contents.
        /// </summary>
        public ProfilePictureFileResult SelectProfilePicture()
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter =
                    "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Multiselect = false
            };

            bool? result = fileDialog.ShowDialog();

            if (result != true)
            {
                return new ProfilePictureFileResult
                {
                    IsSuccessful = false,
                    WasCancelled = true
                };
            }

            try
            {
                return new ProfilePictureFileResult
                {
                    IsSuccessful = true,
                    WasCancelled = false,
                    ImageData = File.ReadAllBytes(fileDialog.FileName),
                    FileName = Path.GetFileName(fileDialog.FileName)
                };
            }
            catch (IOException ex)
            {
                _logger.LogError(
                    ex,
                    "Unable to read the selected profile picture file.");

                return CreateFailureResult();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(
                    ex,
                    "Access to the selected profile picture file was denied.");

                return CreateFailureResult();
            }
        }

        private ProfilePictureFileResult CreateFailureResult()
        {
            return new ProfilePictureFileResult
            {
                IsSuccessful = false,
                WasCancelled = false
            };
        }
    }
}