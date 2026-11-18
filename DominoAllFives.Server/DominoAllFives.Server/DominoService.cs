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
using DominoAllFives.Server.Logging;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server
{
    /// <summary>
    /// Server boundary for the remote use cases.
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Reentrant,
        IncludeExceptionDetailInFaults = false)]
    public partial class DominoService : ISessionService
    {
        private readonly ILogger<DominoService> _logger;

        public DominoService()
            : this(LoggerFactoryProvider.CreateLogger<DominoService>())
        {
        }

        internal DominoService(ILogger<DominoService> logger)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authenticates an account using its identifier and password.
        /// </summary>
        /// <param name = "credentials">
        /// Credentials supplied by the player.
        /// </param>
        /// <returns>The authenticated account.</returns>
        /// <exception cref = "FaultException{AccountIssue}">
        /// The account cannot sign in.
        /// </exception>
        /// <exception cref = "FaultException">
        /// Persistence failed.
        /// </exception>
        public PlayerAccount LoginWithAccount(Credentials credentials)
        {
            try
            {
                AuthenticationController controller = new AuthenticationController();
                PlayerAccount authenticatedAccount = controller.LoginWithAccount(credentials);

                string sessionKey = SessionManager.CreateSession(authenticatedAccount.PlayerId);
                authenticatedAccount.SetSessionKey(sessionKey);

                return authenticatedAccount;
            }
            catch (AccountRuleException ex)
            {
                _logger.LogDebug(ex, "Login rejected. Account issue: {AccountIssue}.", ex.Issue);
                throw new FaultException<AccountIssue>(ex.Issue);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Account login could not access persistence.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (EntityException ex)
            {
                _logger.LogError(ex, "Account login could not access persistence.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Account login could not access persistence.");
                throw new FaultException("The service could not complete the operation.");
            }
        }
    }

    public partial class DominoService : IRankingService
    {
        /// <summary>Gets the accounts with the most wins.</summary>
        /// <returns>The ranking entries.</returns>
        /// <exception cref = "FaultException">Persistence failed.</exception>
        public List<RankingEntry> GetTopWinsRanking()
        {
            try
            {
                ProfilePictureStorage storage = new ProfilePictureStorage(
                    LoggerFactoryProvider.CreateLogger<ProfilePictureStorage>());
                return new RankingController(storage).GetTopWinsRanking();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Top wins ranking could not access persistence.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (EntityException ex)
            {
                _logger.LogError(ex, "Top wins ranking could not access persistence.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Top wins ranking could not access persistence.");
                throw new FaultException("The service could not complete the operation.");
            }
        }

        /// <summary>Gets the wins ranking position for a player.</summary>
        /// <param name = "playerId">The target player identifier.</param>
        /// <returns>The player ranking position.</returns>
        /// <exception cref = "FaultException">
        /// The request is invalid or persistence failed.
        /// </exception>
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
                    LoggerFactoryProvider.CreateLogger<ProfilePictureStorage>());
                return new RankingController(storage).GetWinsRankingPositionById(playerId);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Wins ranking position could not access persistence.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (EntityException ex)
            {
                _logger.LogError(ex, "Wins ranking position could not access persistence.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Wins ranking position could not access persistence.");
                throw new FaultException("The service could not complete the operation.");
            }
        }
    }
}
