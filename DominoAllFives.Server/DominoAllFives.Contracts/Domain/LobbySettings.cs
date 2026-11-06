using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Domain
{
    [DataContract]
    public sealed class LobbySettings
    {
        [DataMember(Order = 1, IsRequired = true)]
        public string HostDisplayName { get; set; }

        [DataMember(Order = 2, IsRequired = true)]
        public int MaximumPlayers { get; set; }
    }
}
