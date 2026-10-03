using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents the result of a profile picture retrieval operation.
    /// </summary>
    public class ProfilePictureResultDto
    {
        public bool IsSuccessful { get; set; }

        public byte[] ProfilePicture { get; set; }

        public ProfilePictureFailureReason FailureReason { get; set; }
    }
}
