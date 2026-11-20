using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Enums
{
    /// <summary>
    /// Identifies the account rule that rejected a service operation.
    /// </summary>
    [DataContract]
    public enum AccountIssue
    {
        [EnumMember]
        InvalidCredentials,

        [EnumMember]
        Banned,

        [EnumMember]
        Suspended,

        [EnumMember]
        PlayerNotFound,

        [EnumMember]
        IncorrectCurrentPassword,

        [EnumMember]
        InvalidPassword,

        [EnumMember]
        PasswordUnchanged,

        [EnumMember]
        UnsupportedLanguage
    }
}
