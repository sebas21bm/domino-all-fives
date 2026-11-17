using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class DominoTile
    {
        [DataMember]
        public int TileId { get; set; }

        [DataMember]
        public int FirstValue { get; set; }

        [DataMember]
        public int SecondValue { get; set; }

        [DataMember]
        public SpecialPower SpecialPower { get; set; }
    }
}
