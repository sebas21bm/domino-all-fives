using System.ServiceModel;
using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Interfaces
{
    /// <summary>Submits account changes for the supplied server session.</summary>
    /// <remarks>Communication and timeout failures propagate to the caller.</remarks>
    public interface IAccountClient
    {
        /// <summary>Changes the password of the account identified by the session.</summary>
        /// <param name = "sessionKey">The server-issued account session.</param>
        /// <param name = "currentPassword">The password to verify.</param>
        /// <param name = "newPassword">The requested replacement password.</param>
        /// <exception cref = "FaultException{AccessIssue}">The session is unavailable.</exception>
        /// <exception cref = "FaultException{AccountIssue}">
        /// The account or password was rejected.
        /// </exception>
        /// <exception cref = "FaultException">The server could not complete the write.</exception>
        void ChangePassword(string sessionKey, string currentPassword, string newPassword);

        /// <summary>Deletes the account identified by the session.</summary>
        /// <param name = "sessionKey">The server-issued account session.</param>
        /// <param name = "password">The current password to verify.</param>
        /// <exception cref = "FaultException{AccessIssue}">The session is unavailable.</exception>
        /// <exception cref = "FaultException{AccountIssue}">
        /// The account or password was rejected.
        /// </exception>
        /// <exception cref = "FaultException">The server could not complete the write.</exception>
        void DeleteAccount(string sessionKey, string password);

        /// <summary>Changes the preferred language for the session's account.</summary>
        /// <param name = "sessionKey">The server-issued account session.</param>
        /// <param name = "preferredLanguageId">The requested language identifier.</param>
        /// <exception cref = "FaultException{AccessIssue}">The session is unavailable.</exception>
        /// <exception cref = "FaultException{AccountIssue}">
        /// The account is unavailable or the language is unsupported.
        /// </exception>
        /// <exception cref = "FaultException">The server could not complete the write.</exception>
        void ChangePreferredLanguage(string sessionKey, string preferredLanguageId);
    }
}
