using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Domain
{
    [DataContract]
    public sealed class LobbySettings
    {
        [DataMember] public int MaxPlayers { get; set; }
        [DataMember] public int TargetPoints { get; set; }
    }
}
