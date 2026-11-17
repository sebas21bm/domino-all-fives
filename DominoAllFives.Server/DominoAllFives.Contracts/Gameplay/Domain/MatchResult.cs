using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class MatchResult
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public List<PlayerScore> Scores { get; set; }

        [DataMember]
        public List<int> WinnerIds { get; set; }

        [DataMember]
        public MatchStatus Phase { get; set; }

        [DataMember]
        public long Version { get; set; }
    }
}
