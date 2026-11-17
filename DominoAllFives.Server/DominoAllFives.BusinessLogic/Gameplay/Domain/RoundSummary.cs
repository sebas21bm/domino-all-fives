using System.Collections.Generic;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class RoundSummary
    {
        public int RoundNumber { get; internal set; }
        public int? WinnerId { get; internal set; }
        public int AwardedPoints { get; internal set; }
        public Dictionary<int, int> Scores { get; internal set; }
        public Dictionary<int, Tile[]> RemainingHands { get; internal set; }
    }
}
