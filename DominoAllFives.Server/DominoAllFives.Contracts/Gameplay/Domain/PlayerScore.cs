using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class PlayerScore
    {
        [DataMember]
        public int PlayerId { get; set; }

        [DataMember]
        public int Points { get; set; }

        [DataMember]
        public int TileCount { get; set; }
    }
}
