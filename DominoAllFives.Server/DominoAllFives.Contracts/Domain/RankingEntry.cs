using System.Runtime.Serialization;
using System;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Represents a server-assigned ordinal position and the corresponding public player data.
    /// </summary>
    [DataContract]
    public sealed class RankingEntry
    {
        [DataMember(Order = 1, IsRequired = true)]
        public int Rank { get; private set; }
        [DataMember(Order = 2, IsRequired = true)]
        public string Username { get; private set; }
        [DataMember(Order = 3, IsRequired = true)]
        public int GamesWon { get; private set; }
        [DataMember(Order = 4, IsRequired = true)]
        public ProfilePicture Picture { get; private set; }

        private RankingEntry()
        {
        }

        /// <summary>
        /// Captures an entry without recalculating server ordering or tie rules.
        /// </summary>
        /// <param name="rank">The server-assigned ordinal position.</param>
        /// <param name="username">The stored username.</param>
        /// <param name="gamesWon">The stored victory count.</param>
        /// <param name="picture">The optional content snapshot.</param>
        /// <returns>A read-only ranking entry.</returns>
        public static RankingEntry FromRankedPlayer(int rank, string username,
            int gamesWon, ProfilePicture picture)
        {
            return new RankingEntry
            {
                Rank = rank,
                Username = username,
                GamesWon = gamesWon,
                Picture = picture ?? throw new ArgumentNullException(nameof(picture))
            };
        }
    }
}
