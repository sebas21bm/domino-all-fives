using System.ServiceModel;
using DominoAllFives.Contracts.Domain;

namespace DominoAllFives.Contracts.Services
{
    [ServiceContract(
        Namespace = "urn:domino-all-fives:services",
        CallbackContract = typeof(ILobbyCallback),
        SessionMode = SessionMode.Required)]
    public interface ILobbyService
    {
        [OperationContract]
        Lobby CreateLobby(LobbySettings settings);

        [OperationContract]
        Lobby JoinLobby(string lobbyCode);

        [OperationContract]
        void LeaveLobby();

        [OperationContract]
        Lobby GetLobby(string lobbyCode);

        [OperationContract]
        void StartMatch();

        [OperationContract]
        void CloseLobby();
    }
}
