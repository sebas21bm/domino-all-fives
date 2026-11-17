using System;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class ActiveMatch
    {
        public Match Match { get; }
        internal MatchUpdateQueue PendingUpdates { get; }
        public DateTime? FinishedAt { get; internal set; }

        public ActiveMatch(Match match)
        {
            Match = match ?? throw new ArgumentNullException(nameof(match));
            PendingUpdates = new MatchUpdateQueue(match.MatchId);
        }
    }
}
