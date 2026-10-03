namespace DominoAllFives.Contracts.Enums
{
    /// <summary>
    /// Defines the possible reasons for a failed registration attempt.
    /// </summary>
    public enum RegistrationFailureReason
    {
        None,
        InvalidRegistrationData,
        InvalidUsername,
        InvalidEmail,
        InvalidPassword,
        UsernameAlreadyExists,
        EmailAlreadyExists
    }
}
