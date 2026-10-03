namespace DominoAllFives.Contracts.Enums
{
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
