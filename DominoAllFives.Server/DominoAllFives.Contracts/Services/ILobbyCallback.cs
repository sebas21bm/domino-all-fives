using System.ServiceModel;
using DominoAllFives.Contracts.Domain;

namespace DominoAllFives.Contracts.Services
{
    public interface ILobbyCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnLobbyChanged(Lobby lobby);

        [OperationContract(IsOneWay = true)]
        void OnLobbyClosed();

        [OperationContract(IsOneWay = true)]
        void OnMatchStarted(int matchId);
    }
}
