using System.ServiceModel;
using DominoAllFives.Contracts.Domain;

namespace DominoAllFives.Contracts.Wcf
{
    [ServiceContract(Namespace = "urn:domino-all-fives:services")]
    public interface IRankingRemoteService
    {
        [OperationContract]
        Ranking GetTopRanking(int playerId);
    }
}
