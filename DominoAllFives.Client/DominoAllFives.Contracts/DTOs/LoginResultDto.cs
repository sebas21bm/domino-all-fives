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

    /// <summary>
    /// Defines the possible reasons for a failed login attempt.
    /// </summary>
    public enum LoginFailureReason
    {
        None,
        InvalidCredentials,
        Banned,
        Suspended
    }
}