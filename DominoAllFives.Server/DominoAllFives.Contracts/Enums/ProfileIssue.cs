using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Enums
{
    /// <summary>Identifies expected profile and profile picture rejections.</summary>
    [DataContract]
    public enum ProfileIssue
    {
        [EnumMember]
        PlayerNotFound,
        [EnumMember]
        InvalidUsername,
        [EnumMember]
        SameUsername,
        [EnumMember]
        UsernameAlreadyExists,
        [EnumMember]
        InvalidProfilePictureFormat,
        [EnumMember]
        ProfilePictureTooLarge,
        [EnumMember]
        InvalidProfilePicture
    }
}
