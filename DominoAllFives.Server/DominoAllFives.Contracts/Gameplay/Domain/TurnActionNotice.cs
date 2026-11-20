using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public sealed class TurnActionNotice
    {
        [DataMember]
        public int PlayerId { get; set; }

        [DataMember]
        public bool IsPass { get; set; }

        [DataMember]
        public long Version { get; set; }
    }
}
