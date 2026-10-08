using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents the result of an account deletion attempt.
    /// </summary>
    public class DeleteAccountResultDto
    {
        public bool IsSuccessful { get; set; }

        public DeleteFailureReason FailureReason { get; set; }
    }
}