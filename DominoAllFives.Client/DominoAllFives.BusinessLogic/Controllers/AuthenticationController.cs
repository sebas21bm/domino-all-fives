using DominoAllFives.BusinessLogic.Security;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using System;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Handles player authentication operations.
    /// </summary>
    public class AuthenticationController
    {
        public LoginResultDto Login(
            string usernameOrEmail,
            string password)
        {
            using (DominoAllFivesEntities context =
                new DominoAllFivesEntities())
            {
                IPlayerRepository playerRepository =
                    new PlayerRepository(context);

                Player player =
                    playerRepository.GetByUsernameOrEmail(usernameOrEmail);

                if (player == null ||
                    !PasswordHasher.VerifyPassword(
                        password,
                        player.PasswordHash))
                {
                    return CreateFailureResult(
                        LoginFailureReason.InvalidCredentials);
                }

                if (player.IsBanned)
                {
                    return CreateFailureResult(
                        LoginFailureReason.Banned);
                }

                if (player.SuspensionUntil.HasValue &&
                    player.SuspensionUntil.Value > DateTime.Now)
                {
                    return CreateFailureResult(
                        LoginFailureReason.Suspended);
                }

                return new LoginResultDto
                {
                    IsSuccessful = true,
                    PlayerId = player.IdPlayer,
                    Username = player.Username,
                    FailureReason = LoginFailureReason.None
                };
            }
        }

        private LoginResultDto CreateFailureResult(
            LoginFailureReason failureReason)
        {
            return new LoginResultDto
            {
                IsSuccessful = false,
                FailureReason = failureReason
            };
        }
    }
}