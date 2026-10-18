using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using DominoAllFives.BusinessLogic.Security;
using DominoAllFives.BusinessLogic.Validation;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Coordinates operations related to player account management.
    /// </summary>
    public class AccountController
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Logically deletes a player account after verifying its password.
        /// </summary>
        /// <param name="playerId">The account identifier.</param>
        /// <param name="password">The current password to verify.</param>
        public DeleteFailureReason DeleteAccount(int playerId, string password)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return DeleteFailureReason.PasswordNotMatch;
            }

            try
            {
                using (DominoAllFivesEntities context =
                    new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository =
                        new PlayerRepository(context);

                    Player player =
                        playerRepository.GetById(playerId);

                    if (player == null || player.IsDeleted)
                    {
                        return DeleteFailureReason.AccountNotFound;
                    }

                    if (!PasswordHasher.VerifyPassword(
                        password, player.PasswordHash))
                    {
                        return DeleteFailureReason.PasswordNotMatch;
                    }

                    bool wasDeleted =
                        playerRepository.SoftDelete(playerId);

                    if (!wasDeleted)
                    {
                        return DeleteFailureReason.AccountNotFound;
                    }

                    context.SaveChanges();

                    return DeleteFailureReason.None;
                }
            }
            catch (Exception ex) when (ex is SqlException 
                                       || ex is EntityException
                                       || ex is DbUpdateException)
            {
                _logger.LogError(
                    ex,
                    "Unable to delete account for player {PlayerId}.",
                    playerId);

                throw;
            }
        }

        /// <summary>
        /// Changes a player account password after verifying
        /// the current password and validating the new password.
        /// </summary>
        /// <param name="playerId">The account identifier.</param>
        /// <param name="currentPassword">The current password to verify.</param>
        /// <param name="newPassword">The new password subject to the creation policy.</param>
        public ChangePasswordFailureReason ChangePassword(int playerId, string currentPassword, string newPassword)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                return ChangePasswordFailureReason.IncorrectCurrentPassword;
            }

            if (!AccountValidator.IsPasswordValid(newPassword))
            {
                return ChangePasswordFailureReason.InvalidNewPassword;
            }

            try
            {
                using (DominoAllFivesEntities context =
                    new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository =
                        new PlayerRepository(context);

                    Player player =
                        playerRepository.GetById(playerId);

                    if (player == null || player.IsDeleted)
                    {
                        return ChangePasswordFailureReason.AccountNotFound;
                    }

                    if (!PasswordHasher.VerifyPassword(
                        currentPassword,
                        player.PasswordHash))
                    {
                        return ChangePasswordFailureReason.IncorrectCurrentPassword;
                    }

                    if (PasswordHasher.VerifyPassword(
                        newPassword,
                        player.PasswordHash))
                    {
                        return ChangePasswordFailureReason.SameAsCurrentPassword;
                    }

                    string newPasswordHash =
                        PasswordHasher.HashPassword(newPassword);

                    bool wasUpdated = playerRepository.UpdatePassword(
                        playerId,
                        newPasswordHash);

                    if (!wasUpdated)
                    {
                        return ChangePasswordFailureReason.AccountNotFound;
                    }

                    context.SaveChanges();

                    return ChangePasswordFailureReason.None;
                }
            }
            catch (Exception ex) when (ex is SqlException
                                       || ex is EntityException
                                       || ex is DbUpdateException)
            {
                _logger.LogError(
                    ex,
                    "Unable to change password for player {PlayerId}.",
                    playerId);

                throw;
            }
        }

    }
}
