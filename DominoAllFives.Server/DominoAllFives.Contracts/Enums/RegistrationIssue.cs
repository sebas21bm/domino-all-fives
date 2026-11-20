using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Enums
{
    /// <summary>Identifies expected registration rejections at the remote boundary.</summary>
    [DataContract]
    public enum RegistrationIssue
    {
        [EnumMember]
        InvalidRegistrationData,
        [EnumMember]
        InvalidUsername,
        [EnumMember]
        InvalidEmail,
        [EnumMember]
        InvalidPassword,
        [EnumMember]
        UsernameAlreadyExists,
        [EnumMember]
        EmailAlreadyExists
    }
}
