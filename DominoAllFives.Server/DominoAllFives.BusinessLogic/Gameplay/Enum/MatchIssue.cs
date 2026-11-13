using System;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public enum MatchIssue
    {
        MatchNotFound,
        MatchNotInProgress,
        StaleVersion,
        NotPlayersTurn,
        TileNotOwned,
        InvalidPlacement,
        PassNotAllowed,
        DrawNotAllowed,
        PlayerNotInMatch,
        OpeningTileRequired
    }
}
