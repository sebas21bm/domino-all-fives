using System.Collections.Generic;
using System;
using System.Data.SqlClient;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.ServiceModel;
using DominoAllFives.Server.Controllers;
using DominoAllFives.Server.Exceptions;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using DominoAllFives.Server.Logging;

namespace DominoAllFives.Server
{
    /// <summary>
    /// Server boundary for the remote use cases.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
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
}
