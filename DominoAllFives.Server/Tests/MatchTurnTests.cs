using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Gameplay;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class MatchTurnTests
    {
        [TestCase("Play", MatchIssue.PlayerNotInMatch)]
        [TestCase("Draw", MatchIssue.PlayerNotInMatch)]
        [TestCase("Pass", MatchIssue.PlayerNotInMatch)]
        [TestCase("Cancel", MatchIssue.PlayerNotInMatch)]
        [TestCase("Play", MatchIssue.NotPlayersTurn)]
        [TestCase("Draw", MatchIssue.NotPlayersTurn)]
        [TestCase("Pass", MatchIssue.NotPlayersTurn)]
        [TestCase("Play", MatchIssue.StaleVersion)]
        [TestCase("Draw", MatchIssue.StaleVersion)]
        [TestCase("Pass", MatchIssue.StaleVersion)]
        [TestCase("Play", MatchIssue.MatchNotInProgress)]
        [TestCase("Draw", MatchIssue.MatchNotInProgress)]
        [TestCase("Pass", MatchIssue.MatchNotInProgress)]
        [TestCase("Play", MatchIssue.TileNotOwned)]
        [TestCase("Play", MatchIssue.OpeningTileRequired)]
        [TestCase("Play", MatchIssue.InvalidPlacement)]
        [TestCase("Draw", MatchIssue.DrawNotAllowed)]
        [TestCase("Pass", MatchIssue.PassNotAllowed)]
        public void Operation_InvalidContext_PreservesState(string operation, MatchIssue issue)
        {
            // Arrange
            List<Tile> deck = MatchTestDeck.Ordered(66, 55);
            Match match = new Match(1, new[] { 1, 2 }, 100, deck);
            int playerId = GetRequestPlayerId(match, issue);
            int tileId = GetRequestTileId(match, issue);
            BoardEndPosition end = BoardEndPosition.Left;
            if (issue == MatchIssue.MatchNotInProgress)
            {
                match.Cancel(match.CurrentPlayerId.Value);
            }

            long version = match.Version;
            if (issue == MatchIssue.StaleVersion)
            {
                version--;
            }

            if (issue == MatchIssue.InvalidPlacement)
            {
                end = (BoardEndPosition)99;
            }

            object[] before = CaptureState(match);
            MatchRuleException failure = null;
            // Act
            try
            {
                switch (operation)
                {
                    case "Play":
                        match.Play(playerId, tileId, end, version);
                        break;
                    case "Draw":
                        match.Draw(playerId, version);
                        break;
                    case "Pass":
                        match.Pass(playerId, version);
                        break;
                    case "Cancel":
                        match.Cancel(playerId);
                        break;
                }
            }
            catch (MatchRuleException ex)
            {
                failure = ex;
            }
            // Assert
            Assert.That(
                new object[] { failure?.Issue, CaptureState(match) },
                Is.EqualTo(new object[] { issue, before }));
        }

        private static int GetRequestPlayerId(Match match, MatchIssue issue)
        {
            if (issue == MatchIssue.PlayerNotInMatch)
            {
                return 99;
            }

            if (issue == MatchIssue.NotPlayersTurn)
            {
                return 2;
            }

            return match.CurrentPlayerId.Value;
        }

        private static int GetRequestTileId(Match match, MatchIssue issue)
        {
            if (issue == MatchIssue.TileNotOwned)
            {
                return 99;
            }

            if (issue == MatchIssue.OpeningTileRequired)
            {
                return match.GetPlayer(1).Tiles[1].TileId;
            }

            return match.OpeningTileId;
        }

        private static object[] CaptureState(Match match)
        {
            List<int> hands = new List<int>();
            List<int> points = new List<int>();
            foreach (MatchPlayer player in match.Players)
            {
                points.Add(player.Points);
                foreach (Tile tile in player.Tiles)
                {
                    hands.Add(tile.TileId);
                }
            }

            return new object[]
            {
                match.Version,
                match.Phase,
                match.CurrentPlayerId,
                match.TurnNumber,
                match.RoundNumber,
                match.StockCount,
                match.Board.Tiles.Count,
                match.LastRound,
                match.LastPlay,
                hands,
                points
            };
        }
    }
}
