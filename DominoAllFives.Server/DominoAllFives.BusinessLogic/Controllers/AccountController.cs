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
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Logically deletes a player account after verifying its password.
        /// </summary>
        /// <param name = "playerId">The account identifier.</param>
        /// <param name = "password">The current password to verify.</param>
        public DeleteStatus DeleteAccount(int playerId, string password)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return DeleteStatus.PasswordNotMatch;
            }

            using (DominoAllFivesEntities context = new DominoAllFivesEntities())
            {
                IPlayerRepository playerRepository = new PlayerRepository(context);
                Player player = playerRepository.GetById(playerId);
                if (player == null || player.IsDeleted)
                {
                    return DeleteStatus.AccountNotFound;
                }

                if (!PasswordHasher.VerifyPassword(password, player.PasswordHash))
                {
                    return DeleteStatus.PasswordNotMatch;
                }

                bool wasDeleted = playerRepository.SoftDelete(playerId);
                if (!wasDeleted)
                {
                    return DeleteStatus.AccountNotFound;
                }

                context.SaveChanges();
                return DeleteStatus.None;
            }
        }

        /// <summary>
        /// Changes a player account password after verifying
        /// the current password and validating the new password.
        /// </summary>
        /// <param name = "playerId">The account identifier.</param>
        /// <param name = "currentPassword">The current password to verify.</param>
        /// <param name = "newPassword">The new password subject to the creation policy.</param>
        public ChangePasswordStatus ChangePassword(
            int playerId,
            string currentPassword,
            string newPassword)
        {
            ChangePasswordStatus validation = ValidatePasswordChange(
                playerId, currentPassword, newPassword);
            if (validation != ChangePasswordStatus.None)
            {
                return validation;
            }
            using (DominoAllFivesEntities context = new DominoAllFivesEntities())
            {
                IPlayerRepository playerRepository = new PlayerRepository(context);
                Player player = playerRepository.GetById(playerId);
                if (player == null || player.IsDeleted)
                {
                    return ChangePasswordStatus.AccountNotFound;
                }

                if (!PasswordHasher.VerifyPassword(currentPassword, player.PasswordHash))
                {
                    return ChangePasswordStatus.IncorrectCurrentPassword;
                }

                if (PasswordHasher.VerifyPassword(newPassword, player.PasswordHash))
                {
                    return ChangePasswordStatus.SameAsCurrentPassword;
                }

                string newPasswordHash = PasswordHasher.HashPassword(newPassword);
                bool wasUpdated = playerRepository.UpdatePassword(playerId, newPasswordHash);
                if (!wasUpdated)
                {
                    return ChangePasswordStatus.AccountNotFound;
                }

                context.SaveChanges();
                return ChangePasswordStatus.None;
            }
        }
        private static ChangePasswordStatus ValidatePasswordChange(
            int playerId, string currentPassword, string newPassword)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                return ChangePasswordStatus.IncorrectCurrentPassword;
            }
            return AccountValidator.IsPasswordValid(newPassword)
                ? ChangePasswordStatus.None : ChangePasswordStatus.InvalidNewPassword;
        }
    }
}