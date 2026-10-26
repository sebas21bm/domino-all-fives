namespace DominoAllFives.Contracts.Enums
{
    /// <summary>
    /// Defines the possible reasons for a failed registration attempt.
    /// </summary>
    public enum RegistrationStatus
    {
        None,
        InvalidRegistrationData,
        InvalidUsername,
        InvalidEmail,
        InvalidPassword,
        UsernameAlreadyExists,
        EmailAlreadyExists,
        ServiceUnavailable
    }
}
