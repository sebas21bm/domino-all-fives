using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;
using DominoAllFives.Server.Controllers;
using DominoAllFives.Server.Exceptions;
using DominoAllFives.Server.Infrastructure;
using DominoAllFives.Server.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.ServiceModel;

namespace DominoAllFives.Server
{
    /// <summary>
    /// Server boundary for the remote use cases.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Reentrant,
        IncludeExceptionDetailInFaults = false)]
    public partial class DominoService : ISessionAccount
    {
        private readonly Func<Credentials, PlayerAccount> _authenticate;
        private readonly ILogger<DominoService> _logger;

        public DominoService()
            : this(new AuthenticationController().LoginWithAccount, new ConsoleLogger<DominoService>())
        {
        }

        internal DominoService(Func<Credentials, PlayerAccount> authenticate, ILogger<DominoService> logger)
        {
            _authenticate = authenticate ?? throw new ArgumentNullException(nameof(authenticate));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Authenticates an account using its identifier and password.</summary>
        /// <param name="credentials">Credentials supplied by the player.</param>
        /// <returns>The authenticated account.</returns>
        /// <exception cref="FaultException{AccountIssue}">The account cannot sign in.</exception>
        /// <exception cref="FaultException">Persistence failed.</exception>
        public PlayerAccount LoginWithAccount(Credentials credentials)
        {
            try
            {
                return _authenticate(credentials);
            }
            catch (AccountRuleException ex)
            {
                _logger.LogWarning(ex, "Login rejected. Account issue: {AccountIssue}.", ex.Issue);
                throw new FaultException<AccountIssue>(
                    ex.Issue,
                    new FaultReason("The account cannot sign in."));
            }
            catch (Exception ex) when (
                ex is SqlException ||
                ex is EntityException ||
                ex is DbUpdateException)
            {
                _logger.LogError(ex, "Login failed on server.");

                throw new FaultException(
                    "The service could not complete the operation.");
            }
        }
    }

    public partial class DominoService :  IRankingService
    {
        /// <summary>Gets the accounts with the most wins.</summary>
        /// <returns>The ranking entries.</returns>
        /// <exception cref="FaultException">Persistence failed.</exception>
        public List<RankingEntry> GetTopWinsRanking()
        {
            try
            {
                ProfilePictureStorage storage = new ProfilePictureStorage(
                    NullLogger<ProfilePictureStorage>.Instance);
                return new RankingController(storage).GetTopWinsRanking();
            }
            catch (Exception ex) when (
                            ex is SqlException ||
                            ex is EntityException ||
                            ex is DbUpdateException)
            {
                _logger.LogError(ex, "Ranking failed on server.");
                throw new FaultException("The service could not complete the operation.");
            }
        }

        /// <summary>Gets the wins ranking position for a player.</summary>
        /// <param name="playerId">The target player identifier.</param>
        /// <returns>The player ranking position.</returns>
        /// <exception cref="FaultException">The request is invalid or persistence failed.</exception>
        public int GetWinsRankingPositionById(int playerId)
        {
            if (playerId <= 0)
            {
                FaultException fault = new FaultException("Invalid request.");
                _logger.LogWarning(fault, "Ranking rejected an invalid player identifier.");
                throw fault;
            }

            try
            {
                ProfilePictureStorage storage = new ProfilePictureStorage(
                    NullLogger<ProfilePictureStorage>.Instance);
                return new RankingController(storage).GetWinsRankingPositionById(playerId);
            }
            catch (Exception ex) when (
                            ex is SqlException ||
                            ex is EntityException ||
                            ex is DbUpdateException)
            {
                _logger.LogError(ex, "Ranking failed on server.");
                throw new FaultException("The service could not complete the operation.");
            }
        }
    }

    public partial class DominoService : ILobbyService
    {
        private static readonly LobbyController LobbyController = new LobbyController();
        private static readonly CallbackRegistry<ILobbyCallback> LobbyCallbacks =
            new CallbackRegistry<ILobbyCallback>();
        private static readonly PresenceManager Presence = new PresenceManager();
        private static readonly GameEventPublisher Publisher =
            new GameEventPublisher(LobbyCallbacks);

        public Lobby CreateLobby(LobbySettings settings)
        {
            string id = CurrentConnectionId();
            LobbyController.Change change = RunLobbyOperation(
                () => LobbyController.Create(id, settings));
            RegisterCallbackOrRollback(id);
            Publisher.LobbyChanged(change.Recipients, change.Lobby);
            return change.Lobby;
        }

        public Lobby JoinLobby(string lobbyCode)
        {
            string id = CurrentConnectionId();
            LobbyController.Change change = RunLobbyOperation(
                () => LobbyController.Join(id, lobbyCode));
            RegisterCallbackOrRollback(id);
            Publisher.LobbyChanged(change.Recipients, change.Lobby);
            return change.Lobby;
        }

        public Lobby GetLobby(string lobbyCode)
        {
            return RunLobbyOperation(() => LobbyController.Get(lobbyCode));
        }

        public void LeaveLobby()
        {
            string id = CurrentConnectionId();
            LobbyController.Change change = LobbyController.Leave(id);
            LobbyCallbacks.Unregister(id);
            Presence.Remove(id);
            PublishChange(change);
        }

        public void CloseLobby()
        {
            string id = CurrentConnectionId();
            LobbyController.Change change = RunLobbyOperation(
                () => LobbyController.Close(id));
            LobbyCallbacks.Unregister(id);
            Presence.Remove(id);
            PublishChange(change);
        }

        public void StartMatch()
        {
            string id = CurrentConnectionId();
            LobbyController.Change change = RunLobbyOperation(
                () => LobbyController.Start(id));
            Publisher.LobbyChanged(change.Recipients, change.Lobby);
            Publisher.MatchStarted(change.Recipients, change.MatchId);
            // A real Match has NOT been created. Do not navigate using this ID.
        }

        private static void PublishChange(LobbyController.Change change)
        {
            if (change == null) return;
            if (change.Closed) Publisher.LobbyClosed(change.Recipients);
            else Publisher.LobbyChanged(change.Recipients, change.Lobby);
        }

        private static void RegisterCallbackOrRollback(string connectionId)
        {
            try
            {
                RegisterCurrentCallback(connectionId);
                Presence.Set(connectionId, ConnectionPresence.InLobby);
            }
            catch
            {
                LobbyCallbacks.Unregister(connectionId);
                Presence.Remove(connectionId);
                PublishChange(LobbyController.Leave(connectionId));
                throw;
            }
        }

        private static void RegisterCurrentCallback(string connectionId)
        {
            ILobbyCallback callback = OperationContext.Current
                .GetCallbackChannel<ILobbyCallback>();
            ICommunicationObject channel =
                (ICommunicationObject)OperationContext.Current.Channel;
            LobbyCallbacks.Register(connectionId, callback, channel);
        }

        private static string CurrentConnectionId()
        {
            OperationContext context = OperationContext.Current;
            string id = context == null ? null : context.SessionId;
            if (string.IsNullOrWhiteSpace(id))
                throw new FaultException("A WCF duplex session is required.");
            return id;
        }

        private static TResult RunLobbyOperation<TResult>(Func<TResult> operation)
        {
            try { return operation(); }
            catch (InvalidOperationException ex)
            {
                throw new FaultException(ex.Message);
            }
        }
    }

}
