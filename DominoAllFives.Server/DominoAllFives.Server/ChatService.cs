using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.ServiceModel;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;
using DominoAllFives.Server.Infrastructure.Chat;
using DominoAllFives.Server.Logging;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server
{
    /// <summary>
    /// Coordinates authenticated chat operations and broadcasts messages to context participants.
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant,
        IncludeExceptionDetailInFaults = false)]
    public sealed class ChatService : IChatService
    {
        private static readonly ChatController controller;
        private static readonly ChatCallbackRegistry callbacks;
        private static readonly ChatEventPublisher publisher;
        private static readonly ILogger<ChatService> logger;
        private Player _player;
        private IChatCallback _callback;
        private ICommunicationObject _callbackChannel;
        static ChatService()
        {
            controller = new ChatController(LobbyManager.Instance);
            callbacks = new ChatCallbackRegistry();
            publisher = new ChatEventPublisher(
                callbacks,
                new ConsoleLogger<ChatEventPublisher>());
            logger = new ConsoleLogger<ChatService>();
        }

        /// <summary>
        /// Associates the duplex chat connection and callback with an authenticated player.
        /// </summary>
        /// <param name = "sessionKey">The authentication token for the player session.</param>
        /// <exception cref="FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref="FaultException">
        /// The server cannot resolve the player session because persistence failed.
        /// </exception>
        public void OpenChatSession(string sessionKey)
        {
            if (_player != null)
            {
                throw AccessFault(AccessIssue.PermissionDenied);
            }

            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                throw AccessFault(AccessIssue.SessionRequired);
            }

            try
            {
                _player = SessionManager.GetPlayer(sessionKey);
            }
            catch (KeyNotFoundException)
            {
                throw AccessFault(AccessIssue.SessionRequired);
            }
            catch (SqlException ex)
            {
                logger.LogError(ex, "Chat session could not resolve player.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (EntityException ex)
            {
                logger.LogError(ex, "Chat session could not resolve player.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Chat session could not resolve player.");
                throw new FaultException("The service could not complete the operation.");
            }

            IChatCallback callback = OperationContext.Current.GetCallbackChannel<IChatCallback>();
            bool callbackRegistered = callbacks.RegisterCallback(_player.PlayerId, callback);
            if (!callbackRegistered)
            {
                _player = null;
                throw AccessFault(AccessIssue.PermissionDenied);
            }

            _callback = callback;
            _callbackChannel = callback as ICommunicationObject;
            if (_callbackChannel != null)
            {
                _callbackChannel.Closed += OnCallbackEnded;
                _callbackChannel.Faulted += OnCallbackEnded;
            }
        }

        /// <summary>
        /// Registers and broadcasts a chat message to the authorized participants of its context.
        /// </summary>
        /// <param name = "message">The message containing the target context and text content.</param>
        /// <returns>The registered message with server-assigned identifier, sender, and timestamp.</returns>
        /// <exception cref="FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player is a guest account.
        /// </exception>
        /// <exception cref="FaultException{ChatIssue}">
        /// The message violates a chat business rule or the player is not in the target chat.
        /// </exception>
        public ChatMessage SendMessage(ChatMessage message)
        {
            Player player = GetPlayer();
            if (player.IsGuest)
            {
                throw AccessFault(AccessIssue.RegisteredAccountRequired);
            }

            if (message == null || message.Context == null)
            {
                throw ChatFault(ChatIssue.EmptyMessage);
            }

            try
            {
                ChatMessage registeredMessage = controller.SendMessage(player, message);
                List<Player> recipients = controller.GetRecipients(registeredMessage.Context);
                publisher.PublishMessageReceived(recipients, registeredMessage);
                return registeredMessage;
            }
            catch (ChatRuleException ex)
            {
                throw ChatFault(ex.Issue);
            }
        }

        /// <summary>
        /// Retrieves the ordered message history for a chat context accessible to the player.
        /// </summary>
        /// <param name = "context">The lobby or match chat context to query.</param>
        /// <returns>The list of messages registered in the specified context ordered by date.</returns>
        /// <exception cref="FaultException{AccessIssue}">
        /// The connection has no authenticated session.
        /// </exception>
        /// <exception cref="FaultException{ChatIssue}">
        /// The player is not an authorized participant of the requested chat context.
        /// </exception>
        public List<ChatMessage> GetMessages(ChatContext context)
        {
            Player player = GetPlayer();
            if (context == null)
            {
                throw ChatFault(ChatIssue.PlayerNotInChat);
            }

            try
            {
                return controller.GetMessages(player, context);
            }
            catch (ChatRuleException ex)
            {
                throw ChatFault(ex.Issue);
            }
        }

        private Player GetPlayer()
        {
            if (_player == null)
            {
                throw AccessFault(AccessIssue.SessionRequired);
            }

            return _player;
        }

        private static FaultException<ChatIssue> ChatFault(ChatIssue issue)
        {
            return new FaultException<ChatIssue>(issue);
        }

        private static FaultException<AccessIssue> AccessFault(AccessIssue issue)
        {
            return new FaultException<AccessIssue>(issue);
        }

        private void OnCallbackEnded(object sender, EventArgs eventArgs)
        {
            if (_player != null && _callback != null)
            {
                callbacks.RemoveCallback(_player.PlayerId, _callback);
            }

            if (_callbackChannel != null)
            {
                _callbackChannel.Closed -= OnCallbackEnded;
                _callbackChannel.Faulted -= OnCallbackEnded;
            }

            _callback = null;
            _callbackChannel = null;
        }
    }
}
