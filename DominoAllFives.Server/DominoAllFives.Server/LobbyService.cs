using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Net.Mail;
using System.ServiceModel;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;
using DominoAllFives.Server.Infrastructure.Lobby;
using DominoAllFives.Server.Logging;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant,
        IncludeExceptionDetailInFaults = false)]
    public sealed class LobbyService : ILobbyService
    {
        private const int MaximumEmailLength = 254;
        private static readonly TimeSpan invitationInterval = TimeSpan.FromSeconds(5);
        private static readonly LobbyController controller;
        private static readonly MatchController matchController;
        private static readonly LobbyCallbackRegistry callbacks;
        private static readonly LobbyEventPublisher publisher;
        private static readonly ILogger<LobbyService> logger;
        private static readonly ProfilePictureStorage pictureStorage;
        private Player _player;
        private ILobbyCallback _callback;
        private ICommunicationObject _callbackChannel;
        private int _invitationLobbyId;
        private readonly HashSet<string> _invitedAddresses =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private DateTime _lastInvitationAttemptUtc;

        static LobbyService()
        {
            LobbyManager lobbyManager = LobbyManager.Instance;
            controller = new LobbyController(lobbyManager);
            matchController = new MatchController(MatchManager.Instance);
            callbacks = new LobbyCallbackRegistry();
            publisher = new LobbyEventPublisher(
                callbacks,
                LoggerFactoryProvider.CreateLogger<LobbyEventPublisher>());
            logger = LoggerFactoryProvider.CreateLogger<LobbyService>();
            pictureStorage = new ProfilePictureStorage(
                LoggerFactoryProvider.CreateLogger<ProfilePictureStorage>());
        }

        public void OpenLobbySession(string sessionKey)
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
                _player = controller.GetAuthenticatedPlayer(sessionKey, pictureStorage);
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogDebug(ex, "The lobby session could not resolve the player.");
                throw new FaultException<AccessIssue>(AccessIssue.SessionRequired);
            }
            catch (SqlException ex)
            {
                logger.LogError(ex, "Lobby session could not resolve player.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (EntityException ex)
            {
                logger.LogError(ex, "Lobby session could not resolve player.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Lobby session could not resolve player.");
                throw new FaultException("The service could not complete the operation.");
            }

            OperationContext operationContext = OperationContext.Current;
            ILobbyCallback callback = operationContext.GetCallbackChannel<ILobbyCallback>();
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

        public void CreateLobby(LobbySettings settings)
        {
            try
            {
                Player player = GetPlayer();
                Lobby lobby = controller.CreateLobby(player, settings);
                publisher.PublishLobbyChanged(lobby);
            }
            catch (LobbyRuleException ex)
            {
                throw new FaultException<LobbyIssue>(ex.Issue);
            }
        }

        public void JoinLobby(string lobbyCode)
        {
            try
            {
                Player player = GetPlayer();
                Lobby lobby = controller.JoinLobby(player, lobbyCode);
                publisher.PublishLobbyChanged(lobby);
            }
            catch (LobbyRuleException ex)
            {
                throw new FaultException<LobbyIssue>(ex.Issue);
            }
        }

        public void SendLobbyInvitation(string recipientEmail)
        {
            int playerId = GetPlayer().PlayerId;
            Lobby lobby;
            try
            {
                lobby = controller.GetLobbyForInvitation(playerId);
            }
            catch (LobbyRuleException ex)
            {
                throw new FaultException<LobbyIssue>(ex.Issue);
            }
            catch (UnauthorizedAccessException)
            {
                throw new FaultException<AccessIssue>(AccessIssue.PermissionDenied);
            }

            MailAddress recipient = ParseInvitationAddress(recipientEmail);
            if (_invitationLobbyId != lobby.LobbyId)
            {
                _invitationLobbyId = lobby.LobbyId;
                _invitedAddresses.Clear();
            }

            if (_invitedAddresses.Contains(recipient.Address))
            {
                return;
            }

            DateTime nowUtc = DateTime.UtcNow;
            if (nowUtc - _lastInvitationAttemptUtc < invitationInterval)
            {
                throw new FaultException("Please wait before sending another invitation.");
            }

            _lastInvitationAttemptUtc = nowUtc;
            try
            {
                LobbyInvitationEmail.Send(recipient, lobby.LobbyCode);
                _invitedAddresses.Add(recipient.Address);
            }
            catch (SmtpException ex)
            {
                logger.LogWarning("Gmail rejected a lobby invitation. SMTP status: {Status}.", ex.StatusCode);
                throw new FaultException("The invitation email could not be sent.");
            }
            catch (FormatException)
            {
                logger.LogWarning("The Gmail sender address is invalid.");
                throw new FaultException("The invitation email could not be sent.");
            }
            catch (InvalidOperationException)
            {
                logger.LogWarning("The Gmail sender could not be configured for lobby invitations.");
                throw new FaultException("The invitation email could not be sent.");
            }
        }

        private static MailAddress ParseInvitationAddress(string recipientEmail)
        {
            string address = recipientEmail?.Trim();
            if (!string.IsNullOrEmpty(address) && address.Length <= MaximumEmailLength)
            {
                try
                {
                    MailAddress recipient = new MailAddress(address);
                    if (string.Equals(recipient.Address, address, StringComparison.Ordinal))
                    {
                        return recipient;
                    }
                }
                catch (FormatException)
                {
                    throw new FaultException("Enter a valid recipient email address.");
                }
            }

            throw new FaultException("Enter a valid recipient email address.");
        }

        public void LeaveLobby()
        {
            try
            {
                Player player = GetPlayer();
                Lobby lobby = controller.LeaveLobby(player.PlayerId);
                if (lobby.State == LobbyState.Closed)
                {
                    publisher.PublishLobbyClosed(lobby);
                }
                else
                {
                    publisher.PublishLobbyChanged(lobby);
                }
            }
            catch (LobbyRuleException ex)
            {
                throw new FaultException<LobbyIssue>(ex.Issue);
            }
        }

        public Lobby GetLobby(string lobbyCode)
        {
            GetPlayer();
            try
            {
                return controller.GetLobby(lobbyCode);
            }
            catch (LobbyRuleException ex)
            {
                throw new FaultException<LobbyIssue>(ex.Issue);
            }
        }

        public void StartMatch()
        {
            Player player = GetPlayer();
            Lobby startingLobby = null;
            bool matchCreated = false;
            try
            {
                startingLobby = controller.PrepareLobbyForMatch(player.PlayerId);
                List<int> playerIds = new List<int>();
                foreach (Player participant in startingLobby.Participants)
                {
                    playerIds.Add(participant.PlayerId);
                }

                matchController.CreateMatch(
                    startingLobby.LobbyId,
                    playerIds,
                    startingLobby.Settings.TargetPoints);
                matchCreated = true;

                Lobby lobby = controller.ConfirmMatchStarted(player.PlayerId);
                publisher.PublishLobbyChanged(lobby);
                publisher.PublishMatchStarted(lobby, lobby.LobbyId);
            }
            catch (LobbyRuleException ex)
            {
                throw new FaultException<LobbyIssue>(ex.Issue);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogDebug(ex, "The player was denied permission to start the match.");
                throw new FaultException<AccessIssue>(AccessIssue.PermissionDenied);
            }
            finally
            {
                if (startingLobby != null && !matchCreated)
                {
                    controller.RestoreLobbyAfterMatchStartFailure(player.PlayerId);
                }
            }
        }

        public void CloseLobby()
        {
            try
            {
                Player player = GetPlayer();
                Lobby lobby = controller.CloseLobby(player.PlayerId);
                publisher.PublishLobbyClosed(lobby);
            }
            catch (LobbyRuleException ex)
            {
                throw new FaultException<LobbyIssue>(ex.Issue);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogDebug(ex, "The player was denied permission to close the lobby.");
                throw new FaultException<AccessIssue>(AccessIssue.PermissionDenied);
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
