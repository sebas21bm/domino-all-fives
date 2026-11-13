using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public enum SpecialPower
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        DoublePoints = 1,
        [EnumMember]
        Block = 2
    }
}
