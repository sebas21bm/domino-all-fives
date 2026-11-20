using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
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
        private const string ServiceFailureReason = "The service could not complete the operation.";

        private readonly ILogger<DominoService> _logger;

        public DominoService()
            : this(LoggerFactoryProvider.CreateLogger<DominoService>())
        {
        }

        internal DominoService(ILogger<DominoService> logger)
        {
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
                throw new FaultException(ServiceFailureReason);
            }
            catch (EntityException ex)
            {
                _logger.LogError(ex, "Account login could not access persistence.");
                throw new FaultException(ServiceFailureReason);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Account login could not access persistence.");
                throw new FaultException(ServiceFailureReason);
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
                throw new FaultException(ServiceFailureReason);
            }
            catch (EntityException ex)
            {
                _logger.LogError(ex, "Top wins ranking could not access persistence.");
                throw new FaultException(ServiceFailureReason);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Top wins ranking could not access persistence.");
                throw new FaultException(ServiceFailureReason);
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
                throw new FaultException(ServiceFailureReason);
            }
            catch (EntityException ex)
            {
                _logger.LogError(ex, "Wins ranking position could not access persistence.");
                throw new FaultException(ServiceFailureReason);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Wins ranking position could not access persistence.");
                throw new FaultException(ServiceFailureReason);
            }
        }
    }

    public partial class DominoService : IRegistrationService
    {
        /// <inheritdoc/>
        public PlayerAccount Register(AccountRegistration registrationData)
        {
            try
            {
                RegistrationController controller = new RegistrationController();
                PlayerAccount account = controller.Register(registrationData);
                account.SetSessionKey(SessionManager.CreateSession(account.PlayerId));
                return account;
            }
            catch (RegistrationRuleException ex)
            {
                _logger.LogDebug(
                    ex,
                    "Registration rejected. Registration issue: {RegistrationIssue}.",
                    ex.Issue);
                throw new FaultException<RegistrationIssue>(ex.Issue);
            }
            catch (ArgumentNullException ex) when (ex.ParamName == nameof(registrationData))
            {
                _logger.LogDebug(ex, "Registration rejected an incomplete request.");
                throw new FaultException<RegistrationIssue>(
                    RegistrationIssue.InvalidRegistrationData);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Registration could not access persistence.");
                throw new FaultException(ServiceFailureReason);
            }
            catch (EntityException ex)
            {
                _logger.LogError(ex, "Registration could not access persistence.");
                throw new FaultException(ServiceFailureReason);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Registration could not access persistence.");
                throw new FaultException(ServiceFailureReason);
            }
            catch (DbEntityValidationException ex)
            {
                _logger.LogError(ex, "Registration failed persistence validation.");
                throw new FaultException(ServiceFailureReason);
            }
        }
    }

    public partial class DominoService : IAccountService
    {
        /// <inheritdoc/>
        public void ChangePreferredLanguage(string sessionKey, string preferredLanguageId)
        {
            const string persistenceFailureMessage =
                "Preferred language change could not complete persistence.";
            int playerId = GetSessionPlayerId(sessionKey);
            try
            {
                AccountLanguageController controller = new AccountLanguageController(
                    LoggerFactoryProvider.CreateLogger<AccountLanguageController>());
                controller.ChangePreferredLanguage(playerId, preferredLanguageId);
            }
            catch (AccountRuleException ex)
            {
                _logger.LogDebug(
                    ex, "Preferred language change rejected. Account issue: {AccountIssue}.",
                    ex.Issue);
                throw new FaultException<AccountIssue>(ex.Issue);
            }
            catch (SqlException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (EntityException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbUpdateException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbEntityValidationException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Preferred language change could not complete persistence.");
                throw new FaultException(ServiceFailureReason);
            }
        }

        /// <inheritdoc/>
        public void ChangePassword(string sessionKey, string currentPassword, string newPassword)
        {
            const string persistenceFailureMessage =
                "Password change could not complete persistence.";
            int playerId = GetSessionPlayerId(sessionKey);
            try
            {
                AccountController controller = new AccountController(
                    LoggerFactoryProvider.CreateLogger<AccountController>());
                controller.ChangePassword(playerId, currentPassword, newPassword);
            }
            catch (AccountRuleException ex)
            {
                _logger.LogDebug(
                    ex, "Password change rejected. Account issue: {AccountIssue}.", ex.Issue);
                throw new FaultException<AccountIssue>(ex.Issue);
            }
            catch (SqlException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (EntityException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbUpdateException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbEntityValidationException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
        }

        /// <inheritdoc/>
        public void DeleteAccount(string sessionKey, string password)
        {
            const string persistenceFailureMessage =
                "Account deletion could not complete persistence.";
            int playerId = GetSessionPlayerId(sessionKey);
            try
            {
                AccountController controller = new AccountController(
                    LoggerFactoryProvider.CreateLogger<AccountController>());
                controller.DeleteAccount(playerId, password);
            }
            catch (AccountRuleException ex)
            {
                _logger.LogDebug(
                    ex, "Account deletion rejected. Account issue: {AccountIssue}.", ex.Issue);
                throw new FaultException<AccountIssue>(ex.Issue);
            }
            catch (SqlException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (EntityException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbUpdateException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbEntityValidationException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
        }
    }

    public partial class DominoService : IProfileService
    {
        /// <inheritdoc/>
        public PlayerProfile GetMyProfile(string sessionKey)
        {
            const string persistenceFailureMessage =
                "Profile retrieval could not access persistence.";
            int playerId = GetSessionPlayerId(sessionKey);
            try
            {
                ProfilePictureStorage storage = new ProfilePictureStorage(
                    LoggerFactoryProvider.CreateLogger<ProfilePictureStorage>());
                ProfileController controller = new ProfileController(storage);
                return controller.GetProfile(playerId);
            }
            catch (ProfileRuleException ex)
            {
                _logger.LogDebug(
                    ex,
                    "Profile retrieval rejected. Profile issue: {ProfileIssue}.",
                    ex.Issue);
                throw new FaultException<ProfileIssue>(ex.Issue);
            }
            catch (SqlException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (EntityException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbUpdateException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Profile retrieval could not reconstruct the stored profile.");
                throw new FaultException(ServiceFailureReason);
            }
        }

        /// <inheritdoc/>
        public void UpdateProfileName(string sessionKey, string username)
        {
            const string persistenceFailureMessage = "Profile update could not access persistence.";
            int playerId = GetSessionPlayerId(sessionKey);
            try
            {
                ProfilePictureStorage storage = new ProfilePictureStorage(
                    LoggerFactoryProvider.CreateLogger<ProfilePictureStorage>());
                ProfileController controller = new ProfileController(storage);
                controller.UpdateProfileName(playerId, username);
            }
            catch (ProfileRuleException ex)
            {
                _logger.LogDebug(
                    ex,
                    "Profile update rejected. Profile issue: {ProfileIssue}.",
                    ex.Issue);
                throw new FaultException<ProfileIssue>(ex.Issue);
            }
            catch (SqlException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (EntityException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbUpdateException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbEntityValidationException ex)
            {
                throw CreatePersistenceFault(ex, "Profile update failed persistence validation.");
            }
        }

        /// <inheritdoc/>
        public void UpdateProfileWithPicture(
            string sessionKey,
            string username,
            ProfilePicture picture)
        {
            const string persistenceFailureMessage =
                "Profile and picture update could not access persistence.";
            int playerId = GetSessionPlayerId(sessionKey);
            if (picture == null)
            {
                _logger.LogDebug("Profile and picture update rejected an incomplete request.");
                throw new FaultException<ProfileIssue>(ProfileIssue.InvalidProfilePicture);
            }

            try
            {
                ProfilePictureStorage storage = new ProfilePictureStorage(
                    LoggerFactoryProvider.CreateLogger<ProfilePictureStorage>());
                ProfileController controller = new ProfileController(storage);
                controller.UpdateProfileWithPicture(playerId, username, picture);
            }
            catch (ProfileRuleException ex)
            {
                _logger.LogDebug(
                    ex,
                    "Profile and picture update rejected. Profile issue: {ProfileIssue}.",
                    ex.Issue);
                throw new FaultException<ProfileIssue>(ex.Issue);
            }
            catch (SqlException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (EntityException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbUpdateException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbEntityValidationException ex)
            {
                throw CreatePersistenceFault(
                    ex,
                    "Profile and picture update failed persistence validation.");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Profile update could not write picture storage.");
                throw new FaultException(ServiceFailureReason);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Profile update could not write picture storage.");
                throw new FaultException(ServiceFailureReason);
            }
        }

        /// <inheritdoc/>
        public void SetProfilePicture(string sessionKey, ProfilePicture picture)
        {
            const string persistenceFailureMessage =
                "Picture replacement could not access persistence.";
            int playerId = GetSessionPlayerId(sessionKey);
            try
            {
                ProfilePictureStorage storage = new ProfilePictureStorage(
                    LoggerFactoryProvider.CreateLogger<ProfilePictureStorage>());
                ProfilePictureController controller = new ProfilePictureController(storage);
                controller.SetProfilePicture(playerId, picture);
            }
            catch (ProfileRuleException ex)
            {
                _logger.LogDebug(
                    ex,
                    "Picture replacement rejected. Profile issue: {ProfileIssue}.",
                    ex.Issue);
                throw new FaultException<ProfileIssue>(ex.Issue);
            }
            catch (ArgumentNullException ex) when (ex.ParamName == nameof(picture))
            {
                _logger.LogDebug(ex, "Picture replacement rejected an incomplete request.");
                throw new FaultException<ProfileIssue>(
                    ProfileIssue.InvalidProfilePicture);
            }
            catch (SqlException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (EntityException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbUpdateException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbEntityValidationException ex)
            {
                throw CreatePersistenceFault(
                    ex,
                    "Picture replacement failed persistence validation.");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Picture replacement could not write picture storage.");
                throw new FaultException(ServiceFailureReason);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Picture replacement could not write picture storage.");
                throw new FaultException(ServiceFailureReason);
            }
        }

        /// <inheritdoc/>
        public void RemoveProfilePicture(string sessionKey)
        {
            const string persistenceFailureMessage =
                "Picture removal could not access persistence.";
            int playerId = GetSessionPlayerId(sessionKey);
            try
            {
                ProfilePictureStorage storage = new ProfilePictureStorage(
                    LoggerFactoryProvider.CreateLogger<ProfilePictureStorage>());
                ProfilePictureController controller = new ProfilePictureController(storage);
                controller.RemoveProfilePicture(playerId);
            }
            catch (ProfileRuleException ex)
            {
                _logger.LogDebug(
                    ex,
                    "Picture removal rejected. Profile issue: {ProfileIssue}.",
                    ex.Issue);
                throw new FaultException<ProfileIssue>(ex.Issue);
            }
            catch (SqlException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (EntityException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbUpdateException ex)
            {
                throw CreatePersistenceFault(ex, persistenceFailureMessage);
            }
            catch (DbEntityValidationException ex)
            {
                throw CreatePersistenceFault(ex, "Picture removal failed persistence validation.");
            }
        }

        private FaultException CreatePersistenceFault(Exception ex, string message)
        {
            _logger.LogError(ex, message);
            return new FaultException(ServiceFailureReason);
        }

        private int GetSessionPlayerId(string sessionKey)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                _logger.LogDebug("Authenticated operation rejected a missing session.");
                throw new FaultException<AccessIssue>(AccessIssue.SessionRequired);
            }

            try
            {
                return SessionManager.GetPlayerId(sessionKey);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogDebug(ex, "Authenticated operation rejected an unavailable session.");
                throw new FaultException<AccessIssue>(AccessIssue.SessionRequired);
            }
        }
    }
}
