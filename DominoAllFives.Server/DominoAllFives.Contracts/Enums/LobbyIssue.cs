using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Enums
{
    [DataContract]
    public enum LobbyIssue
    {
        [EnumMember(Value = "LobbyNotFound")] NotFound,
        [EnumMember(Value = "LobbyUnavailable")] Unavailable,
        [EnumMember(Value = "LobbyFull")] Full,
        [EnumMember]
        InvalidPlayerLimit,
        [EnumMember]
        InvalidTargetPoints,
        [EnumMember]
        PlayerAlreadyInGame,
        [EnumMember]
        PlayerAlreadyInLobby,
        [EnumMember]
        PlayerNotInLobby,
        [EnumMember]
        PlayerExpelled,
        [EnumMember]
        InsufficientPlayers,
        [EnumMember]
        SelfExpulsionNotAllowed
    }
}
