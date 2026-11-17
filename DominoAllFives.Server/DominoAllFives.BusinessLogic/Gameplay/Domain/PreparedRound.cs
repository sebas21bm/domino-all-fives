using System.Collections.Generic;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    internal sealed class PreparedRound
    {
        public IReadOnlyList<MatchPlayer> Players { get; }
        public Board Board { get; }
        public Queue<Tile> Stock { get; }
        public int OpeningTileId { get; }

        public PreparedRound(List<MatchPlayer> players, Queue<Tile> stock, int openingTileId)
        {
            Players = players.AsReadOnly();
            Board = new Board();
            Stock = stock;
            OpeningTileId = openingTileId;
        }
    }
}
