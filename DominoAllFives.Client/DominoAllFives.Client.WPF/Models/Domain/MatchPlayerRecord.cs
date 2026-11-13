using MatchService;

namespace DominoAllFives.Client.WPF.Models
{
    public sealed class MatchPlayerRecord
    {
        public int PlayerId { get; }
        public string Username { get; }
        public int Points { get; }
        public int TileCount { get; }
        public bool IsCurrentPlayer { get; }

        public MatchPlayerRecord(PlayerScore score, string username, int? currentPlayerId)
        {
            PlayerId = score.PlayerId;
            Username = username;
            Points = score.Points;
            TileCount = score.TileCount;
            IsCurrentPlayer = score.PlayerId == currentPlayerId;
        }
    }
}
