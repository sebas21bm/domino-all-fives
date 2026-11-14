using System;
using DominoAllFives.BusinessLogic.Gameplay;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests.Gameplay
{
    [TestFixture]
    [Category("Unit")]
    public sealed class TileTests
    {
        [Test]
        public void IsDouble_EqualValues_ReturnsTrue()
        {
            // Arrange
            Tile tile = new Tile(1, 3, 3, SpecialPower.None);

            // Act
            bool result = tile.IsDouble;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsDouble_DifferentValues_ReturnsFalse()
        {
            // Arrange
            Tile tile = new Tile(1, 2, 5, SpecialPower.None);

            // Act
            bool result = tile.IsDouble;

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsDouble_BlankDouble_ReturnsTrue()
        {
            // Arrange
            Tile tile = new Tile(1, 0, 0, SpecialPower.None);

            // Act
            bool result = tile.IsDouble;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void TotalPips_DifferentValues_ReturnsTheirSum()
        {
            // Arrange
            Tile tile = new Tile(1, 2, 5, SpecialPower.None);

            // Act
            int result = tile.TotalPips;

            // Assert
            Assert.That(result, Is.EqualTo(7));
        }

        [Test]
        public void TotalPips_BlankDouble_ReturnsZero()
        {
            // Arrange
            Tile tile = new Tile(1, 0, 0, SpecialPower.None);

            // Act
            int result = tile.TotalPips;

            // Assert
            Assert.That(result, Is.Zero);
        }

        [Test]
        public void TotalPips_HighestDouble_ReturnsTwelve()
        {
            // Arrange
            Tile tile = new Tile(1, 6, 6, SpecialPower.None);

            // Act
            int result = tile.TotalPips;

            // Assert
            Assert.That(result, Is.EqualTo(12));
        }

        [Test]
        public void Matches_FirstValue_ReturnsTrue()
        {
            // Arrange
            Tile tile = new Tile(1, 2, 5, SpecialPower.None);

            // Act
            bool result = tile.Matches(2);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Matches_SecondValue_ReturnsTrue()
        {
            // Arrange
            Tile tile = new Tile(1, 2, 5, SpecialPower.None);

            // Act
            bool result = tile.Matches(5);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Matches_ValueNotOnTile_ReturnsFalse()
        {
            // Arrange
            Tile tile = new Tile(1, 2, 5, SpecialPower.None);

            // Act
            bool result = tile.Matches(4);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetOtherValue_FirstValue_ReturnsSecondValue()
        {
            // Arrange
            Tile tile = new Tile(1, 2, 5, SpecialPower.None);

            // Act
            int result = tile.GetOtherValue(2);

            // Assert
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        public void GetOtherValue_SecondValue_ReturnsFirstValue()
        {
            // Arrange
            Tile tile = new Tile(1, 2, 5, SpecialPower.None);

            // Act
            int result = tile.GetOtherValue(5);

            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void GetOtherValue_ValueNotOnTile_ThrowsMatchRuleException()
        {
            // Arrange
            Tile tile = new Tile(1, 2, 5, SpecialPower.None);

            // Act
            TestDelegate getOtherValue = () => tile.GetOtherValue(4);

            // Assert
            Assert.Throws<MatchRuleException>(getOtherValue);
        }

        [Test]
        public void Constructor_ZeroIdentifier_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int tileId = 0;

            // Act
            TestDelegate createTile = () => new Tile(tileId, 2, 5, SpecialPower.None);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(createTile);
        }

        [Test]
        public void Constructor_NegativeFirstValue_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int firstValue = -1;

            // Act
            TestDelegate createTile = () => new Tile(1, firstValue, 5, SpecialPower.None);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(createTile);
        }

        [Test]
        public void Constructor_SecondValueAboveSix_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            int secondValue = 7;

            // Act
            TestDelegate createTile = () => new Tile(1, 2, secondValue, SpecialPower.None);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(createTile);
        }

        [Test]
        public void Constructor_UndefinedSpecialPower_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            SpecialPower undefinedPower = (SpecialPower)99;

            // Act
            TestDelegate createTile = () => new Tile(1, 2, 5, undefinedPower);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(createTile);
        }
    }
}
