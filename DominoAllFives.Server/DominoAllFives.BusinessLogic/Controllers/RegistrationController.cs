using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Security;
using DominoAllFives.BusinessLogic.Validation;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using Player = DominoAllFives.DataAccess.Models.Player;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>Creates player accounts and their initial statistics.</summary>
    public class RegistrationController
    {
        /// <summary>
        /// Validates registration and persists the account and statistics together.
        /// </summary>
        /// <param name = "registrationData">The information required to create the account.</param>
        /// <returns>The created account without a session key.</returns>
        /// <exception cref = "System.ArgumentNullException">
        /// The registration data is null.
        /// </exception>
        /// <exception cref = "RegistrationRuleException">
        /// Registration violates an account rule.
        /// </exception>
        public PlayerAccount Register(AccountRegistration registrationData)
        {
            RegistrationValidator.Validate(registrationData);

            using (DominoAllFivesEntities context = new DominoAllFivesEntities())
            {
                IPlayerRepository playerRepository = new PlayerRepository(context);
                IPlayerStatsRepository playerStatsRepository = new PlayerStatsRepository(context);
                ValidateAvailability(registrationData, playerRepository);
                Player player = CreatePlayer(registrationData);
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

                AccountAccessStatus accessStatus = AccountAccessStatus.FromStoredState(
                    player.IsDeleted,
                    player.IsBanned,
                    player.SuspensionUntil);
                return PlayerAccount.FromStoredAccount(
                    player.IdPlayer,
                    player.Username,
                    player.PreferredLanguageId,
                    accessStatus);
            }
        }

        private static void ValidateAvailability(
            AccountRegistration registrationData,
            IPlayerRepository playerRepository)
        {
            if (playerRepository.UsernameExists(registrationData.Username))
            {
                throw new RegistrationRuleException(RegistrationIssue.UsernameAlreadyExists);
            }

            if (playerRepository.EmailExists(registrationData.Email))
            {
                throw new RegistrationRuleException(RegistrationIssue.EmailAlreadyExists);
            }
        }

        private static Player CreatePlayer(AccountRegistration registrationData)
        {
            return new Player
            {
                Username = registrationData.Username,
                Email = registrationData.Email,
                PasswordHash = PasswordHasher.HashPassword(registrationData.Password),
                ProfilePicture = null,
                IsGuest = false,
                SuspensionUntil = null,
                IsBanned = false,
                PreferredLanguageId = registrationData.PreferredLanguageId,
                IsDeleted = false,
                DeletedAt = null
            };
        }
    }
}
