using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class MatchPlayer
    {
        internal List<Tile> Hand { get; } = new List<Tile>();
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
