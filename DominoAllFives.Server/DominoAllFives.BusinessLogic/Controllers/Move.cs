using DominoAllFives.BusinessLogic.Gameplay;

namespace DominoAllFives.BusinessLogic.Controllers
{
    public sealed class Move
    {
        public int TileId { get; set; }
        public BoardEndPosition End { get; set; }
        public long ExpectedVersion { get; set; }
    }
}
