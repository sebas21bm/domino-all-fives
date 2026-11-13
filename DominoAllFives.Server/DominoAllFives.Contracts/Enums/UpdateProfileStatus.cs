namespace DominoAllFives.Contracts.Enums
{
    /// <summary>
    /// Represents the possible failure reasons when updating
    /// player profile information.
    /// </summary>
    public enum UpdateProfileStatus
    {
        None,
        InvalidUsername,
        SameUsername,
        UsernameAlreadyExists,
        InvalidProfilePictureFormat,
        ProfilePictureTooLarge,
        InvalidProfilePicture,
        PlayerNotFound,
        ServiceUnavailable
    }
}
