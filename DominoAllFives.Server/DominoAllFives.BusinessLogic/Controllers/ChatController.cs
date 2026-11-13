using System;
using System.Collections.Generic;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.BusinessLogic.Controllers
{
    public sealed class ChatController
    {
        private const int MaximumMessageLength = 150;
        private readonly LobbyManager _lobbyManager;

        public ChatController(LobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager ?? throw new ArgumentNullException(nameof(lobbyManager));
        }

        public ChatMessage SendMessage(
            Player sender,
            ChatMessage message,
            out List<Player> recipients)
        {
            ValidatePlayer(sender);
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            ValidateContext(message.Context);
            string normalizedContent = ValidateAndNormalizeContent(message.Content);
            switch (message.Context.Scope)
            {
                case ChatScope.Lobby:
                    return _lobbyManager.RecordChatMessage(
                        sender,
                        message.Context.ContextId,
                        normalizedContent,
                        out recipients);
                case ChatScope.Match:
                    throw new ChatRuleException(ChatIssue.PlayerNotInChat);
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }
        }

        public List<ChatMessage> GetMessages(int playerId, ChatContext context)
        {
            ValidateId(playerId, nameof(playerId));
            ValidateContext(context);
            switch (context.Scope)
            {
                case ChatScope.Lobby:
                    return _lobbyManager.GetChatMessages(playerId, context.ContextId);
                case ChatScope.Match:
                    throw new ChatRuleException(ChatIssue.PlayerNotInChat);
                default:
                    throw new ArgumentOutOfRangeException(nameof(context));
            }
        }

        private static void ValidatePlayer(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            ValidateId(player.PlayerId, nameof(player));
            if (string.IsNullOrWhiteSpace(player.Username))
            {
                throw new ArgumentException(null, nameof(player));
            }
        }

        private static void ValidateContext(ChatContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!Enum.IsDefined(typeof(ChatScope), context.Scope))
            {
                throw new ArgumentOutOfRangeException(nameof(context));
            }

            ValidateId(context.ContextId, nameof(context));
        }

        private static void ValidateId(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static string ValidateAndNormalizeContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ChatRuleException(ChatIssue.EmptyMessage);
            }

            string trimmed = content.Trim();
            if (trimmed.Length > MaximumMessageLength)
            {
                throw new ChatRuleException(ChatIssue.MessageTooLong);
            }

            return trimmed;
        }
    }
}