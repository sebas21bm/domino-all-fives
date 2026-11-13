using System.Collections.Generic;
using Network = DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.BusinessLogic.Controllers
{
    public sealed class MatchUpdate
    {
        public Network.MatchView View { get; internal set; }
        public List<Network.PlayerHand> Hands { get; internal set; }
        public Network.RoundResult Round { get; internal set; }
        public Network.MatchResult Result { get; internal set; }
    }
}
