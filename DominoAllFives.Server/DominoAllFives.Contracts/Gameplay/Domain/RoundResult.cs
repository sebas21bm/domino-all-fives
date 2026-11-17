using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class RoundResult
    {
        [DataMember]
        public int RoundNumber { get; set; }

        [DataMember]
        public int? WinnerId { get; set; }

        [DataMember]
        public int AwardedPoints { get; set; }

        [DataMember]
        public List<PlayerScore> Scores { get; set; }

        [DataMember]
        public List<PlayerHand> RevealedHands { get; set; }
    }
}
