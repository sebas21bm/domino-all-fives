namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Represents optional image content independently of its storage path.
    /// </summary>
    public sealed class ProfilePicture
    {
        private readonly byte[] _imageData;
        public string OriginalFileName { get; private set; }
        public byte[] ImageData => _imageData == null ? null : (byte[])_imageData.Clone();

        private ProfilePicture(string originalFileName, byte[] imageData)
        {
            OriginalFileName = originalFileName;
            _imageData = imageData == null ? null : (byte[])imageData.Clone();
        }

        /// <summary>
        /// Captures stored content, including an absent picture, without upload validation.
        /// </summary>
        /// <param name="imageData">Stored content or null for an absent picture.</param>
        /// <returns>An independent picture snapshot.</returns>
        public static ProfilePicture FromStoredContent(byte[] imageData)
        {
            return new ProfilePicture(null, imageData);
        }

        /// <summary>
        /// Captures selection metadata while leaving ordered upload validation to the operation.
        /// </summary>
        /// <param name="originalFileName">The selected name, never a trusted storage path.</param>
        /// <param name="imageData">The selected content to copy.</param>
        /// <returns>An independent selection snapshot.</returns>
        public static ProfilePicture FromSelectedContent(string originalFileName, byte[] imageData)
        {
            return new ProfilePicture(originalFileName, imageData);
        }
    }
}
