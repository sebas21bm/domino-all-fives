using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.ServiceModel;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.BusinessLogic.Storage;
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
        private static readonly ProfilePictureStorage pictureStorage =
            new ProfilePictureStorage(LoggerFactoryProvider.CreateLogger<ProfilePictureStorage>());
        private Player _player;
        private IChatCallback _callback;
        private ICommunicationObject _callbackChannel;
        static ChatService()
        {
            controller = new ChatController(LobbyManager.Instance);
            callbacks = new ChatCallbackRegistry();
            publisher = new ChatEventPublisher(
                callbacks,
                LoggerFactoryProvider.CreateLogger<ChatEventPublisher>());

            logger =
                LoggerFactoryProvider.CreateLogger<ChatService>();
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
        /// </exception
        public void OpenChatSession(string sessionKey)
        {
            if (_player != null)
            {
                throw new FaultException<AccessIssue>(AccessIssue.PermissionDenied);
            }

            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                throw new FaultException<AccessIssue>(AccessIssue.SessionRequired);
            }

            try
            {
                _player = SessionManager.GetPlayer(sessionKey, pictureStorage);
            }
            catch (KeyNotFoundException)
            {
                throw new FaultException<AccessIssue>(AccessIssue.SessionRequired);
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
                throw new FaultException<AccessIssue>(AccessIssue.PermissionDenied);
            }

            _callback = callback;
            _callbackChannel = callback as ICommunicationObject;
            if (_callbackChannel != null)
            {
                _callbackChannel.Closed += OnCallbackEnded;
                _callbackChannel.Faulted += OnCallbackEnded;
            }
        }

        public void SendMessage(ChatMessage message)
        {

        }

        /*
        /// <summary>
        /// Registers and broadcasts a chat message to the authorized participants of its context.
        /// </summary>
        /// <param name = "message">
        /// The message containing the target context and text content.
        /// </param>
        /// <returns>
        /// The registered message with server-assigned identifier, sender, and timestamp.
        /// </returns>
        /// <exception cref="FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player is a guest account.
        /// </exception>
        /// <exception cref="FaultException{ChatIssue}">
        /// The message violates a chat business rule or the player is not in the target chat.
        /// </exception>
        public ChatMessage SendMessage(ChatMessage message)
        {
            
            //Player player = GetPlayer();
            if (player.IsGuest)
            {
                throw new FaultException<AccessIssue>(AccessIssue.RegisteredAccountRequired);
            }

            if (message == null || message.Context == null)
            {
                throw new FaultException<ChatIssue>(ChatIssue.EmptyMessage);
            }

            try
            {
                // registeredMessage = controller.SendMessage(player, message);
                //List<Player> recipients = controller.GetRecipients(registeredMessage.Context);
                //publisher.PublishMessageReceived(recipients, registeredMessage);
                return registeredMessage;
            }
            catch (ChatRuleException ex)
            {
                throw new FaultException<ChatIssue>(ex.Issue);
            }
        }
        */


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
