using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class MatchView
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public PublicBoard Board { get; set; }

        [DataMember]
        public int? CurrentPlayerId { get; set; }

        [DataMember]
        public List<PlayerScore> Scores { get; set; }

        [DataMember]
        public MatchStatus Phase { get; set; }

        [DataMember]
        public long Version { get; set; }

        [DataMember]
        public int StockCount { get; set; }

        [DataMember]
        public int RoundNumber { get; set; }

        [DataMember]
        public RoundResult LastRound { get; set; }

        [DataMember]
        public int OpeningTileId { get; set; }

        [DataMember]
        public PlayScore LastPlay { get; set; }
    }
}
