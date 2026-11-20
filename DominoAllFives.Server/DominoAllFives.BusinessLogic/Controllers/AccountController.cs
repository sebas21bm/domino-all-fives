using System;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Managers;
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
        /// <exception cref = "ArgumentOutOfRangeException">The player identifier is invalid.</exception>
        /// <exception cref = "AccountRuleException">The account or password was rejected.</exception>
        public void DeleteAccount(int playerId, string password)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new AccountRuleException(AccountIssue.IncorrectCurrentPassword);
            }

            using (DominoAllFivesEntities context = new DominoAllFivesEntities())
            {
                IPlayerRepository playerRepository = new PlayerRepository(context);
                Player player = playerRepository.GetById(playerId);
                if (player == null || player.IsDeleted)
                {
                    throw new AccountRuleException(AccountIssue.PlayerNotFound);
                }

                if (!PasswordHasher.VerifyPassword(password, player.PasswordHash))
                {
                    throw new AccountRuleException(AccountIssue.IncorrectCurrentPassword);
                }

                bool wasDeleted = playerRepository.SoftDelete(playerId);
                if (!wasDeleted)
                {
                    throw new AccountRuleException(AccountIssue.PlayerNotFound);
                }

                context.SaveChanges();
            }

            SessionManager.RemovePlayerSession(playerId);
        }

        /// <summary>
        /// Changes a player account password after verifying
        /// the current password and validating the new password.
        /// </summary>
        /// <param name = "playerId">The account identifier.</param>
        /// <param name = "currentPassword">The current password to verify.</param>
        /// <param name = "newPassword">The new password subject to the creation policy.</param>
        /// <exception cref = "ArgumentOutOfRangeException">The player identifier is invalid.</exception>
        /// <exception cref = "AccountRuleException">The account or password was rejected.</exception>
        public void ChangePassword(
            int playerId,
            string currentPassword,
            string newPassword)
        {
            ValidatePasswordChange(playerId, currentPassword, newPassword);

            using (DominoAllFivesEntities context = new DominoAllFivesEntities())
            {
                IPlayerRepository playerRepository = new PlayerRepository(context);
                Player player = playerRepository.GetById(playerId);
                if (player == null || player.IsDeleted)
                {
                    throw new AccountRuleException(AccountIssue.PlayerNotFound);
                }

                if (!PasswordHasher.VerifyPassword(currentPassword, player.PasswordHash))
                {
                    throw new AccountRuleException(AccountIssue.IncorrectCurrentPassword);
                }

                if (PasswordHasher.VerifyPassword(newPassword, player.PasswordHash))
                {
                    throw new AccountRuleException(AccountIssue.PasswordUnchanged);
                }

                string newPasswordHash = PasswordHasher.HashPassword(newPassword);
                bool wasUpdated = playerRepository.UpdatePassword(playerId, newPasswordHash);
                if (!wasUpdated)
                {
                    throw new AccountRuleException(AccountIssue.PlayerNotFound);
                }

                context.SaveChanges();
            }
        }
        private static void ValidatePasswordChange(
            int playerId, string currentPassword, string newPassword)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                throw new AccountRuleException(AccountIssue.IncorrectCurrentPassword);
            }

            if (!AccountValidator.IsPasswordValid(newPassword))
            {
                throw new AccountRuleException(AccountIssue.InvalidPassword);
            }
        }
    }
}