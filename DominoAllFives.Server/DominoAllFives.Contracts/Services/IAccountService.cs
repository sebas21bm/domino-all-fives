using System.ServiceModel;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>Manages the credentials and deletion of the account identified by a session.</summary>
    [ServiceContract(Namespace = "urn:domino-all-fives:services")]
    public interface IAccountService
    {
        /// <summary>Changes the password of the account identified by the session.</summary>
        /// <param name = "sessionKey">The server-issued account session.</param>
        /// <param name = "currentPassword">The password to verify.</param>
        /// <param name = "newPassword">The requested replacement password.</param>
        /// <exception cref = "FaultException{AccessIssue}">The session is unavailable.</exception>
        /// <exception cref = "FaultException{AccountIssue}">The account or password was rejected.</exception>
        /// <exception cref = "FaultException">Persistence failed.</exception>
        [OperationContract]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(AccountIssue))]
        void ChangePassword(string sessionKey, string currentPassword, string newPassword);

        /// <summary>Logically deletes the account and revokes its session after saving.</summary>
        /// <param name = "sessionKey">The server-issued account session.</param>
        /// <param name = "password">The current password to verify.</param>
        /// <exception cref = "FaultException{AccessIssue}">The session is unavailable.</exception>
        /// <exception cref = "FaultException{AccountIssue}">The account or password was rejected.</exception>
        /// <exception cref = "FaultException">Persistence failed.</exception>
        [OperationContract]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(AccountIssue))]
        void DeleteAccount(string sessionKey, string password);
    }
}
