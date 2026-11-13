using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class BoardEnd
    {
        private const int UnblockedTurn = -1;
        public End End { get; internal set; }
        public int Value { get; internal set; }
        public int ScoreValue { get; internal set; }
        public int BlockedTurn { get; internal set; } = UnblockedTurn;
        public bool IsOpeningEnd { get; internal set; }
    }
}
