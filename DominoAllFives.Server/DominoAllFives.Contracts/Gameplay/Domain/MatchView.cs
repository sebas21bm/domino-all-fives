using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class MatchView
    {
        [DataMember(Order = 1)]
        public int MatchId { get; set; }

        [DataMember(Order = 2)]
        public PublicBoard Board { get; set; }

        [DataMember(Order = 3)]
        public int? CurrentPlayerId { get; set; }

        [DataMember(Order = 4)]
        public List<PlayerScore> Scores { get; set; }

        [DataMember(Order = 5)]
        public MatchPhase Phase { get; set; }

        [DataMember(Order = 6)]
        public long Version { get; set; }

        [DataMember(Order = 7)]
        public int StockCount { get; set; }

        [DataMember(Order = 8)]
        public int RoundNumber { get; set; }

        [DataMember(Order = 9)]
        public RoundResult LastRound { get; set; }

        [DataMember(Order = 10)]
        public int OpeningTileId { get; set; }
    }
}
