using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Exposes account authentication and session creation over WCF.
    /// </summary>
    [ServiceContract(Name = "ISessionAccount", Namespace = "urn:domino-all-fives:services")]
    public interface ISessionAccountService
    {
        /// <summary>
        /// Authenticates credentials and returns the account with its server session.
        /// </summary>
        /// <param name = "credentials">The account identifier and password to validate.</param>
        /// <returns>The authenticated account and its session token.</returns>
        /// <exception cref="FaultException{AccountIssue}">
        /// The credentials are invalid or the account is banned or suspended.
        /// </exception>
        /// <exception cref="FaultException">
        /// The server cannot authenticate the account because persistence failed.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(AccountIssue))]
        PlayerAccount LoginWithAccount(Credentials credentials);
    }
}
