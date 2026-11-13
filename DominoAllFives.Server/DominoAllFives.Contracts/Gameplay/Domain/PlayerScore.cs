using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class PlayerScore
    {
        [DataMember(Order = 1)]
        public int PlayerId { get; set; }

        [DataMember(Order = 2)]
        public int Points { get; set; }

        [DataMember(Order = 3)]
        public int TileCount { get; set; }
    }
}
