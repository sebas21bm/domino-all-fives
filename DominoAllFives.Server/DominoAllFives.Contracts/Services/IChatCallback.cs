using System.ServiceModel;
using DominoAllFives.Contracts.Domain;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Defines the notification operations pushed from the chat service to subscribed clients.
    /// </summary>
    public interface IChatCallback
    {
        /// <summary>
        /// Notifies the client that a new chat message was posted in its active context.
        /// </summary>
        /// <param name = "message">The registered chat message.</param>
        [OperationContract(IsOneWay = true)]
        void OnMessageReceived(ChatMessage message);
    }
}
