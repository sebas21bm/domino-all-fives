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
using DominoAllFives.DataAccess.Models;
using DominoAllFives.Server.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DominoAllFives.Server
{
    /// <summary>
    /// Server boundary for the remote use cases.
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Reentrant,
        IncludeExceptionDetailInFaults = false)]
    public partial class DominoService : ISessionAccountService
    {
        private readonly Func<Credentials, PlayerAccount> _authenticate;
        private readonly ILogger<DominoService> _logger;
        public DominoService() : this(
            new AuthenticationController().LoginWithAccount,
            new ConsoleLogger<DominoService>())
        {
        }

        internal DominoService(
            Func<Credentials, PlayerAccount> authenticate,
            ILogger<DominoService> logger)
        {
            _authenticate = authenticate ?? throw new ArgumentNullException(nameof(authenticate));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                PlayerAccount authenticatedAccount = _authenticate(credentials);
                string sessionKey = SessionManager.CreateSession(authenticatedAccount.PlayerId);
                authenticatedAccount.SetSessionKey(sessionKey);
                return authenticatedAccount;
            }
            catch (AccountRuleException ex)
            {
                _logger.LogWarning(ex, "Login rejected. Account issue: {AccountIssue}.", ex.Issue);
                throw new FaultException<AccountIssue>(
                    ex.Issue,
                    new FaultReason(
                    "The account cannot sign in."));
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Login failed on server.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (EntityException ex)
            {
                _logger.LogError(ex, "Login failed on server.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Login failed on server.");
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
                ProfilePictureStorage storage =
                    new ProfilePictureStorage(
                    NullLogger<ProfilePictureStorage>.Instance);
                return new RankingController(storage).GetTopWinsRanking();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Ranking failed on server.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (EntityException ex)
            {
                _logger.LogError(ex, "Ranking failed on server.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ranking failed on server.");
                throw new FaultException("The service could not complete the operation.");
            }
        }

        /// <summary>Gets the wins ranking position for a player.</summary>
        /// <param name = "playerId">The target player identifier.</param>
        /// <returns>The player ranking position.</returns>
        /// <exception cref = "FaultException">The request is invalid or persistence failed.</exception>
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
                ProfilePictureStorage storage =
                    new ProfilePictureStorage(
                    NullLogger<ProfilePictureStorage>.Instance);
                return new RankingController(storage).GetWinsRankingPositionById(playerId);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Ranking failed on server.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (EntityException ex)
            {
                _logger.LogError(ex, "Ranking failed on server.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ranking failed on server.");
                throw new FaultException("The service could not complete the operation.");
            }
        }
    }
}
