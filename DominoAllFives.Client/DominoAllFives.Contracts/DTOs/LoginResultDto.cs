using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents the result of a player login attempt.
    /// </summary>
    public class LoginResultDto
    {
        public bool IsSuccessful { get; set; }

        public int PlayerId { get; set; }

        public string Username { get; set; }

        public LoginFailureReason FailureReason { get; set; }
    }
}