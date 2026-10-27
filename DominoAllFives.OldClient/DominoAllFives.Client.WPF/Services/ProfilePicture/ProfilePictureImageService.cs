using System;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.Services.ProfilePicture
{
    /// <summary>
    /// Converts profile picture data into images that can be displayed
    /// by the WPF client.
    /// </summary>
    public class ProfilePictureImageService
    {
        private const string DefaultProfilePictureUri =
            "pack://application:,,,/Assets/Images/ProfilePictures/defaultProfilePic.png";

        private readonly ILogger<ProfilePictureImageService> _logger;

        public ProfilePictureImageService(
            ILogger<ProfilePictureImageService> logger)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a profile picture from the specified image data.
        /// </summary>
        /// <param name="profilePictureData">
        /// The profile picture data.
        /// </param>
        /// <returns>
        /// The profile picture, or the default profile picture when
        /// the provided data is not available or cannot be loaded.
        /// </returns>
        public BitmapImage GetProfilePicture(
            byte[] profilePictureData)
        {
            if (profilePictureData == null ||
                profilePictureData.Length == 0)
            {
                return GetDefaultProfilePicture();
            }

            try
            {
                using (MemoryStream stream =
                    new MemoryStream(profilePictureData))
                {
                    BitmapImage profilePicture =
                        new BitmapImage();

                    profilePicture.BeginInit();
                    profilePicture.CacheOption =
                        BitmapCacheOption.OnLoad;
                    profilePicture.StreamSource = stream;
                    profilePicture.EndInit();
                    profilePicture.Freeze();

                    return profilePicture;
                }
            }
            catch (Exception ex) when (
                ex is ArgumentException ||
                ex is InvalidOperationException ||
                ex is NotSupportedException)
            {
                _logger.LogWarning(
                    ex,
                    "Unable to create a profile picture from the provided image data.");

                return GetDefaultProfilePicture();
            }
        }

        private BitmapImage GetDefaultProfilePicture()
        {
            try
            {
                BitmapImage defaultProfilePicture =
                    new BitmapImage(
                        new Uri(
                            DefaultProfilePictureUri,
                            UriKind.Absolute));

                defaultProfilePicture.Freeze();

                return defaultProfilePicture;
            }
            catch (Exception ex) when (
                ex is ArgumentException ||
                ex is InvalidOperationException ||
                ex is NotSupportedException ||
                ex is IOException)
            {
                _logger.LogError(
                    ex,
                    "Unable to load the default profile picture.");

                return new BitmapImage();
            }
        }
    }
}