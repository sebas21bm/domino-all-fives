using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class PublicEnd
    {
        [DataMember]
        public BoardEnd End { get; set; }

        [DataMember]
        public int Value { get; set; }

        [DataMember]
        public int ScoreValue { get; set; }

        [DataMember]
        public bool IsBlocked { get; set; }

        [DataMember]
        public bool IsOpeningEnd { get; set; }
    }
}
