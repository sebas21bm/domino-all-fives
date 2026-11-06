using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Domain
{
    [DataContract]
    public sealed class Lobby
    {
        [DataMember(Order = 1)]
        public string LobbyCode { get; set; }

        [DataMember(Order = 2)]
        public int MaximumPlayers { get; set; }

        [DataMember(Order = 3)]
        public bool HasStarted { get; set; }

        [DataMember(Order = 4)]
        public List<LobbyParticipant> Participants { get; set; }
    }
}
