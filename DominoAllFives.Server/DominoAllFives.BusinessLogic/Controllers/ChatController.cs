using System;
using System.Collections.Generic;

using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using Player = DominoAllFives.Contracts.Domain.Player;
using StoredPlayer = DominoAllFives.DataAccess.Models.Player;

namespace DominoAllFives.BusinessLogic.Controllers
{
    public sealed class ChatController
    {
        private const int MaximumMessageLength = 150;

        private readonly LobbyManager _lobbyManager;

        public ChatController(LobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager
                ?? throw new ArgumentNullException(nameof(lobbyManager));
        }

        public Player GetAuthenticatedPlayer(string sessionKey)
        {
            int playerId = SessionManager.GetPlayerId(sessionKey);
            using (DominoAllFivesEntities context = new DominoAllFivesEntities())
            {
                IPlayerRepository playerRepository = new PlayerRepository(context);
                StoredPlayer storedPlayer = playerRepository.GetById(playerId);
                if (storedPlayer == null || storedPlayer.IsDeleted)
                {
                    SessionManager.RemoveSession(sessionKey);
                    throw new KeyNotFoundException();
                }

                return new Player
                {
                    PlayerId = storedPlayer.IdPlayer,
                    Username = storedPlayer.Username,
                    IsGuest = storedPlayer.IsGuest
                };
            }
        }

        public List<int> SendMessage(Player sender, ChatMessage message)
        {
            ValidatePlayer(sender);
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.ContextId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(message));
            }

            if (sender.IsGuest)
            {
                throw new UnauthorizedAccessException();
            }

            List<int> recipientIds;
            switch (message.Scope)
            {
                case ChatScope.Lobby:
                    recipientIds = _lobbyManager.GetChatRecipientIds(
                        message.ContextId,
                        sender.PlayerId);
                    break;
                case ChatScope.Match:
                    throw new ChatRuleException(ChatIssue.PlayerNotInChat);
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }

            string normalizedContent = ValidateAndNormalizeContent(message.Content);
            message.SenderId = sender.PlayerId;
            message.SenderUsername = sender.Username;
            message.Content = normalizedContent;
            return recipientIds;
        }

        private static void ValidatePlayer(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (player.PlayerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(player));
            }

            if (string.IsNullOrWhiteSpace(player.Username))
            {
                throw new ArgumentException(
                    "The player username cannot be empty.",
                    nameof(player));
            }
        }

        private static string ValidateAndNormalizeContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ChatRuleException(ChatIssue.EmptyMessage);
            }

            string trimmedContent = content.Trim();
            if (trimmedContent.Length > MaximumMessageLength)
            {
                throw new ChatRuleException(ChatIssue.MessageTooLong);
            }

            return trimmedContent;
        }
    }
}