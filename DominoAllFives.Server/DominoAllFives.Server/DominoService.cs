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

    public partial class DominoService : IRegistrationService
    {
        /// <inheritdoc/>
        public PlayerAccount Register(AccountRegistration registrationData)
        {
            if (registrationData == null)
            {
                _logger.LogDebug("Registration rejected a missing request.");
                throw new FaultException<RegistrationIssue>(
                    RegistrationIssue.InvalidRegistrationData);
            }

            try
            {
                RegistrationController controller = new RegistrationController(
                    LoggerFactoryProvider.CreateLogger<RegistrationController>());
                RegistrationResult result = controller.Register(registrationData);
                if (result.Status != RegistrationStatus.None)
                {
                    RegistrationIssue issue = GetRegistrationIssue(result.Status);
                    _logger.LogDebug("Registration rejected. Issue: {RegistrationIssue}.", issue);
                    throw new FaultException<RegistrationIssue>(issue);
                }

                PlayerAccount account = result.Account;
                account.SetSessionKey(SessionManager.CreateSession(account.PlayerId));
                return account;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Registration could not access persistence.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (EntityException ex)
            {
                _logger.LogError(ex, "Registration could not access persistence.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Registration could not access persistence.");
                throw new FaultException("The service could not complete the operation.");
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                _logger.LogError(ex, "Registration persistence validation failed.");
                throw new FaultException("The service could not complete the operation.");
            }
        }

        private static RegistrationIssue GetRegistrationIssue(RegistrationStatus status)
        {
            switch (status)
            {
                case RegistrationStatus.InvalidUsername:
                    return RegistrationIssue.InvalidUsername;
                case RegistrationStatus.InvalidEmail:
                    return RegistrationIssue.InvalidEmail;
                case RegistrationStatus.InvalidPassword:
                    return RegistrationIssue.InvalidPassword;
                case RegistrationStatus.UsernameAlreadyExists:
                    return RegistrationIssue.UsernameAlreadyExists;
                case RegistrationStatus.EmailAlreadyExists:
                    return RegistrationIssue.EmailAlreadyExists;
                default:
                    return RegistrationIssue.InvalidRegistrationData;
            }
        }
    }
}
