using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public enum MatchIssue
    {
        [EnumMember]
        MatchNotFound = 0,
        [EnumMember]
        MatchNotInProgress = 1,
        [EnumMember]
        StaleVersion = 2,
        [EnumMember]
        NotPlayersTurn = 3,
        [EnumMember]
        TileNotOwned = 4,
        [EnumMember]
        InvalidPlacement = 5,
        [EnumMember]
        PassNotAllowed = 6,
        [EnumMember]
        DrawNotAllowed = 7,
        [EnumMember]
        PlayerNotInMatch = 8,
        [EnumMember]
        OpeningTileRequired = 9
    }
}
