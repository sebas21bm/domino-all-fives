using System.Collections.Generic;
using MatchService;

namespace DominoAllFives.Client.WPF.Models
{
    public sealed class RoundPlayerRecord
    {
        public string Username { get; }
        public int Points { get; }
        public IReadOnlyList<DominoTile> Tiles { get; }

        public RoundPlayerRecord(string username, int points, IReadOnlyList<DominoTile> tiles)
        {
            Username = username;
            Points = points;
            Tiles = tiles;
        }
    }
}
