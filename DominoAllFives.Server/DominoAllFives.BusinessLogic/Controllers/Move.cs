using DominoAllFives.BusinessLogic.Gameplay;

namespace DominoAllFives.BusinessLogic.Controllers
{
    public sealed class Move
    {
        public int TileId { get; }
        public BoardEndPosition End { get; }
        public long ExpectedVersion { get; }

        public Move(int tileId, BoardEndPosition end, long expectedVersion)
        {
            TileId = tileId;
            End = end;
            ExpectedVersion = expectedVersion;
        }
    }
}
