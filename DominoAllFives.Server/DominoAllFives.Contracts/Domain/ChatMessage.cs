using System;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Represents a chat message exchanged within a lobby or match context.
    /// </summary>
    [DataContract]
    public sealed class ChatMessage
    {
        [DataMember]
        public int MessageId { get; set; }

        [DataMember]
        public ChatContext Context { get; set; }

        [DataMember]
        public int SenderId { get; set; }

        [DataMember]
        public string SenderUsername { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public DateTime SentAtUtc { get; set; }
    }
}
