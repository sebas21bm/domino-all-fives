namespace DominoAllFives.Contracts.Enums
{
    /// <summary>
    /// Defines the possible reasons for a failed profile picture retrieval operation.
    /// </summary>
    public enum ProfilePictureStatus
    {
        None,
        InvalidFormat,
        FileTooLarge,
        InvalidImage,
        PlayerNotFound,
        ServiceUnavailable
    }
}
