using System;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Gameplay;
using DominoAllFives.BusinessLogic.Managers;
using GameplayContracts = DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.BusinessLogic.Controllers
{
    public sealed class MatchController
    {
        private readonly MatchManager _matchManager;

        public MatchController(MatchManager matchManager)
        {
            _matchManager = matchManager
                ?? throw new ArgumentNullException(nameof(matchManager));
        }

        public void CreateMatch(int matchId, IEnumerable<int> players, int targetPoints)
        {
            _matchManager.CreateMatch(matchId, players, targetPoints);
        }

        public GameplayContracts.MatchView GetMatch(int matchId, int playerId)
        {
            return _matchManager.GetMatchView(matchId, playerId);
        }

        public GameplayContracts.PlayerHand GetPlayerHand(int matchId, int playerId)
        {
            return _matchManager.GetPlayerHand(matchId, playerId);
        }

        public MatchUpdate PlayTile(int matchId, int playerId, Move move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            return _matchManager.PlayTile(matchId, playerId, move);
        }

        public MatchUpdate DrawTiles(int matchId, int playerId, long expectedVersion)
        {
            return _matchManager.DrawTiles(matchId, playerId, expectedVersion);
        }

        public MatchUpdate PassTurn(int matchId, int playerId, long expectedVersion)
        {
            return _matchManager.PassTurn(matchId, playerId, expectedVersion);
        }

        public MatchUpdate LeaveMatch(int matchId, int playerId)
        {
            return _matchManager.LeaveMatch(matchId, playerId);
        }
    }
}
