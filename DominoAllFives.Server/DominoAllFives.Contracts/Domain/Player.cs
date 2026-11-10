using System.Runtime.Serialization;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Domain
{
    [DataContract]
    public sealed class Player
    {
        [DataMember] public int PlayerId { get; set; }
        [DataMember] public string Username { get; set; }
        [DataMember] public ProfilePicture Picture { get; set; }
        [DataMember] public PlayerPresence Presence { get; set; }
        [DataMember] public bool IsGuest { get; set; }
    }
}
