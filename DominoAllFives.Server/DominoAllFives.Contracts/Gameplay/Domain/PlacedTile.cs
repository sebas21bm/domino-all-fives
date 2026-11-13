using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class PlacedTile
    {
        [DataMember(Order = 1)]
        public DominoTile Tile { get; set; }

        [DataMember(Order = 2)]
        public BoardEnd End { get; set; }

        [DataMember(Order = 3)]
        public int ConnectingValue { get; set; }

        [DataMember(Order = 4)]
        public int ExposedValue { get; set; }
    }
}
