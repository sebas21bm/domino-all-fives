using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

using DominoAllFives.BusinessLogic.Security;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Handles player authentication operations.
    /// </summary>
    public class AuthenticationController
    {
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(
            ILogger<AuthenticationController> logger)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authenticates a player using the provided credentials.
        /// </summary>
        /// <param name="credentials">
        /// Contains the player's login credentials.
        /// </param>
        /// <returns>
        /// The authenticated player account.
        /// </returns>
        public LoginResult Login(LoginCredentials credentials)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            LoginFailureReason validation = credentials.EnsureValid();
            if (validation != LoginFailureReason.None)
            {
                return LoginResult.Rejected(validation);
            }

            try
            {
                using (DominoAllFivesEntities context =
                    new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository =
                        new PlayerRepository(context);

                    Player player =
                        playerRepository.GetByUsernameOrEmail(
                            credentials.EmailOrUsername);

                    if (player == null || player.IsDeleted)
                    {
                        return LoginResult.Rejected(LoginFailureReason.InvalidCredentials);
                    }

                    if (!PasswordHasher.VerifyPassword(
                        credentials.Password,
                        player.PasswordHash))
                    {
                        return LoginResult.Rejected(LoginFailureReason.InvalidCredentials);
                    }

                    PlayerAccount storedAccount =
                        PlayerAccount.FromStoredAccount(
                            player.IdPlayer,
                            player.Username,
                            player.PreferredLanguageId,
                            AccountAccessStatus.FromStoredState(
                                player.IsDeleted, player.IsBanned, player.SuspensionUntil));

                    DateTime utcNow = DateTime.UtcNow;
                    // SQL timestamps with unknown provenance retain their historical wall-clock meaning.
                    // This server-local comparison preserves prior behavior; it does not identify
                    // the original time zone or migrate historical values to UTC.
                    DateTime evaluationTime = player.SuspensionUntil.HasValue &&
                        player.SuspensionUntil.Value.Kind != DateTimeKind.Utc
                        ? utcNow.ToLocalTime()
                        : utcNow;
                    LoginFailureReason access = storedAccount.EnsureCanLogin(evaluationTime);
                    if (access != LoginFailureReason.None)
                    {
                        return LoginResult.Rejected(access);
                    }

                    return LoginResult.Success(storedAccount);
                }
            }
            catch (Exception ex) when (
                ex is SqlException ||
                ex is EntityException ||
                ex is DbUpdateException)
            {
                _logger.LogError(
                    ex,
                    "Database error while authenticating player.");

                throw;
            }
        }
    }
}
