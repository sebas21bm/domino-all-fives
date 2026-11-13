using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Exposes authenticated lobby operations over a duplex WCF session.
    /// </summary>
    [ServiceContract(
        Namespace = "urn:domino-all-fives:services",
        CallbackContract = typeof(ILobbyCallback),
        SessionMode = SessionMode.Required)]
    public interface ILobbyService
    {
        /// <summary>
        /// Associates the duplex connection and callback with an authenticated player.
        /// </summary>
        /// <param name = "sessionKey">The authentication token for the player session.</param>
        /// <exception cref="FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref="FaultException">
        /// The server cannot resolve the player session because persistence failed.
        /// </exception>
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        [FaultContract(typeof(AccessIssue))]
        void OpenLobbySession(string sessionKey);
        /// <summary>
        /// Creates a lobby owned by the authenticated player.
        /// </summary>
        /// <param name = "settings">The requested lobby configuration.</param>
        /// <returns>The created lobby snapshot.</returns>
        /// <exception cref="FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref="FaultException{LobbyIssue}">
        /// The requested operation violates a lobby rule or the lobby is unavailable.
        /// </exception>
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(LobbyIssue))]
        [FaultContract(typeof(AccessIssue))]
        Lobby CreateLobby(LobbySettings settings);
        /// <summary>
        /// Adds the authenticated player to an existing lobby.
        /// </summary>
        /// <param name = "lobbyCode">The code identifying the target lobby.</param>
        /// <returns>The lobby snapshot after the player joins.</returns>
        /// <exception cref="FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref="FaultException{LobbyIssue}">
        /// The requested operation violates a lobby rule or the lobby is unavailable.
        /// </exception>
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(LobbyIssue))]
        [FaultContract(typeof(AccessIssue))]
        Lobby JoinLobby(string lobbyCode);
        /// <summary>
        /// Removes the authenticated player from the current lobby.
        /// </summary>
        /// <exception cref="FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref="FaultException{LobbyIssue}">
        /// The requested operation violates a lobby rule or the lobby is unavailable.
        /// </exception>
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(LobbyIssue))]
        [FaultContract(typeof(AccessIssue))]
        void LeaveLobby();
        /// <summary>
        /// Retrieves a lobby snapshot for an authenticated connection.
        /// </summary>
        /// <param name = "lobbyCode">The code identifying the requested lobby.</param>
        /// <returns>The current lobby snapshot.</returns>
        /// <exception cref="FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref="FaultException{LobbyIssue}">
        /// The requested operation violates a lobby rule or the lobby is unavailable.
        /// </exception>
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(LobbyIssue))]
        [FaultContract(typeof(AccessIssue))]
        Lobby GetLobby(string lobbyCode);
        /// <summary>
        /// Starts the current lobby when the authenticated player is its owner.
        /// </summary>
        /// <exception cref="FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref="FaultException{LobbyIssue}">
        /// The requested operation violates a lobby rule or the lobby is unavailable.
        /// </exception>
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(LobbyIssue))]
        [FaultContract(typeof(AccessIssue))]
        void StartMatch();
        /// <summary>
        /// Closes the current lobby when the authenticated player is its owner.
        /// </summary>
        /// <exception cref="FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref="FaultException{LobbyIssue}">
        /// The requested operation violates a lobby rule or the lobby is unavailable.
        /// </exception>
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(LobbyIssue))]
        [FaultContract(typeof(AccessIssue))]
        void CloseLobby();
    }
}
