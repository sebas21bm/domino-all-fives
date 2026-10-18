using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Enums
{
    /// <summary>
    /// Defines the possible reasons for a failed login attempt.
    /// </summary>
    [DataContract]
    public enum LoginFailureReason
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        InvalidCredentials = 1,
        [EnumMember]
        Banned = 2,
        [EnumMember]
        Suspended = 3,
        [EnumMember]
        ServiceUnavailable = 4
    }
}
