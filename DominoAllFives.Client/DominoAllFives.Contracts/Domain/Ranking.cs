using System;
using System.Collections.Generic;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Preserves an ordered ranking snapshot and the independently calculated player position.
    /// </summary>
    public sealed class Ranking
    {
        public IReadOnlyList<RankingEntry> TopPlayers { get; private set; }
        public int CurrentPlayerRank { get; private set; }

        private Ranking()
        {
        }

        /// <summary>
        /// Copies an ordered selection without imposing new eligibility or tie policies.
        /// </summary>
        /// <param name="topPlayers">The entries already ordered by the server.</param>
        /// <param name="currentPlayerRank">The calculated position, including zero when absent.</param>
        /// <returns>An independent read-only ranking snapshot.</returns>
        public static Ranking FromOrderedEntries(
            IEnumerable<RankingEntry> topPlayers, int currentPlayerRank)
        {
            if (topPlayers == null)
            {
                throw new ArgumentNullException(nameof(topPlayers));
            }
            return new Ranking
            {
                TopPlayers = new List<RankingEntry>(topPlayers).AsReadOnly(),
                CurrentPlayerRank = currentPlayerRank
            };
        }
    }
}
