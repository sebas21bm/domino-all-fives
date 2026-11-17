using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Gameplay;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public sealed class DeckGeneratorTests
    {
        [Test]
        public void Generate_DoubleSixDeck_ContainsEveryValuePairOnce()
        {
            // Arrange
            DeckGenerator generator = new DeckGenerator();
            HashSet<int> expectedValues = new HashSet<int>();
            for (int first = 0; first <= 6; first++)
            {
                for (int second = first; second <= 6; second++)
                {
                    expectedValues.Add(first * 10 + second);
                }
            }
            // Act
            List<Tile> deck = generator.Generate();
            // Assert
            HashSet<int> values = new HashSet<int>();
            HashSet<int> identifiers = new HashSet<int>();
            foreach (Tile tile in deck)
            {
                values.Add(tile.FirstValue * 10 + tile.SecondValue);
                identifiers.Add(tile.TileId);
            }

            Assert.That(
                new object[] { deck.Count, identifiers.Count, values.SetEquals(expectedValues) },
                Is.EqualTo(new object[] { 28, 28, true }));
        }

        [Test]
        public void Generate_SpecialTiles_AssignsExactlyFiveSupportedPowers()
        {
            // Arrange
            DeckGenerator generator = new DeckGenerator();
            // Act
            List<Tile> deck = generator.Generate();
            // Assert
            int ordinary = 0;
            int powered = 0;
            foreach (Tile tile in deck)
            {
                if (tile.SpecialPower == SpecialPower.None)
                {
                    ordinary++;
                }
                else if (tile.SpecialPower == SpecialPower.Block
                    || tile.SpecialPower == SpecialPower.DoublePoints)
                {
                    powered++;
                }
            }

            Assert.That(
                new object[] { ordinary, powered },
                Is.EqualTo(new object[] { 23, 5 }));
        }

        [Test]
        public void Generate_RepeatedCall_ReturnsIndependentDeck()
        {
            // Arrange
            DeckGenerator generator = new DeckGenerator();
            List<Tile> firstDeck = generator.Generate();
            // Act
            List<Tile> secondDeck = generator.Generate();
            firstDeck.Clear();
            // Assert
            Assert.That(secondDeck.Count, Is.EqualTo(28));
        }
    }
}
