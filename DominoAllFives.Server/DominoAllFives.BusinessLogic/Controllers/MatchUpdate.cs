using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Gameplay;
using GameplayContracts = DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.BusinessLogic.Controllers
{
    public sealed class MatchUpdate
    {
        public GameplayContracts.MatchView View { get; internal set; }
        public IReadOnlyList<GameplayContracts.PlayerHand> Hands { get; internal set; }
        public GameplayContracts.RoundResult Round { get; internal set; }
        public GameplayContracts.MatchResult Result { get; internal set; }
        public bool HasStateChange { get; internal set; }
        public MatchUpdateQueue PendingUpdates { get; internal set; }
    }
}
