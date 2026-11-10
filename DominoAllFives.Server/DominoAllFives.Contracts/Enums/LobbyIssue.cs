using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Enums
{
    [DataContract]
    public enum LobbyIssue
    {
        [EnumMember] LobbyNotFound,
        [EnumMember] LobbyUnavailable,
        [EnumMember] LobbyFull,
        [EnumMember] InvalidPlayerLimit,
        [EnumMember] InvalidTargetPoints,
        [EnumMember] PlayerAlreadyInGame,
        [EnumMember] PlayerAlreadyInLobby,
        [EnumMember] PlayerNotInLobby,
        [EnumMember] PlayerExpelled,
        [EnumMember] InsufficientPlayers,
        [EnumMember] SelfExpulsionNotAllowed
    }
}
