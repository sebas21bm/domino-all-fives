using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Enums
{
    [DataContract]
    public enum AccountIssue
    {
        [EnumMember]
        InvalidCredentials,
        [EnumMember]
        Banned,
        [EnumMember]
        Suspended
    }
}
