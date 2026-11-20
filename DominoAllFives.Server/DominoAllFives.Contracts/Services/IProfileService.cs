using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Exposes profile operations for the account identified by its server session.
    /// </summary>
    [ServiceContract(Namespace = "urn:domino-all-fives:services")]
    public interface IProfileService
    {
        /// <summary>Gets the profile belonging to the supplied session.</summary>
        /// <param name = "sessionKey">The server-issued account session.</param>
        /// <returns>The account identity, statistics and optional image content.</returns>
        /// <exception cref = "FaultException{AccessIssue}">The session is unavailable.</exception>
        /// <exception cref = "FaultException{ProfileIssue}">
        /// The profile is unavailable.
        /// </exception>
        /// <exception cref = "FaultException">The operation could not be completed.</exception>
        [OperationContract]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(ProfileIssue))]
        PlayerProfile GetMyProfile(string sessionKey);

        /// <summary>Updates the username while preserving the existing profile picture.</summary>
        /// <param name = "sessionKey">The server-issued account session.</param>
        /// <param name = "username">The requested username.</param>
        /// <exception cref = "FaultException{AccessIssue}">The session is unavailable.</exception>
        /// <exception cref = "FaultException{ProfileIssue}">
        /// The update was rejected.
        /// </exception>
        /// <exception cref = "FaultException">The operation could not be completed.</exception>
        [OperationContract]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(ProfileIssue))]
        void UpdateProfileName(string sessionKey, string username);

        /// <summary>Updates the username and replaces the profile picture together.</summary>
        /// <param name = "sessionKey">The server-issued account session.</param>
        /// <param name = "username">The requested username.</param>
        /// <param name = "picture">The explicit replacement content and selection metadata.</param>
        /// <exception cref = "FaultException{AccessIssue}">The session is unavailable.</exception>
        /// <exception cref = "FaultException{ProfileIssue}">
        /// The update was rejected.
        /// </exception>
        /// <exception cref = "FaultException">The operation could not be completed.</exception>
        [OperationContract]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(ProfileIssue))]
        void UpdateProfileWithPicture(string sessionKey, string username, ProfilePicture picture);

        /// <summary>Replaces the profile picture belonging to the supplied session.</summary>
        /// <param name = "sessionKey">The server-issued account session.</param>
        /// <param name = "picture">The selected image content and original filename.</param>
        /// <exception cref = "FaultException{AccessIssue}">The session is unavailable.</exception>
        /// <exception cref = "FaultException{ProfileIssue}">
        /// The picture was rejected.
        /// </exception>
        /// <exception cref = "FaultException">The operation could not be completed.</exception>
        [OperationContract]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(ProfileIssue))]
        void SetProfilePicture(string sessionKey, ProfilePicture picture);

        /// <summary>Removes the profile picture belonging to the supplied session.</summary>
        /// <param name = "sessionKey">The server-issued account session.</param>
        /// <exception cref = "FaultException{AccessIssue}">The session is unavailable.</exception>
        /// <exception cref = "FaultException{ProfileIssue}">
        /// The removal was rejected.
        /// </exception>
        /// <exception cref = "FaultException">The operation could not be completed.</exception>
        [OperationContract]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(ProfileIssue))]
        void RemoveProfilePicture(string sessionKey);
    }
}
