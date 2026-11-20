using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Represents a read-only snapshot of the statistics maintained by the server.
    /// </summary>
    [DataContract]
    public sealed class PlayerStatistics
    {
        [DataMember(Order = 1, IsRequired = true)]
        public int GamesWon { get; private set; }
        [DataMember(Order = 2, IsRequired = true)]
        public int TotalPointsScored { get; private set; }
        [DataMember(Order = 3, IsRequired = true)]
        public int GamesPlayed { get; private set; }

        private PlayerStatistics()
        {
        }

        /// <summary>
        /// Preserves stored counters without introducing additional game rules.
        /// </summary>
        /// <param name = "gamesWon">The stored victory count.</param>
        /// <param name = "totalPointsScored">The stored points total.</param>
        /// <param name = "gamesPlayed">The stored match count.</param>
        /// <returns>An immutable statistics snapshot.</returns>
        public static PlayerStatistics FromStoredStatistics(
            int gamesWon,
            int totalPointsScored,
            int gamesPlayed)
        {
            return new PlayerStatistics
            {
                GamesWon = gamesWon,
                TotalPointsScored = totalPointsScored,
                GamesPlayed = gamesPlayed
            };
        }
    }
}
