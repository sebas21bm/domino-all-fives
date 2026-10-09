
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents the result of a password change attempt.
    /// </summary>
    public class ChangePasswordResultDto
    {
        public bool IsSuccessful { get; set; }

        public ChangePasswordFailureReason FailureReason { get; set; }
    }
}
