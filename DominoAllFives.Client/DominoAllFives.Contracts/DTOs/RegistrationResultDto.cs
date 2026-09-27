namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents the result of a player registration attempt.
    /// </summary>
    public class RegistrationResultDto
    {
        public bool IsSuccessful { get; set; }

        public int PlayerId { get; set; }

        public string Username { get; set; }

        public RegistrationFailureReason FailureReason { get; set; }
    }

    /// <summary>
    /// Defines the possible reasons for a failed registration attempt.
    /// </summary>
    public enum RegistrationFailureReason
    {
        None,
        UsernameAlreadyExists,
        EmailAlreadyExists
    }
}