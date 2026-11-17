using System;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class ActiveMatch
    {
        public Match Match { get; }
        public DateTime? FinishedAt { get; internal set; }

        public ActiveMatch(Match match)
        {
            Match = match ?? throw new ArgumentNullException(nameof(match));
        }
    }
}
