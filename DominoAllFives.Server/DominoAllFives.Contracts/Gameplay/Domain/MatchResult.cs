using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class MatchResult
    {
        [DataMember(Order = 1)]
        public int MatchId { get; set; }

        [DataMember(Order = 2)]
        public List<PlayerScore> Scores { get; set; }

        [DataMember(Order = 3)]
        public List<int> WinnerIds { get; set; }

        [DataMember(Order = 4)]
        public MatchStatus Phase { get; set; }

        [DataMember(Order = 5)]
        public long Version { get; set; }
    }
}
