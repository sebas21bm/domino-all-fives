using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Defines authenticated duplex operations for sending chat messages.
    /// </summary>
    [ServiceContract(
        CallbackContract = typeof(IChatCallback),
        SessionMode = SessionMode.Required)]
    public interface IChatService
    {
        /// <summary>
        /// Associates the duplex chat connection and callback with an authenticated player session.
        /// </summary>
        /// <param name="sessionKey">The authentication token for the player session.</param>
        /// <exception cref="FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref="FaultException">
        /// The server cannot resolve the player session because persistence failed.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(AccessIssue))]
        void OpenChatSession(string sessionKey);

        /// <summary>
        /// Broadcasts a chat message to the authorized participants of its context.
        /// </summary>
        /// <param name="message">The message containing the target context and text content.</param>
        /// <exception cref="FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player is a guest account.
        /// </exception>
        /// <exception cref="FaultException{ChatIssue}">
        /// The message violates a chat validation rule or the player is not in the target chat.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(ChatIssue))]
        void SendMessage(ChatMessage message);
    }
}