using System;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    internal sealed class ActiveMatch
    {
        public Match Match { get; }
        internal MatchUpdateQueue PendingUpdates { get; }
        public DateTime? FinishedAt { get; private set; }

        public ActiveMatch(Match match)
        {
            Match = match ?? throw new ArgumentNullException(nameof(match));
            PendingUpdates = new MatchUpdateQueue(match.MatchId);
        }

        public void RecordMatchFinish()
        {
            if (Match.Phase != MatchState.InProgress && !FinishedAt.HasValue)
            {
                FinishedAt = DateTime.UtcNow;
            }
        }
    }
}
