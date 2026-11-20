using System.Runtime.Serialization;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Represents a chat message exchanged within a lobby or match context.
    /// </summary>
    [DataContract]
    public sealed class ChatMessage
    {
        [DataMember]
        public int ContextId { get; set; }

        [DataMember]
        public ChatScope Scope { get; set; }

        [DataMember]
        public int SenderId { get; set; }

        [DataMember]
        public string SenderUsername { get; set; }

        [DataMember]
        public string Content { get; set; }
    }
}
