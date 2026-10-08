using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using DominoAllFives.BusinessLogic.Security;
using DominoAllFives.Contracts.DTOs;
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
        /// <param name="request">
        /// The account deletion request.
        /// </param>
        /// <returns>
        /// The result of the account deletion operation.
        /// </returns>
        public DeleteAccountResultDto DeleteAccount(
            DeleteAccountRequestDto request)
        {
            if (request == null || request.PlayerId <= 0)
            {
                return CreateDeleteFailureResult(
                    DeleteFailureReason.AccountNotFound);
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return CreateDeleteFailureResult(
                    DeleteFailureReason.PasswordNotMatch);
            }

            try
            {
                using (DominoAllFivesEntities context =
                    new DominoAllFivesEntities())
                {
                    IPlayerRepository playerRepository =
                        new PlayerRepository(context);

                    Player player =
                        playerRepository.GetById(request.PlayerId);

                    if (player == null || player.IsDeleted)
                    {
                        return CreateDeleteFailureResult(
                            DeleteFailureReason.AccountNotFound);
                    }

                    if (!PasswordHasher.VerifyPassword(
                        request.Password, player.PasswordHash))
                    {
                        return CreateDeleteFailureResult(
                            DeleteFailureReason.PasswordNotMatch);
                    }

                    bool wasDeleted =
                        playerRepository.SoftDelete(request.PlayerId);

                    if (!wasDeleted)
                    {
                        return CreateDeleteFailureResult(
                            DeleteFailureReason.AccountNotFound);
                    }

                    context.SaveChanges();

                    return CreateDeleteSuccessResult();
                }
            }
            catch (Exception ex) when (ex is SqlException 
                                       || ex is EntityException
                                       || ex is DbUpdateException)
            {
                _logger.LogError(
                    ex,
                    "Unable to delete account for player {PlayerId}.",
                    request.PlayerId);

                return CreateDeleteFailureResult(
                    DeleteFailureReason.ServiceUnavailable);
            }
        }

        private DeleteAccountResultDto CreateDeleteSuccessResult()
        {
            return new DeleteAccountResultDto
            {
                IsSuccessful = true,
                FailureReason = DeleteFailureReason.None
            };
        }

        private DeleteAccountResultDto CreateDeleteFailureResult(
            DeleteFailureReason reason)
        {
            return new DeleteAccountResultDto
            {
                IsSuccessful = false,
                FailureReason = reason
            };
        }
    }
}