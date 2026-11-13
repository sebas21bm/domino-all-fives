using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Enums
{
    [DataContract]
    public enum AccessIssue
    {
        [EnumMember]
        SessionRequired,
        [EnumMember]
        RegisteredAccountRequired,
        [EnumMember]
        PermissionDenied
    }
}
