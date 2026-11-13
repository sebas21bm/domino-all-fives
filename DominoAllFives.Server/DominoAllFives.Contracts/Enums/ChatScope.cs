using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Enums
{
    /// <summary>
    /// Identifies the operational scope where a chat conversation takes place.
    /// </summary>
    [DataContract]
    public enum ChatScope
    {
        [EnumMember]
        Lobby,
        [EnumMember]
        Match
    }
}
