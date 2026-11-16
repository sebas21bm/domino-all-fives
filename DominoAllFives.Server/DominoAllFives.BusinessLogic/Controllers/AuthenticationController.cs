using System;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.BusinessLogic.Security;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using Player = DominoAllFives.DataAccess.Models.Player;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>Authenticates accounts and enforces their access restrictions.</summary>
    public class AuthenticationController
    {
        /// <summary>
        /// Authenticates credentials and reconstructs the stored account.
        /// </summary>
        /// <param name = "credentials">The credentials supplied by the player.</param>
        /// <returns>The authenticated account without a session key.</returns>
        /// <exception cref = "AccountRuleException">The account cannot sign in.</exception>
        public PlayerAccount LoginWithAccount(Credentials credentials)
        {
            ValidateCredentials(credentials);

            using (DominoAllFivesEntities context = new DominoAllFivesEntities())
            {
                IPlayerRepository playerRepository = new PlayerRepository(context);
                Player player = FindAuthenticatedPlayer(credentials, playerRepository);
                ValidateAccountCanLogin(player);
                return CreatePlayerAccount(player);
            }
        }


        private static PlayerAccount CreatePlayerAccount(Player player)
        {
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

        private static void ValidateAccountCanLogin(Player player)
        {
            if (player.IsBanned)
            {
                throw new AccountRuleException(AccountIssue.Banned);
            }

            DateTime utcNow = DateTime.UtcNow;
            DateTime evaluationTime =
                player.SuspensionUntil.HasValue
                && player.SuspensionUntil.Value.Kind != DateTimeKind.Utc
                ? utcNow.ToLocalTime() : utcNow;

            if (player.SuspensionUntil.HasValue
                && player.SuspensionUntil.Value > evaluationTime)
            {
                throw new AccountRuleException(AccountIssue.Suspended);
            }
        }

        private static void ValidateCredentials(Credentials credentials)
        {
            if (credentials == null
                || credentials.Validate()
                != LoginStatus.None)
            {
                throw new AccountRuleException(AccountIssue.InvalidCredentials);
            }
        }

        private static Player FindAuthenticatedPlayer(
            Credentials credentials,
            IPlayerRepository playerRepository)
        {
            Player player = playerRepository.GetByUsernameOrEmail(
                credentials.EmailOrUsername);
            if (player == null
                || player.IsDeleted
                || !PasswordHasher.VerifyPassword(credentials.Password,player.PasswordHash))
            {
                throw new AccountRuleException(AccountIssue.InvalidCredentials);
            }

            return player;
        }
    }
}
