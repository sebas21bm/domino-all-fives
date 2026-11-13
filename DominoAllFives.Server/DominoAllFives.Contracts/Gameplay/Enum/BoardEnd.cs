using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoAllFives.Contracts.Gameplay
{
    [DataContract]
    public enum BoardEnd
    {
        [EnumMember]
        Left = 0,
        [EnumMember]
        Right = 1,
        [EnumMember]
        Top = 2,
        [EnumMember]
        Bottom = 3
    }
}
