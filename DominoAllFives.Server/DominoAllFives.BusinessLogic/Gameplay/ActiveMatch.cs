using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class ActiveMatch
    {
        public object SyncRoot { get; } = new object ();
        public Match Match { get; private set; }
        public DateTime? FinishedAt { get; set; }

        public ActiveMatch(Match match)
        {
            Match = match;
        }
    }
}
