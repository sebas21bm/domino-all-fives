using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public enum MatchPhase
    {
        [EnumMember]
        InProgress = 0,
        [EnumMember]
        Completed = 1,
        [EnumMember]
        Cancelled = 2
    }
}
