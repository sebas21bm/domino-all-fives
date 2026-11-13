using System.Runtime.Serialization;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Domain
{
    /// <summary>
    /// Identifies a specific lobby or match conversation channel.
    /// </summary>
    [DataContract]
    public sealed class ChatContext
    {
        [DataMember]
        public ChatScope Scope { get; set; }

        [DataMember]
        public int ContextId { get; set; }
    }
}
