using DominoAllFives.BusinessLogic.Controllers;
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
            new GameEventPublisher(LobbyCallbacks, new ConsoleLogger<GameEventPublisher>());

        public DominoService(ILogger<DominoService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Lobby CreateLobby(LobbySettings settings)
        {
            string connectionId = CurrentConnectionId();
            int playerId = GetPlayerIdFromConnection(connectionId);

            LobbyController.Change change = RunLobbyOperation(
                () => LobbyController.Create(connectionId, settings));

            RegisterCallbackOrRollback(connectionId, playerId);
            Publisher.LobbyChanged(change.Recipients, change.Lobby);

            return change.Lobby;
        }

        public Lobby JoinLobby(string lobbyCode)
        {
            string connectionId = CurrentConnectionId();
            int playerId = GetPlayerIdFromConnection(connectionId);

            LobbyController.Change change = RunLobbyOperation(
                () => LobbyController.Join(connectionId, lobbyCode));

            RegisterCallbackOrRollback(connectionId, playerId);
            Publisher.LobbyChanged(change.Recipients, change.Lobby);

            return change.Lobby;
        }

        public Lobby GetLobby(string lobbyCode)
        {
            return RunLobbyOperation(() => LobbyController.Get(lobbyCode));
        }

        public void LeaveLobby()
        {
            string connectionId = CurrentConnectionId();
            int playerId = GetPlayerIdFromConnection(connectionId);

            LobbyController.Change change = LobbyController.Leave(connectionId);
            LobbyCallbacks.Unregister(connectionId);
            Presence.Remove(playerId);

            PublishChange(change);
        }

        public void CloseLobby()
        {
            string connectionId = CurrentConnectionId();
            int playerId = GetPlayerIdFromConnection(connectionId);

            LobbyController.Change change = RunLobbyOperation(
                () => LobbyController.Close(connectionId));

            LobbyCallbacks.Unregister(connectionId);
            Presence.Remove(playerId);

            PublishChange(change);
        }

        public void StartMatch()
        {
            string connectionId = CurrentConnectionId();
            LobbyController.Change change = RunLobbyOperation(
                () => LobbyController.Start(connectionId));

            Publisher.LobbyChanged(change.Recipients, change.Lobby);
            Publisher.MatchStarted(change.Recipients, change.MatchId);
        }

        private static void PublishChange(LobbyController.Change change)
        {
            if (change == null)
            {
                return;
            }

            if (change.Closed)
            {
                Publisher.LobbyClosed(change.Recipients);
            }
            else
            {
                Publisher.LobbyChanged(change.Recipients, change.Lobby);
            }
        }

        private static void RegisterCallbackOrRollback(string connectionId, int playerId)
        {
            try
            {
                RegisterCurrentCallback(connectionId);
                Presence.SetPresence(playerId, PlayerPresence.InGame);
            }
            catch (Exception ex)
            {
                LobbyCallbacks.Unregister(connectionId);
                Presence.Remove(playerId);
                PublishChange(LobbyController.Leave(connectionId));
                throw new FaultException($"Failed to register lobby session: {ex.Message}");
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
            {
                throw new FaultException("A WCF duplex session is required.");
            }

            return id;
        }

        private int GetPlayerIdFromConnection(string connectionId)
        {
            if (int.TryParse(connectionId, out int parsedId))
            {
                return parsedId;
            }

            return 1;
        }

        private static TResult RunLobbyOperation<TResult>(Func<TResult> operation)
        {
            try
            {
                return operation();
            }
            catch (InvalidOperationException ex)
            {
                throw new FaultException(ex.Message);
            }
        }
    }

}
