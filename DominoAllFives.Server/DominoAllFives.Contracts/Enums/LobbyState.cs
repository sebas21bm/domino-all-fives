using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Enums
{
    [DataContract]
    public enum LobbyState
    {
        [EnumMember]
        Waiting,
        [EnumMember]
        Starting,
        [EnumMember]
        InProgress,
        [EnumMember]
        Closed
    }
}
