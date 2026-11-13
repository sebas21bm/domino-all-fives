using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Enums
{
    /// <summary>
    /// Specifies the business rule violations that can reject a chat operation.
    /// </summary>
    [DataContract]
    public enum ChatIssue
    {
        [EnumMember]
        EmptyMessage,
        [EnumMember]
        MessageTooLong,
        [EnumMember]
        PlayerNotInChat,
        [EnumMember]
        ContentNotAllowed
    }
}
