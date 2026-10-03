using DominoAllFives.BusinessLogic.Security;
using DominoAllFives.BusinessLogic.Validation;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Coordinates player account registration operations.
    /// </summary>
    public class RegistrationController
    {
        /// <summary>
        /// Registers a new player account.
        /// </summary>
        /// <param name="registrationData">
        /// The information required to create the player account.
        /// </param>
        /// <returns>
        /// The result of the registration attempt.
        /// </returns>
        public RegistrationResultDto Register(
            RegisterAccountDto registrationData)
        {
            RegistrationFailureReason validationResult =
                RegistrationValidator.Validate(registrationData);

            if (validationResult != RegistrationFailureReason.None)
            {
                return CreateFailureResult(validationResult);
            }

            using (DominoAllFivesEntities context =
                new DominoAllFivesEntities())
            {
                IPlayerRepository playerRepository =
                    new PlayerRepository(context);

                IPlayerStatsRepository playerStatsRepository =
                    new PlayerStatsRepository(context);

                if (playerRepository.UsernameExists(
                    registrationData.Username))
                {
                    return CreateFailureResult(
                        RegistrationFailureReason.UsernameAlreadyExists);
                }

                if (playerRepository.EmailExists(
                    registrationData.Email))
                {
                    return CreateFailureResult(
                        RegistrationFailureReason.EmailAlreadyExists);
                }

                Player player = new Player
                {
                    Username = registrationData.Username,
                    Email = registrationData.Email,
                    PasswordHash = PasswordHasher.HashPassword(
                        registrationData.Password),
                    ProfilePicture = null,
                    Status = "Offline",
                    IsGuest = false,
                    SuspensionUntil = null,
                    IsBanned = false,
                    PreferredLanguageId =
                        registrationData.PreferredLanguageId
                };

                PlayerStats playerStats = new PlayerStats
                {
                    GamesWon = 0,
                    GamesPlayed = 0,
                    TotalPointsScored = 0,
                    LastVictoryDate = null,
                    Player = player
                };

                playerRepository.Add(player);
                playerStatsRepository.Add(playerStats);

                context.SaveChanges();

                return new RegistrationResultDto
                {
                    IsSuccessful = true,
                    PlayerId = player.IdPlayer,
                    Username = player.Username,
                    FailureReason = RegistrationFailureReason.None
                };
            }
        }

        private RegistrationResultDto CreateFailureResult(
            RegistrationFailureReason failureReason)
        {
            return new RegistrationResultDto
            {
                IsSuccessful = false,
                PlayerId = 0,
                Username = null,
                FailureReason = failureReason
            };
        }
    }
}