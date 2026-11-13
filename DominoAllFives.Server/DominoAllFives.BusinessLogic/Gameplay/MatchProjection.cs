using System.Collections.Generic;
using System.Linq;
using Network = DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public static class MatchProjection
    {
        public static Network.MatchView CreateView(Match match)
        {
            return new Network.MatchView
            {
                MatchId = match.MatchId,
                OpeningTileId = match.OpeningTileId,
                CurrentPlayerId = match.CurrentPlayerId,
                Version = match.Version,
                Phase = (Network.MatchPhase)match.Phase,
                RoundNumber = match.RoundNumber,
                StockCount = match.StockCount,
                Scores = CreateScores(match),
                LastRound = CreateRoundResult(match),
                LastPlay = CreatePlayScore(match.LastPlay),
                Board = new Network.PublicBoard
                {
                    Tiles = match.Board.Tiles.Select(placement
                        => new Network.PlacedTile { Tile = CreateTile(placement.Tile), End = (
                        Network.BoardEnd)placement.End, ConnectingValue =
                        placement.ConnectingValue, ExposedValue = placement.ExposedValue })
                        .ToList(),
                    Ends = match.Board.Ends.Select(end => new Network.PublicEnd { End = (
                        Network.BoardEnd)end.End, Value = end.Value, ScoreValue =
                        end.ScoreValue, IsBlocked = end.BlockedTurn == match.TurnNumber,
                        IsOpeningEnd = end.IsOpeningEnd })
                        .ToList()
                }
            };
        }

        public static Network.PlayerHand CreateHand(Match match, int playerId)
        {
            return new Network.PlayerHand
            {
                MatchId = match.MatchId,
                PlayerId = playerId,
                Version = match.Version,
                Tiles = match.GetPlayer(playerId).Tiles.Select(CreateTile)
                    .ToList()
            };
        }

        public static Network.MatchResult CreateResult(Match match)
        {
            return new Network.MatchResult
            {
                MatchId = match.MatchId,
                Scores = CreateScores(match),
                Version = match.Version,
                Phase = (Network.MatchPhase)match.Phase,
                WinnerIds = match.Phase == MatchPhase.Completed ? match.Players.Where(player
                    => player.Points >= match.TargetPoints)
                    .Select(player => player.PlayerId)
                    .ToList() : new List<int>()
            };
        }

        private static List<Network.PlayerScore> CreateScores(Match match)
        {
            return match.Players.Select(player => new Network.PlayerScore { PlayerId =
                player.PlayerId, Points = player.Points, TileCount = player.Tiles.Count })
                .ToList();
        }

        private static Network.PlayScore CreatePlayScore(PlayScore score)
        {
            if (score == null)
            {
                return null;
            }

            return new Network.PlayScore
            {
                PlayerId = score.PlayerId,
                RoundNumber = score.RoundNumber,
                Tile = CreateTile(score.Tile),
                PlayedEnd = (Network.BoardEnd)score.PlayedEnd,
                EndValues = score.EndValues.ToList(),
                BlockedEnds = score.BlockedEnds.Select(end => (Network.BoardEnd)end)
                    .ToList(),
                IncludesOpeningDouble = score.IncludesOpeningDouble,
                Total = score.Total,
                BasePoints = score.BasePoints,
                Multiplier = score.Multiplier,
                Points = score.Points,
                RoundPoints = score.RoundPoints
            };
        }

        private static Network.RoundResult CreateRoundResult(Match match)
        {
            RoundSummary round = match.LastRound;
            if (round == null)
            {
                return null;
            }

            return new Network.RoundResult
            {
                RoundNumber = round.RoundNumber,
                WinnerId = round.WinnerId,
                AwardedPoints = round.AwardedPoints,
                Scores = round.Scores.Select(score => new Network.PlayerScore { PlayerId =
                    score.Key, Points = score.Value, TileCount =
                    round.RemainingHands[score.Key].Length })
                    .ToList(),
                RevealedHands = round.RemainingHands.Select(hand
                    => new Network.PlayerHand { MatchId = match.MatchId, PlayerId = hand.Key,
                    Version = match.Version, Tiles = hand.Value.Select(CreateTile)
                    .ToList() })
                    .ToList()
            };
        }

        private static Network.DominoTile CreateTile(Tile tile)
        {
            return new Network.DominoTile
            {
                TileId = tile.TileId,
                FirstValue = tile.FirstValue,
                SecondValue = tile.SecondValue,
                SpecialPower = (Network.SpecialPower)tile.SpecialPower
            };
        }
    }
}
