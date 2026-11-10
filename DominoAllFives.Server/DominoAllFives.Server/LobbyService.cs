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
        private static readonly LobbyController Controller;
        private static readonly LobbyCallbackRegistry Callbacks;
        private static readonly LobbyEventPublisher Publisher;
        private static readonly ILogger<LobbyService> Logger;

        private Player _player;
        private ILobbyCallback _callback;
        private ICommunicationObject _callbackChannel;

        static LobbyService()
        {
            LobbyManager lobbyManager =
                new LobbyManager();

            Controller =
                new LobbyController(
                    lobbyManager);

            Callbacks =
                new LobbyCallbackRegistry();

            Publisher =
                new LobbyEventPublisher(
                    Callbacks,
                    new ConsoleLogger<LobbyEventPublisher>());

            Logger =
                new ConsoleLogger<LobbyService>();
        }

        public void OpenLobbySession(
            string sessionKey)
        {
            if (_player != null)
            {
                throw AccessFault(
                    AccessIssue.PermissionDenied);
            }

            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                throw AccessFault(
                    AccessIssue.SessionRequired);
            }

            try
            {
                _player =
                    SessionManager.GetPlayer(
                        sessionKey);
            }
            catch (KeyNotFoundException)
            {
                throw AccessFault(
                    AccessIssue.SessionRequired);
            }
            catch (Exception ex) when (
                ex is SqlException ||
                ex is EntityException ||
                ex is DbUpdateException)
            {
                Logger.LogError(
                    ex,
                    "Lobby session could not resolve player.");

                throw new FaultException(
                    "The service could not complete the operation.");
            }

            ILobbyCallback callback =
                OperationContext.Current
                    .GetCallbackChannel<ILobbyCallback>();

            bool callbackRegistered =
                Callbacks.RegisterCallback(
                    _player.PlayerId,
                    callback);

            if (!callbackRegistered)
            {
                _player = null;

                throw AccessFault(
                    AccessIssue.PermissionDenied);
            }

            _callback = callback;
            _callbackChannel =
                callback as ICommunicationObject;

            if (_callbackChannel != null)
            {
                _callbackChannel.Closed +=
                    OnCallbackEnded;

                _callbackChannel.Faulted +=
                    OnCallbackEnded;
            }
        }

        public Lobby CreateLobby(
            LobbySettings settings)
        {
            try
            {
                Player player =
                    GetPlayer();

                Lobby lobby =
                    Controller.CreateLobby(
                        player,
                        settings);

                Publisher.PublishLobbyChanged(
                    lobby);

                return lobby;
            }
            catch (LobbyRuleException ex)
            {
                throw LobbyFault(
                    ex.Issue);
            }
        }

        public Lobby JoinLobby(
            string lobbyCode)
        {
            try
            {
                Player player =
                    GetPlayer();

                Lobby lobby =
                    Controller.JoinLobby(
                        player,
                        lobbyCode);

                Publisher.PublishLobbyChanged(
                    lobby);

                return lobby;
            }
            catch (LobbyRuleException ex)
            {
                throw LobbyFault(
                    ex.Issue);
            }
        }

        public void LeaveLobby()
        {
            try
            {
                Player player =
                    GetPlayer();

                Lobby lobby =
                    Controller.LeaveLobby(
                        player.PlayerId);

                if (lobby.State == LobbyState.Closed)
                {
                    Publisher.PublishLobbyClosed(
                        lobby);
                }
                else
                {
                    Publisher.PublishLobbyChanged(
                        lobby);
                }
            }
            catch (LobbyRuleException ex)
            {
                throw LobbyFault(
                    ex.Issue);
            }
        }

        public Lobby GetLobby(
            string lobbyCode)
        {
            GetPlayer();

            try
            {
                return Controller.GetLobby(
                    lobbyCode);
            }
            catch (LobbyRuleException ex)
            {
                throw LobbyFault(
                    ex.Issue);
            }
        }

        public void StartMatch()
        {
            try
            {
                Player player =
                    GetPlayer();

                Lobby lobby =
                    Controller.StartLobby(
                        player.PlayerId);

                Publisher.PublishLobbyChanged(
                    lobby);
            }
            catch (LobbyRuleException ex)
            {
                throw LobbyFault(
                    ex.Issue);
            }
            catch (UnauthorizedAccessException)
            {
                throw AccessFault(
                    AccessIssue.PermissionDenied);
            }
        }

        public void CloseLobby()
        {
            try
            {
                Player player =
                    GetPlayer();

                Lobby lobby =
                    Controller.CloseLobby(
                        player.PlayerId);

                Publisher.PublishLobbyClosed(
                    lobby);
            }
            catch (LobbyRuleException ex)
            {
                throw LobbyFault(
                    ex.Issue);
            }
            catch (UnauthorizedAccessException)
            {
                throw AccessFault(
                    AccessIssue.PermissionDenied);
            }
        }

        private Player GetPlayer()
        {
            if (_player == null)
            {
                throw AccessFault(
                    AccessIssue.SessionRequired);
            }

            return _player;
        }

        private static FaultException<LobbyIssue> LobbyFault(
            LobbyIssue issue)
        {
            return new FaultException<LobbyIssue>(
                issue);
        }

        private static FaultException<AccessIssue> AccessFault(
            AccessIssue issue)
        {
            return new FaultException<AccessIssue>(
                issue);
        }

        private void OnCallbackEnded(
            object sender,
            EventArgs eventArgs)
        {
            if (_player != null &&
                _callback != null)
            {
                Callbacks.RemoveCallback(
                    _player.PlayerId,
                    _callback);
            }

            if (_callbackChannel != null)
            {
                _callbackChannel.Closed -=
                    OnCallbackEnded;

                _callbackChannel.Faulted -=
                    OnCallbackEnded;
            }

            _callback = null;
            _callbackChannel = null;
        }
    }
}