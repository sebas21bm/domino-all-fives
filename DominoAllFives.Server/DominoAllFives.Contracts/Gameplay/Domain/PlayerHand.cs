using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class PlayerHand
    {
        [DataMember(Order = 1)]
        public int MatchId { get; set; }

        [DataMember(Order = 2)]
        public int PlayerId { get; set; }

        [DataMember(Order = 3)]
        public List<DominoTile> Tiles { get; set; }

        [DataMember(Order = 4)]
        public long Version { get; set; }
    }
}
