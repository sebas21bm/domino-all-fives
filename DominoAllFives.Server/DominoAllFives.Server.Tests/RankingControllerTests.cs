using System;
using System.ServiceModel;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.BusinessLogic.Controllers;
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
            RankingController controller = new RankingController(
                new ProfilePictureStorage(NullLogger<ProfilePictureStorage>.Instance));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => controller.GetWinsRankingPositionById(playerId));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void GetWinsRankingPositionById_InvalidId_ReturnsSanitizedFault(int playerId)
        {
            DominoService service = new DominoService();

            FaultException fault = Assert.Throws<FaultException>(
                () => service.GetWinsRankingPositionById(playerId));

            Assert.That(fault.Message, Is.EqualTo("Invalid request."));
        }
    }
}
