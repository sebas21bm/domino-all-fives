using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Enums
{
    [DataContract]
    public enum PlayerPresence
    {
        [EnumMember] Online,
        [EnumMember] Offline,
        [EnumMember] InGame
    }
}
