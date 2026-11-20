using System;
using DominoAllFives.BusinessLogic.Exceptions;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Repositories;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>
    /// Coordinates persistence of the current account language.
    /// </summary>
    public class AccountLanguageController
    {
        private const string SpanishLanguageId = "es-MX";
        private const string EnglishLanguageId = "en-US";
        private const string PortugueseLanguageId = "pt-BR";

        private readonly ILogger<AccountLanguageController> _logger;

        public AccountLanguageController(ILogger<AccountLanguageController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Validates the language and commits it before reporting completion.
        /// </summary>
        /// <param name = "playerId">The player identified by the server.</param>
        /// <param name = "preferredLanguageId">The requested language identifier.</param>
        /// <exception cref = "ArgumentOutOfRangeException">
        /// The player ID is not positive.
        /// </exception>
        /// <exception cref = "AccountRuleException">
        /// The account or language was rejected.
        /// </exception>
        /// <exception cref = "InvalidOperationException">
        /// The supported language is missing from the catalog.
        /// </exception>
        public void ChangePreferredLanguage(int playerId, string preferredLanguageId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            if (preferredLanguageId != SpanishLanguageId
                && preferredLanguageId != EnglishLanguageId
                && preferredLanguageId != PortugueseLanguageId)
            {
                throw new AccountRuleException(AccountIssue.UnsupportedLanguage);
            }

            using (ILanguagePreferenceStore store = new LanguagePreferenceStore())
            {
                PersistLanguage(store, playerId, preferredLanguageId);
            }

            _logger.LogDebug("Preferred language saved for player {PlayerId}.", playerId);
        }

        private static void PersistLanguage(
            ILanguagePreferenceStore store,
            int playerId,
            string preferredLanguageId)
        {
            if (!store.LanguageExists(preferredLanguageId))
            {
                throw new InvalidOperationException(
                    "The supported language is absent from the language catalog.");
            }

            if (!store.UpdatePreferredLanguage(playerId, preferredLanguageId))
            {
                throw new AccountRuleException(AccountIssue.PlayerNotFound);
            }

            store.SaveChanges();
        }
    }
}
