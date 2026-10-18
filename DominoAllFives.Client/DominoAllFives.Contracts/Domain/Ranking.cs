using System.Runtime.Serialization;
using System;
using System.Collections.Generic;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Preserves an ordered ranking snapshot and the independently calculated player position.
    /// </summary>
    [DataContract]
    public sealed class Ranking
    {
        [DataMember(Name = "TopPlayers", Order = 1, IsRequired = true)]
        private List<RankingEntry> _topPlayers;
        public IReadOnlyList<RankingEntry> TopPlayers => _topPlayers.AsReadOnly();
        [DataMember(Order = 2, IsRequired = true)]
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
                _topPlayers = new List<RankingEntry>(topPlayers),
                CurrentPlayerRank = currentPlayerRank
            };
        }
    }
}
