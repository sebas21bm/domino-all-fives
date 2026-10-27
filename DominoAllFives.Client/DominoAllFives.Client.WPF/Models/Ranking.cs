using System;
using System.Collections.Generic;
using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Models
{
    /// <summary>Combines the remote leaderboard and player position for presentation.</summary>
    public sealed class Ranking
    {
        public List<RankingEntry> TopPlayers { get; }
        public int CurrentPlayerRank { get; }

        public Ranking(List<RankingEntry> topPlayers, int currentPlayerRank)
        {
            TopPlayers = topPlayers ?? throw new ArgumentNullException(nameof(topPlayers));
            CurrentPlayerRank = currentPlayerRank;
        }
    }
}
