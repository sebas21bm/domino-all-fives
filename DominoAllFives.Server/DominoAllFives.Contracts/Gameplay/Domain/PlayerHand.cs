using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class PlayerHand
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public int PlayerId { get; set; }

        [DataMember]
        public List<DominoTile> Tiles { get; set; }

        [DataMember]
        public long Version { get; set; }
    }
}
