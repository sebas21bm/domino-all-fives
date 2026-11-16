using System;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Gameplay;
using DominoAllFives.BusinessLogic.Managers;
using Network = DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.BusinessLogic.Controllers
{
    public sealed class MatchController
    {
        private readonly MatchManager _matchManager;
        private readonly Action<MatchUpdate> _enqueue;
        public MatchController(MatchManager matchManager, Action<MatchUpdate> enqueue)
        {
            _matchManager = matchManager ?? throw new ArgumentNullException(nameof(matchManager));
            _enqueue = enqueue ?? throw new ArgumentNullException(nameof(enqueue));
        }

        public void Create(int matchId, IEnumerable<int> players, int targetPoints)
        {
            _matchManager.CreateMatch(matchId, players, targetPoints);
        }

        public Network.MatchView GetMatch(int matchId, int playerId)
        {
            return _matchManager.GetMatchView(matchId, playerId);
        }

        public Network.PlayerHand GetHand(int matchId, int playerId)
        {
            return _matchManager.GetPlayerHand(matchId, playerId);
        }

        public Network.MatchView Play(int matchId, int playerId, Move move)
        {
            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            return Change(matchId, playerId, match => match.Play(playerId, move.TileId,
                move.End, move.ExpectedVersion));
        }

        public Network.MatchView Draw(int matchId, int playerId, long expectedVersion)
        {
            return Change(matchId, playerId, match => match.Draw(playerId, expectedVersion));
        }

        public Network.MatchView Pass(int matchId, int playerId, long expectedVersion)
        {
            return Change(matchId, playerId, match => match.Pass(playerId, expectedVersion));
        }

        public void Leave(int matchId, int playerId)
        {
            Change(matchId, playerId, match => match.Cancel(playerId));
        }

        private Network.MatchView Change(int matchId, int playerId, Action<Match> change)
        {
            return _matchManager.ChangeMatch(matchId, playerId, change, _enqueue);
        }
    }
}
