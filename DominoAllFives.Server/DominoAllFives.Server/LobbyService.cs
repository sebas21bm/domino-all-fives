using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using DominoAllFives.BusinessLogic.Controllers;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Managers;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;
using DominoAllFives.Server.Infrastructure.Lobby;
using DominoAllFives.Server.Logging;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server
{
    /// <summary>
    /// Coordinates authenticated lobby operations and publishes participant callbacks.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode =
        ConcurrencyMode.Reentrant, IncludeExceptionDetailInFaults = false)]
    public sealed class LobbyService : ILobbyService
    {
        private static readonly LobbyController controller;
        private static readonly LobbyCallbackRegistry callbacks;
        private static readonly LobbyEventPublisher publisher;
        private static readonly ILogger<LobbyService> logger;
        private Player _player;
        private ILobbyCallback _callback;
        private ICommunicationObject _callbackChannel;
        static LobbyService()
        {
            LobbyManager lobbyManager = new LobbyManager();
            Infrastructure.GameRuntime.Publisher.MatchEnded = lobbyManager.ReleaseMatch;
            controller = new LobbyController(lobbyManager);
            callbacks = new LobbyCallbackRegistry();
            publisher = new LobbyEventPublisher(callbacks,
                new ConsoleLogger<LobbyEventPublisher>());
            logger = new ConsoleLogger<LobbyService>();
        }

        /// <summary>
        /// Associates the duplex connection and callback with an authenticated player.
        /// </summary>
        /// <param name = "sessionKey">The authentication token for the player session.</param>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref = "FaultException">
        /// The server cannot resolve the player session because persistence failed.
        /// </exception>
        public void OpenLobbySession(string sessionKey)
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

            ILobbyCallback callback = OperationContext.Current.GetCallbackChannel<ILobbyCallback>();
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
        /// Creates a lobby owned by the authenticated player.
        /// </summary>
        /// <param name = "settings">The requested lobby configuration.</param>
        /// <returns>The created lobby snapshot.</returns>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref = "FaultException{LobbyIssue}">
        /// The requested operation violates a lobby rule or the lobby is unavailable.
        /// </exception>
        public Lobby CreateLobby(LobbySettings settings)
        {
            try
            {
                Player player = GetPlayer();
                Lobby lobby = controller.CreateLobby(player, settings);
                publisher.PublishLobbyChanged(lobby);
                return lobby;
            }
            catch (LobbyRuleException ex)
            {
                throw LobbyFault(ex.Issue);
            }
        }

        /// <summary>
        /// Adds the authenticated player to an existing lobby.
        /// </summary>
        /// <param name = "lobbyCode">The code identifying the target lobby.</param>
        /// <returns>The lobby snapshot after the player joins.</returns>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref = "FaultException{LobbyIssue}">
        /// The requested operation violates a lobby rule or the lobby is unavailable.
        /// </exception>
        public Lobby JoinLobby(string lobbyCode)
        {
            try
            {
                Player player = GetPlayer();
                Lobby lobby = controller.JoinLobby(player, lobbyCode);
                publisher.PublishLobbyChanged(lobby);
                return lobby;
            }
            catch (LobbyRuleException ex)
            {
                throw LobbyFault(ex.Issue);
            }
        }

        /// <summary>
        /// Removes the authenticated player from the current lobby.
        /// </summary>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref = "FaultException{LobbyIssue}">
        /// The requested operation violates a lobby rule or the lobby is unavailable.
        /// </exception>
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
                throw LobbyFault(ex.Issue);
            }
        }

        /// <summary>
        /// Retrieves a lobby snapshot for an authenticated connection.
        /// </summary>
        /// <param name = "lobbyCode">The code identifying the requested lobby.</param>
        /// <returns>The current lobby snapshot.</returns>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref = "FaultException{LobbyIssue}">
        /// The requested operation violates a lobby rule or the lobby is unavailable.
        /// </exception>
        public Lobby GetLobby(string lobbyCode)
        {
            GetPlayer();
            try
            {
                return controller.GetLobby(lobbyCode);
            }
            catch (LobbyRuleException ex)
            {
                throw LobbyFault(ex.Issue);
            }
        }

        /// <summary>
        /// Starts the current lobby when the authenticated player is its owner.
        /// </summary>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref = "FaultException{LobbyIssue}">
        /// The requested operation violates a lobby rule or the lobby is unavailable.
        /// </exception>
        public void StartMatch()
        {
            try
            {
                Player player = GetPlayer();
                Lobby lobby = controller.StartLobby(player.PlayerId, starting
                    => Infrastructure.GameRuntime.Controller.Create(starting.LobbyId,
                    starting.Participants.Select(participant => participant.PlayerId),
                    starting.Settings.TargetPoints));
                publisher.PublishLobbyChanged(lobby);
                publisher.PublishMatchStarted(lobby, lobby.LobbyId);
            }
            catch (LobbyRuleException ex)
            {
                throw LobbyFault(ex.Issue);
            }
            catch (UnauthorizedAccessException)
            {
                throw AccessFault(AccessIssue.PermissionDenied);
            }
        }

        /// <summary>
        /// Closes the current lobby when the authenticated player is its owner.
        /// </summary>
        /// <exception cref = "FaultException{AccessIssue}">
        /// The connection has no authenticated session or the player lacks permission.
        /// </exception>
        /// <exception cref = "FaultException{LobbyIssue}">
        /// The requested operation violates a lobby rule or the lobby is unavailable.
        /// </exception>
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
                throw LobbyFault(ex.Issue);
            }
            catch (UnauthorizedAccessException)
            {
                throw AccessFault(AccessIssue.PermissionDenied);
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

        private static FaultException<LobbyIssue> LobbyFault(LobbyIssue issue)
        {
            return new FaultException<LobbyIssue>(issue);
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
