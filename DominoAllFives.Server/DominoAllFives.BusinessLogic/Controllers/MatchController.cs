using System;
using System.Collections.Generic;
using System.Linq;
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
            ActiveMatch active = _matchManager.GetMatch(matchId);
            lock (active.SyncRoot)
            {
                active.Match.GetPlayer(playerId);
                return MatchProjection.CreateView(active.Match);
            }
        }

        public Network.PlayerHand GetHand(int matchId, int playerId)
        {
            ActiveMatch active = _matchManager.GetMatch(matchId);
            lock (active.SyncRoot)
            {
                return MatchProjection.CreateHand(active.Match, playerId);
            }
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
            ActiveMatch active = _matchManager.GetMatch(matchId);
            lock (active.SyncRoot)
            {
                Match match = active.Match;
                match.GetPlayer(playerId);
                RoundSummary previousRound = match.LastRound;
                long previousVersion = match.Version;
                change(match);
                if (match.Phase != MatchState.InProgress && !active.FinishedAt.HasValue)
                {
                    active.FinishedAt = DateTime.UtcNow;
                }

                Network.MatchView view = MatchProjection.CreateView(match);
                if (previousVersion == match.Version)
                {
                    return view;
                }

                // Enqueue only; WCF delivery must never execute under the match lock.
                _enqueue(new MatchUpdate { View = view, Hands = match.Players.Select(player
                    => MatchProjection.CreateHand(match, player.PlayerId))
                    .ToList(), Round = ReferenceEquals(previousRound, match.LastRound)
                        ? null : view.LastRound, Result = match.Phase == MatchState.InProgress
                        ? null : MatchProjection.CreateResult(match) });
                return view;
            }
        }
    }
}
