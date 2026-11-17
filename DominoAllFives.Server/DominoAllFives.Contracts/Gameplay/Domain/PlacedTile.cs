using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class PlacedTile
    {
        [DataMember]
        public DominoTile Tile { get; set; }

        [DataMember]
        public BoardEnd End { get; set; }

        [DataMember]
        public int ConnectingValue { get; set; }

        [DataMember]
        public int ExposedValue { get; set; }
    }
}
