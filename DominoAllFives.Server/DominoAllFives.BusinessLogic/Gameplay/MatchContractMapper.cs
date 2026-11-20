using System.Collections.Generic;
using GameplayContracts = DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    internal static class MatchContractMapper
    {
        public static GameplayContracts.MatchView CreateMatchView(Match match)
        {
            GameplayContracts.MatchResult result = null;
            if (match.Phase != MatchState.InProgress)
            {
                result = CreateMatchResult(match);
            }

            return new GameplayContracts.MatchView
            {
                MatchId = match.MatchId,
                OpeningTileId = match.OpeningTileId,
                CurrentPlayerId = match.CurrentPlayerId,
                Version = match.Version,
                Phase = (GameplayContracts.MatchStatus)match.Phase,
                RoundNumber = match.RoundNumber,
                StockCount = match.StockCount,
                Scores = CreatePlayerScores(match),
                LastRound = CreateRoundResult(match),
                LastPlay = CreatePlayScore(match.LastPlay),
                LastTurnAction = CreateTurnActionNotice(match.LastTurnAction),
                Result = result,
                Board = CreatePublicBoard(match.Board, match.TurnNumber)
            };
        }

        public static GameplayContracts.PlayerHand CreatePlayerHand(
            Match match,
            int playerId)
        {
            List<GameplayContracts.DominoTile> dominoTiles =
                new List<GameplayContracts.DominoTile>();
            MatchPlayer player = match.GetPlayer(playerId);
            foreach (Tile tile in player.Tiles)
            {
                GameplayContracts.DominoTile dominoTile = CreateDominoTile(tile);
                dominoTiles.Add(dominoTile);
            }

            return new GameplayContracts.PlayerHand
            {
                MatchId = match.MatchId,
                PlayerId = playerId,
                Version = match.Version,
                Tiles = dominoTiles
            };
        }

        public static GameplayContracts.MatchResult CreateMatchResult(Match match)
        {
            List<int> winnerIds = new List<int>();
            if (match.Phase == MatchState.Completed)
            {
                foreach (MatchPlayer player in match.Players)
                {
                    if (player.Points >= match.TargetPoints)
                    {
                        winnerIds.Add(player.PlayerId);
                    }
                }
            }

            return new GameplayContracts.MatchResult
            {
                MatchId = match.MatchId,
                Scores = CreatePlayerScores(match),
                Version = match.Version,
                Phase = (GameplayContracts.MatchStatus)match.Phase,
                WinnerIds = winnerIds
            };
        }

        private static GameplayContracts.PublicBoard CreatePublicBoard(Board board, int turnNumber)
        {
            List<GameplayContracts.PlacedTile> placedTiles =
                new List<GameplayContracts.PlacedTile>();
            foreach (Placement placement in board.Tiles)
            {
                GameplayContracts.PlacedTile placedTile = CreatePlacedTile(placement);
                placedTiles.Add(placedTile);
            }

            List<GameplayContracts.PublicEnd> publicEnds =
                new List<GameplayContracts.PublicEnd>();
            foreach (BoardEndState boardEnd in board.Ends)
            {
                GameplayContracts.PublicEnd publicEnd = CreatePublicBoardEnd(
                    boardEnd,
                    turnNumber);
                publicEnds.Add(publicEnd);
            }

            return new GameplayContracts.PublicBoard
            {
                Tiles = placedTiles,
                Ends = publicEnds
            };
        }

        private static GameplayContracts.TurnActionNotice CreateTurnActionNotice(TurnAction action)
        {
            if (action == null)
            {
                return null;
            }

            return new GameplayContracts.TurnActionNotice
            {
                PlayerId = action.PlayerId,
                IsPass = action.IsPass,
                Version = action.Version
            };
        }

        private static List<GameplayContracts.PlayerScore> CreatePlayerScores(Match match)
        {
            List<GameplayContracts.PlayerScore> playerScores =
                new List<GameplayContracts.PlayerScore>();
            foreach (MatchPlayer player in match.Players)
            {
                GameplayContracts.PlayerScore playerScore = CreatePlayerScore(player);
                playerScores.Add(playerScore);
            }

            return playerScores;
        }

        private static GameplayContracts.PlayScore CreatePlayScore(PlayScore score)
        {
            if (score == null)
            {
                return null;
            }

            List<int> endValues = new List<int>();
            foreach (int endValue in score.EndValues)
            {
                endValues.Add(endValue);
            }

            List<GameplayContracts.BoardEnd> blockedEnds =
                new List<GameplayContracts.BoardEnd>();
            foreach (BoardEndPosition blockedEnd in score.BlockedEnds)
            {
                GameplayContracts.BoardEnd contractEnd =
                    (GameplayContracts.BoardEnd)blockedEnd;
                blockedEnds.Add(contractEnd);
            }

            return new GameplayContracts.PlayScore
            {
                PlayerId = score.PlayerId,
                RoundNumber = score.RoundNumber,
                Tile = CreateDominoTile(score.Tile),
                PlayedEnd = (GameplayContracts.BoardEnd)score.PlayedEnd,
                EndValues = endValues,
                BlockedEnds = blockedEnds,
                IncludesOpeningDouble = score.IncludesOpeningDouble,
                Total = score.Total,
                BasePoints = score.BasePoints,
                Multiplier = score.Multiplier,
                Points = score.Points,
                RoundPoints = score.RoundPoints
            };
        }

        private static GameplayContracts.RoundResult CreateRoundResult(Match match)
        {
            RoundSummary round = match.LastRound;
            if (round == null)
            {
                return null;
            }

            List<GameplayContracts.PlayerScore> playerScores =
                new List<GameplayContracts.PlayerScore>();
            foreach (KeyValuePair<int, int> score in round.Scores)
            {
                GameplayContracts.PlayerScore playerScore = CreateRoundPlayerScore(
                    score,
                    round);
                playerScores.Add(playerScore);
            }

            List<GameplayContracts.PlayerHand> revealedHands =
                new List<GameplayContracts.PlayerHand>();
            foreach (KeyValuePair<int, IReadOnlyList<Tile>> hand in round.RemainingHands)
            {
                GameplayContracts.PlayerHand revealedHand = CreateRevealedPlayerHand(
                    hand,
                    match);
                revealedHands.Add(revealedHand);
            }

            return new GameplayContracts.RoundResult
            {
                RoundNumber = round.RoundNumber,
                WinnerId = round.WinnerId,
                AwardedPoints = round.AwardedPoints,
                Scores = playerScores,
                RevealedHands = revealedHands,
                ClosedBoard = CreatePublicBoard(round.ClosedBoard, round.ClosingTurn),
                Version = round.Version
            };
        }

        private static GameplayContracts.PlacedTile CreatePlacedTile(Placement placement)
        {
            return new GameplayContracts.PlacedTile
            {
                Tile = CreateDominoTile(placement.Tile),
                End = (GameplayContracts.BoardEnd)placement.End,
                ConnectingValue = placement.ConnectingValue,
                ExposedValue = placement.ExposedValue
            };
        }

        private static GameplayContracts.PublicEnd CreatePublicBoardEnd(
            BoardEndState boardEnd,
            int turnNumber)
        {
            return new GameplayContracts.PublicEnd
            {
                End = (GameplayContracts.BoardEnd)boardEnd.End,
                Value = boardEnd.Value,
                ScoreValue = boardEnd.ScoreValue,
                IsBlocked = boardEnd.BlockedTurn == turnNumber,
                IsOpeningEnd = boardEnd.IsOpeningEnd
            };
        }

        private static GameplayContracts.PlayerScore CreatePlayerScore(MatchPlayer player)
        {
            return new GameplayContracts.PlayerScore
            {
                PlayerId = player.PlayerId,
                Points = player.Points,
                TileCount = player.Tiles.Count
            };
        }

        private static GameplayContracts.PlayerScore CreateRoundPlayerScore(
            KeyValuePair<int, int> score,
            RoundSummary round)
        {
            return new GameplayContracts.PlayerScore
            {
                PlayerId = score.Key,
                Points = score.Value,
                TileCount = round.RemainingHands[score.Key].Count
            };
        }

        private static GameplayContracts.PlayerHand CreateRevealedPlayerHand(
            KeyValuePair<int, IReadOnlyList<Tile>> hand,
            Match match)
        {
            List<GameplayContracts.DominoTile> dominoTiles =
                new List<GameplayContracts.DominoTile>();
            foreach (Tile tile in hand.Value)
            {
                GameplayContracts.DominoTile dominoTile = CreateDominoTile(tile);
                dominoTiles.Add(dominoTile);
            }

            return new GameplayContracts.PlayerHand
            {
                MatchId = match.MatchId,
                PlayerId = hand.Key,
                Version = match.LastRound.Version,
                Tiles = dominoTiles
            };
        }

        private static GameplayContracts.DominoTile CreateDominoTile(Tile tile)
        {
            return new GameplayContracts.DominoTile
            {
                TileId = tile.TileId,
                FirstValue = tile.FirstValue,
                SecondValue = tile.SecondValue,
                SpecialPower = (GameplayContracts.SpecialPower)tile.SpecialPower
            };
        }
    }
}
