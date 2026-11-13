using System.Collections.Generic;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class PlayScore
    {
        public int PlayerId { get; internal set; }
        public int RoundNumber { get; internal set; }
        public Tile Tile { get; internal set; }
        public End PlayedEnd { get; internal set; }
        public IReadOnlyList<int> EndValues { get; internal set; }
        public IReadOnlyList<End> BlockedEnds { get; internal set; }
        public bool IncludesOpeningDouble { get; internal set; }
        public int Total { get; internal set; }
        public int BasePoints { get; internal set; }
        public int Multiplier { get; internal set; }
        public int Points { get; internal set; }
        public int RoundPoints { get; internal set; }
    }
}
