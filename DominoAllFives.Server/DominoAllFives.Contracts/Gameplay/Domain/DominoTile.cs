using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class DominoTile
    {
        [DataMember(Order = 1)]
        public int TileId { get; set; }

        [DataMember(Order = 2)]
        public int FirstValue { get; set; }

        [DataMember(Order = 3)]
        public int SecondValue { get; set; }

        [DataMember(Order = 4)]
        public SpecialPower SpecialPower { get; set; }
    }
}
