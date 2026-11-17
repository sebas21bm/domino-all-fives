using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class PublicBoard
    {
        [DataMember]
        public List<PlacedTile> Tiles { get; set; }

        [DataMember]
        public List<PublicEnd> Ends { get; set; }
    }
}
