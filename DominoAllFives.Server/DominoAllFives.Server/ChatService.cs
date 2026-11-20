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
                LoggerFactoryProvider.CreateLogger<ChatEventPublisher>());
            logger = LoggerFactoryProvider.CreateLogger<ChatService>();
        }

        /// <inheritdoc/>
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
                _player = controller.GetAuthenticatedPlayer(sessionKey);
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
            callbacks.RegisterCallback(_player.PlayerId, callback);

            _callback = callback;
            _callbackChannel = callback as ICommunicationObject;
            if (_callbackChannel != null)
            {
                _callbackChannel.Closed += OnCallbackEnded;
                _callbackChannel.Faulted += OnCallbackEnded;
            }
        }

        /// <inheritdoc/>
        public void SendMessage(ChatMessage message)
        {
            Player player = GetPlayer();
            try
            {
                List<int> recipientIds = controller.SendMessage(player, message);
                publisher.PublishMessageReceived(recipientIds, message);
            }
            catch (ChatRuleException ex)
            {
                throw new FaultException<ChatIssue>(ex.Issue);
            }
            catch (UnauthorizedAccessException)
            {
                throw new FaultException<AccessIssue>(AccessIssue.RegisteredAccountRequired);
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(
                    ex,
                    "Chat message request was null for player {PlayerId}.",
                    player.PlayerId);
                throw new FaultException("The service could not complete the operation.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                logger.LogError(
                    ex,
                    "Chat message request had an invalid context or scope for player {PlayerId}.",
                    player.PlayerId);
                throw new FaultException("The service could not complete the operation.");
            }
        }

        private Player GetPlayer()
        {
            if (_player == null)
            {
                throw new FaultException<AccessIssue>(AccessIssue.SessionRequired);
            }

            return _player;
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