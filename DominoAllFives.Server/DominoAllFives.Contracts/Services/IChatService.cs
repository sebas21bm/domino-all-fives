using System.Collections.Generic;
using System.ServiceModel;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Services
{
    /// <summary>
    /// Defines authenticated duplex operations for sending and retrieving chat messages.
    /// </summary>
    [ServiceContract(
        CallbackContract = typeof(IChatCallback),
        SessionMode = SessionMode.Required)]
    public interface IChatService
    {
        /// <summary>
        /// Associates the duplex chat connection and callback with an authenticated player session.
        /// </summary>
        /// <param name = "sessionKey">The authentication token for the player session.</param>
        [OperationContract]
        [FaultContract(typeof(AccessIssue))]
        void OpenChatSession(string sessionKey);

        /// <summary>
        /// Registers and broadcasts a chat message to the authorized participants of its context.
        /// </summary>
        /// <param name = "message">The message containing the target context and text content.</param>
        /// <returns>The registered message with server-assigned identifier, sender, and timestamp.</returns>
        [OperationContract]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(ChatIssue))]
        ChatMessage SendMessage(ChatMessage message);

        /// <summary>
        /// Retrieves the ordered message history for a chat context accessible to the player.
        /// </summary>
        /// <param name = "context">The lobby or match chat context to query.</param>
        /// <returns>The list of messages registered in the specified context ordered by date.</returns>
        [OperationContract]
        [FaultContract(typeof(AccessIssue))]
        [FaultContract(typeof(ChatIssue))]
        List<ChatMessage> GetMessages(ChatContext context);
    }
}
