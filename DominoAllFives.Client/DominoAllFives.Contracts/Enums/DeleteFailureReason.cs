namespace DominoAllFives.Contracts.Enums
{
    /// <summary>
    /// Defines the possible reasons for a failed account deletion attempt.
    /// </summary>
    public enum DeleteFailureReason
    {
        None,
        PasswordNotMatch,
        AccountNotFound,
        ServiceUnavailable
    }
}