using System.ServiceModel;
using NUnit.Framework;

namespace DominoAllFives.Server.AdditionalTest
{
    [TestFixture]
    public class RankingServiceTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void GetWinsRankingPositionById_InvalidId_ReturnsSanitizedFault(int playerId)
        {
            // Arrange
            DominoService service = new DominoService();
            // Act
            TestDelegate getRankingPosition = () => service.GetWinsRankingPositionById(playerId);
            // Assert
            Assert.That(getRankingPosition, Throws.TypeOf<FaultException>()
                .With.Message.EqualTo("Invalid request."));
        }
    }
}
