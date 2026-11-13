using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DominoAllFives.Client.WPF.Models;
using MatchService;
using NUnit.Framework;

namespace DominoAllFives.Client.Tests
{
    [TestFixture]
    public sealed class PlayScoreFormatterTests
    {
        [TestCase("es-MX", "Roja", "Puntos ganados")]
        [TestCase("en-US", "Red", "Points earned")]
        [TestCase("pt-BR", "Vermelha", "Pontos ganhos")]
        public void Format_RedPlay_ExplainsAuthoritativeValuesInSelectedLanguage(
            string cultureName, string powerText, string pointsText)
        {
            // Arrange
            CultureInfo previous = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
            try
            {
                PlayScore score = CreateScore();
                // Act
                string text = PlayScoreFormatter.Format(score, "Alice");
                // Assert
                Assert.That(text, Does.Contain("12 + 5 + 3 = 20"));
                Assert.That(text, Does.Contain("20 × 2 = 40"));
                Assert.That(text, Does.Contain(powerText));
                Assert.That(text, Does.Contain(pointsText));
                Assert.That(text, Does.Contain("+40"));
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = previous;
            }
        }

        [Test]
        public void Format_PurpleAndBlockedEnds_DistinguishesCurrentExclusionFromNextTurn()
        {
            // Arrange
            CultureInfo previous = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            try
            {
                PlayScore score = CreateScore();
                score.Tile.SpecialPower = SpecialPower.Block;
                score.BlockedEnds.Add(BoardEnd.Left);
                score.PlayedEnd = BoardEnd.Top;
                score.RoundPoints = 25;
                // Act
                string text = PlayScoreFormatter.Format(score, "Alice");
                // Assert
                Assert.That(text, Does.Contain("Excluded from this play"));
                Assert.That(text, Does.Contain("Left"));
                Assert.That(text, Does.Contain("blocks Top next turn"));
                Assert.That(text, Does.Contain("award: +25"));
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = previous;
            }
        }

        [Test]
        public void Format_NoPlay_ShowsLocalizedPendingExplanation()
        {
            // Arrange
            PlayScore score = null;
            // Act
            string text = PlayScoreFormatter.Format(score, string.Empty);
            // Assert
            Assert.That(text, Is.Not.Empty);
            Assert.That(text, Does.Not.StartWith("["));
        }

        private static PlayScore CreateScore()
        {
            return new PlayScore
            {
                PlayerId = 1,
                RoundNumber = 2,
                Tile = new DominoTile
                {
                    FirstValue = 4,
                    SecondValue = 5,
                    SpecialPower = SpecialPower.DoublePoints
                },
                EndValues = new List<int>
                {
                    12,
                    5,
                    3
                },
                BlockedEnds = new List<BoardEnd>(),
                IncludesOpeningDouble = true,
                Total = 20,
                BasePoints = 20,
                Multiplier = 2,
                Points = 40
            };
        }
    }
}
