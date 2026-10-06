using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents the result of updating player profile information.
    /// </summary>
    public class UpdateProfileResultDto
    {
        public bool IsSuccessful { get; set; }

        public UpdateProfileFailureReason FailureReason { get; set; }
    }
}