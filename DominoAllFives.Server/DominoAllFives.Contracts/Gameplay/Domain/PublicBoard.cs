using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class PublicBoard
    {
        [DataMember(Order = 1)]
        public List<PlacedTile> Tiles { get; set; }

        [DataMember(Order = 2)]
        public List<PublicEnd> Ends { get; set; }
    }
}
