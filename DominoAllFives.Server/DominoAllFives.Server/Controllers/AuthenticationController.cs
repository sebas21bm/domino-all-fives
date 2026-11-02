using System;
using DominoAllFives.BusinessLogic.Security;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using DominoAllFives.Server.Exceptions;

namespace DominoAllFives.Server.Controllers
{
    /// <summary>Authenticates accounts and enforces their access restrictions.</summary>
    public class AuthenticationController
    {
        private readonly Func<string, Player> _findPlayer;
        private readonly Func<DateTime> _utcNow;

        public AuthenticationController() : this(FindStoredPlayer, () => DateTime.UtcNow)
        {
        }

        internal AuthenticationController(Func<string, Player> findPlayer, Func<DateTime> utcNow)
        {
            _findPlayer = findPlayer ?? throw new ArgumentNullException(nameof(findPlayer));
            _utcNow = utcNow ?? throw new ArgumentNullException(nameof(utcNow));
        }

        public PlayerAccount LoginWithAccount(Credentials credentials)
        {
            if (credentials == null || string.IsNullOrWhiteSpace(credentials.EmailOrUsername) ||
                string.IsNullOrWhiteSpace(credentials.Password))
            {
                throw new AccountRuleException(AccountIssue.InvalidCredentials);
            }

            Player player = _findPlayer(credentials.EmailOrUsername);
            if (player == null || player.IsDeleted ||
                !PasswordHasher.VerifyPassword(credentials.Password, player.PasswordHash))
            {
                throw new AccountRuleException(AccountIssue.InvalidCredentials);
            }

            if (player.IsBanned)
            {
                throw new AccountRuleException(AccountIssue.Banned);
            }

            DateTime utcNow = _utcNow();
            DateTime evaluationTime = player.SuspensionUntil.HasValue &&
                player.SuspensionUntil.Value.Kind != DateTimeKind.Utc
                ? utcNow.ToLocalTime()
                : utcNow;
            if (player.SuspensionUntil.HasValue && player.SuspensionUntil.Value > evaluationTime)
            {
                throw new AccountRuleException(AccountIssue.Suspended);
            }

            return PlayerAccount.FromStoredAccount(player.IdPlayer, player.Username,
                player.PreferredLanguageId, AccountAccessStatus.FromStoredState(
                    player.IsDeleted, player.IsBanned, player.SuspensionUntil));
        }

        private static Player FindStoredPlayer(string identifier)
        {
            using (DominoAllFivesEntities context = new DominoAllFivesEntities())
            {
                return new PlayerRepository(context).GetByUsernameOrEmail(identifier);
            }
        }
    }
}
