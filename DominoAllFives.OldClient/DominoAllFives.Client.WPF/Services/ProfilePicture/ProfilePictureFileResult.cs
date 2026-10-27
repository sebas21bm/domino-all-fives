namespace DominoAllFives.Client.WPF.Services
{
    /// <summary>
    /// Represents the result of selecting and reading
    /// a profile picture file.
    /// </summary>
    public class ProfilePictureFileResult
    {
        public bool IsSuccessful { get; set; }

        public bool WasCancelled { get; set; }

        public byte[] ImageData { get; set; }

        public string FileName { get; set; }
    }
}