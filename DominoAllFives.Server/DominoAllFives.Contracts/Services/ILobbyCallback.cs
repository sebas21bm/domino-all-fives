using System.ServiceModel;
using DominoAllFives.Contracts.Domain;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Notifies a connected player about changes to the current lobby.
    /// </summary>
    public interface ILobbyCallback
    {
        /// <summary>
        /// Receives the current lobby snapshot after its participants or state change.
        /// </summary>
        /// <param name = "lobby">The updated lobby snapshot.</param>
        [OperationContract(IsOneWay = true)]
        void OnLobbyChanged(Lobby lobby);
        /// <summary>
        /// Notifies the client that its current lobby has been closed.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void OnLobbyClosed();
        /// <summary>
        /// Notifies the client that the lobby has entered a match.
        /// </summary>
        /// <param name = "matchId">The identifier of the started match.</param>
        [OperationContract(IsOneWay = true)]
        void OnMatchStarted(int matchId);
    }
}
