using System.Collections.Generic;
using System.ServiceModel;
using DominoAllFives.Contracts.Domain;

namespace DominoAllFives.Contracts.Services
{
    [ServiceContract(Name = "IRankingRemoteService", Namespace = "urn:domino-all-fives:services")]
    public interface IRankingService
    {
        [OperationContract]
        List<RankingEntry> GetTopWinsRanking();

        [OperationContract]
        int GetWinsRankingPositionById(int playerId);
    }
}
