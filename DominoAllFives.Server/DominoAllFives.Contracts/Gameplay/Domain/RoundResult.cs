using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class RoundResult
    {
        [DataMember(Order = 1)]
        public int RoundNumber { get; set; }

        [DataMember(Order = 2)]
        public int? WinnerId { get; set; }

        [DataMember(Order = 3)]
        public int AwardedPoints { get; set; }

        [DataMember(Order = 4)]
        public List<PlayerScore> Scores { get; set; }

        [DataMember(Order = 5)]
        public List<PlayerHand> RevealedHands { get; set; }
    }
}
