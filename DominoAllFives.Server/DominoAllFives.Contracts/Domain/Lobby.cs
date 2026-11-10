using System.Collections.Generic;
using System.Runtime.Serialization;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Domain
{
    [DataContract]
    public sealed class Lobby
    {
        [DataMember] public int LobbyId { get; set; }
        [DataMember] public string LobbyCode { get; set; }
        [DataMember] public Player Host { get; set; }
        [DataMember] public List<Player> Participants { get; set; }
        [DataMember] public List<Player> KickedPlayers { get; set; }
        [DataMember] public LobbySettings Settings { get; set; }
        [DataMember] public LobbyState State { get; set; }
    }
}
