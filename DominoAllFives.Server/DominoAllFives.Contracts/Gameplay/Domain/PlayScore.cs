using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class PlayScore
    {
        [DataMember(Order = 1)]
        public int PlayerId { get; set; }

        [DataMember(Order = 2)]
        public int RoundNumber { get; set; }

        [DataMember(Order = 3)]
        public DominoTile Tile { get; set; }

        [DataMember(Order = 4)]
        public BoardEnd PlayedEnd { get; set; }

        [DataMember(Order = 5)]
        public List<int> EndValues { get; set; }

        [DataMember(Order = 6)]
        public List<BoardEnd> BlockedEnds { get; set; }

        [DataMember(Order = 7)]
        public bool IncludesOpeningDouble { get; set; }

        [DataMember(Order = 8)]
        public int Total { get; set; }

        [DataMember(Order = 9)]
        public int BasePoints { get; set; }

        [DataMember(Order = 10)]
        public int Multiplier { get; set; }

        [DataMember(Order = 11)]
        public int Points { get; set; }

        [DataMember(Order = 12)]
        public int RoundPoints { get; set; }
    }
}
