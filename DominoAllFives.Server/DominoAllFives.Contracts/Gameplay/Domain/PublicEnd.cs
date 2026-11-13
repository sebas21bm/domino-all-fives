using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class PublicEnd
    {
        [DataMember(Order = 1)]
        public BoardEnd End { get; set; }

        [DataMember(Order = 2)]
        public int Value { get; set; }

        [DataMember(Order = 3)]
        public int ScoreValue { get; set; }

        [DataMember(Order = 4)]
        public bool IsBlocked { get; set; }

        [DataMember(Order = 5)]
        public bool IsOpeningEnd { get; set; }
    }
}
