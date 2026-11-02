using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Wcf
{
    [ServiceContract(Namespace = "urn:domino-all-fives:services")]
    public interface ISessionAccount
    {
        [OperationContract]
        [FaultContract(typeof(AccountIssue))]
        PlayerAccount LoginWithAccount(Credentials credentials);
    }
}
