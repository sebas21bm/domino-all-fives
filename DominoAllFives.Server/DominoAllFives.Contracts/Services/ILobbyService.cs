using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Services
{
    [ServiceContract(
        Namespace = "urn:domino-all-fives:services",
        CallbackContract = typeof(ILobbyCallback),
        SessionMode = SessionMode.Required)]
    public interface ILobbyService
    {
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        [FaultContract(typeof(AccessIssue))]
        void OpenLobbySession(string sessionKey);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(LobbyIssue))]
        [FaultContract(typeof(AccessIssue))]
        Lobby CreateLobby(LobbySettings settings);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(LobbyIssue))]
        [FaultContract(typeof(AccessIssue))]
        Lobby JoinLobby(string lobbyCode);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(LobbyIssue))]
        [FaultContract(typeof(AccessIssue))]
        void LeaveLobby();

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(LobbyIssue))]
        [FaultContract(typeof(AccessIssue))]
        Lobby GetLobby(string lobbyCode);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(LobbyIssue))]
        [FaultContract(typeof(AccessIssue))]
        void StartMatch();

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        [FaultContract(typeof(LobbyIssue))]
        [FaultContract(typeof(AccessIssue))]
        void CloseLobby();
    }
}
