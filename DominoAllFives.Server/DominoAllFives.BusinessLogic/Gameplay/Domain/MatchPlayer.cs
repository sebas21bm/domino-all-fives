using System.Collections.Generic;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    internal sealed class MatchPlayer
    {
        internal List<Tile> Hand { get; set; } = new List<Tile>();
        public int PlayerId { get; internal set; }
        public int Points { get; internal set; }

        public IReadOnlyList<Tile> Tiles
        {
            get
            {
                return Hand.AsReadOnly();
            }
        }
    }
}
