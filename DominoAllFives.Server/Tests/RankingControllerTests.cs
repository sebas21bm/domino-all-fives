using System;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Contracts.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class RankingControllerTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public void GetWinsRankingPositionById_InvalidId_RejectsWithoutOpeningDatabase(int playerId)
        {
            // Arrange
            RankingController controller =
                new RankingController(
                new ProfilePictureStorage(
                NullLogger<ProfilePictureStorage>.Instance));
            // Act
            TestDelegate getRankingPosition = () => controller.GetWinsRankingPositionById(playerId);
            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(getRankingPosition);
        }

        [Test]
        public void Constructor_NullStorage_ThrowsArgumentNullException()
        {
            // Arrange
            ProfilePictureStorage storage = null;
            // Act
            TestDelegate createController = () => new RankingController(storage);
            // Assert
            Assert.Throws<ArgumentNullException>(createController);
        }

        [Test]
        public void FromRankedPlayer_ValidEntry_PreservesServerValues()
        {
            // Arrange
            ProfilePicture picture = ProfilePicture.FromStoredContent(Array.Empty<byte>());
            // Act
            RankingEntry entry = RankingEntry.FromRankedPlayer(2, "Player", 5, picture);
            // Assert
            Assert.That(new object[] { entry.Rank, entry.Username, entry.GamesWon, entry.Picture },
                Is.EqualTo(new object[] { 2, "Player", 5, picture }));
        }

        [Test]
        public void FromRankedPlayer_NullPicture_ThrowsArgumentNullException()
        {
            // Arrange
            ProfilePicture picture = null;
            // Act
            TestDelegate createEntry = () => RankingEntry.FromRankedPlayer(1, "Player", 5, picture);
            // Assert
            Assert.Throws<ArgumentNullException>(createEntry);
        }

        [Test]
        public void FromOrderedEntries_EmptyRanking_PreservesAbsentPosition()
        {
            // Arrange
            List<RankingEntry> entries = new List<RankingEntry>();
            // Act
            Ranking ranking = Ranking.FromOrderedEntries(entries, 0);
            // Assert
            Assert.That(new[] { ranking.TopPlayers.Count, ranking.CurrentPlayerRank },
                Is.EqualTo(new[] { 0, 0 }));
        }

        [Test]
        public void FromOrderedEntries_SourceChanges_PreservesSnapshot()
        {
            // Arrange
            ProfilePicture picture = ProfilePicture.FromStoredContent(Array.Empty<byte>());
            RankingEntry entry = RankingEntry.FromRankedPlayer(1, "Player", 5, picture);
            List<RankingEntry> entries = new List<RankingEntry> { entry };
            Ranking ranking = Ranking.FromOrderedEntries(entries, 1);
            // Act
            entries.Clear();
            // Assert
            Assert.That(ranking.TopPlayers, Is.EqualTo(new[] { entry }));
        }

        [Test]
        public void FromOrderedEntries_NullEntries_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<RankingEntry> entries = null;
            // Act
            TestDelegate createRanking = () => Ranking.FromOrderedEntries(entries, 0);
            // Assert
            Assert.Throws<ArgumentNullException>(createRanking);
        }
    }
}
