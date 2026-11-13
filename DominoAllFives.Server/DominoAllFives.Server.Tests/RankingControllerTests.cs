using System;
using System.ServiceModel;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace DominoAllFives.Server.Tests
{
    [TestFixture]
    public class RankingControllerTests
    {
        [TestCase(0)]
        [TestCase(-1)]
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

        [TestCase(0)]
        [TestCase(-1)]
        public void GetWinsRankingPositionById_InvalidId_ReturnsSanitizedFault(int playerId)
        {
            // Arrange
            DominoService service = new DominoService();
            // Act
            FaultException fault =
                Assert.Throws<FaultException>(
                () => service.GetWinsRankingPositionById(
                playerId));
            // Assert
            Assert.That(fault.Message, Is.EqualTo("Invalid request."));
        }
    }
}
