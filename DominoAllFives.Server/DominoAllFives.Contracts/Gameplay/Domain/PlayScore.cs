using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class PlayScore
    {
        [DataMember]
        public int PlayerId { get; set; }

        [DataMember]
        public int RoundNumber { get; set; }

        [DataMember]
        public DominoTile Tile { get; set; }

        [DataMember]
        public BoardEnd PlayedEnd { get; set; }

        [DataMember]
        public List<int> EndValues { get; set; }

        [DataMember]
        public List<BoardEnd> BlockedEnds { get; set; }

        [DataMember]
        public bool IncludesOpeningDouble { get; set; }

        [DataMember]
        public int Total { get; set; }

        [DataMember]
        public int BasePoints { get; set; }

        [DataMember]
        public int Multiplier { get; set; }

        [DataMember]
        public int Points { get; set; }

        [DataMember]
        public int RoundPoints { get; set; }
    }
}
