using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Domain
{
    [DataContract]
    public sealed class LobbyParticipant
    {
        [DataMember(Order = 1)]
        public string ConnectionId { get; set; }

        [DataMember(Order = 2)]
        public string DisplayName { get; set; }

        [DataMember(Order = 3)]
        public bool IsHost { get; set; }
    }
}
