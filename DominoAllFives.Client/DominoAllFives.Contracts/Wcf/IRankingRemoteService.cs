using System.Collections.Generic;
using System.ServiceModel;
using DominoAllFives.Contracts.Domain;

namespace DominoAllFives.Contracts.Wcf
{
    [ServiceContract(Namespace = "urn:domino-all-fives:services")]
    public interface IRankingRemoteService
    {
        [OperationContract]
        List<RankingEntry> GetTopWinsRanking();

        [OperationContract]
        int GetWinsRankingPositionById(int playerId);
    }
}
