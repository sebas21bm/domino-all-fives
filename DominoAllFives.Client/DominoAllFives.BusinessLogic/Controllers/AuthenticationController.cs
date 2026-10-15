using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

using DominoAllFives.BusinessLogic.Security;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Exceptions;
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
        /// Authenticates a player using the provided account.
        /// </summary>
        /// <param name="account">
        /// Contains the player's login credentials.
        /// </param>
        /// <returns>
        /// The authenticated player account.
        /// </returns>
        public PlayerAccount Login(PlayerAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
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
                            account.EmailOrUsername);

                    if (player == null || player.IsDeleted)
                    {
                        throw new AuthenticationException(
                            LoginFailureReason.InvalidCredentials);
                    }

                    if (!PasswordHasher.VerifyPassword(
                        account.Password,
                        player.PasswordHash))
                    {
                        throw new AuthenticationException(
                            LoginFailureReason.InvalidCredentials);
                    }

                    PlayerAccount storedAccount =
                        PlayerAccount.FromStoredAccount(
                            player.IdPlayer,
                            player.Username,
                            player.IsDeleted,
                            player.IsBanned,
                            player.SuspensionUntil);

                    storedAccount.SetPreferredLanguage(player.PreferredLanguageId);
                    storedAccount.MarkAsAuthenticated();

                    return storedAccount;
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

                throw new ServiceUnavailableException();
            }
        }
    }
}
