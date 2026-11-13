using System;
using System.Collections.Generic;
using System.Linq;
using DominoAllFives.BusinessLogic.Gameplay;
using Network = DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.BusinessLogic.Controllers
{
    public sealed class Move
    {
        public int TileId { get; set; }
        public End End { get; set; }
        public long ExpectedVersion { get; set; }
    }
}
