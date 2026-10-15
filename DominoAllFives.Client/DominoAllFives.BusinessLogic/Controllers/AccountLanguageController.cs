using System;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Exceptions;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Repositories;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.BusinessLogic.Controllers
{
    /// <summary>Coordinates persistence of the current account language.</summary>
    public class AccountLanguageController
    {
        private const string SpanishLanguageId = "es-MX";
        private const string EnglishLanguageId = "en-US";
        private const string PortugueseLanguageId = "pt-BR";
        private readonly Func<ILanguagePreferenceStore> _storeFactory;
        private readonly ILogger<AccountLanguageController> _logger;

        public AccountLanguageController(ILogger<AccountLanguageController> logger)
            : this(() => new LanguagePreferenceStore(), logger)
        {
        }

        public AccountLanguageController(Func<ILanguagePreferenceStore> storeFactory,
            ILogger<AccountLanguageController> logger)
        {
            _storeFactory = storeFactory
                ?? throw new ArgumentNullException(nameof(storeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Validates the language and commits it before reporting completion.</summary>
        public void ChangePreferredLanguage(int playerId, string languageId)
        {
            if (playerId <= 0)
            {
                throw new LanguagePreferenceException(
                    LanguagePreferenceFailureReason.NotSignedIn);
            }
            if (languageId != SpanishLanguageId &&
                languageId != EnglishLanguageId &&
                languageId != PortugueseLanguageId)
            {
                throw new LanguagePreferenceException(
                    LanguagePreferenceFailureReason.InvalidLanguage);
            }
            try
            {
                using (ILanguagePreferenceStore store = _storeFactory())
                {
                    PersistLanguage(store, playerId, languageId);
                }
            }
            catch (Exception ex) when (ex is SqlException ||
                ex is EntityException || ex is DbUpdateException)
            {
                _logger.LogError(
                    ex,
                    "Unable to persist preferred language for player {PlayerId}.",
                    playerId);
                throw new ServiceUnavailableException();
            }
        }

        private static void PersistLanguage(
            ILanguagePreferenceStore store, int playerId, string languageId)
        {
            if (!store.LanguageExists(languageId))
            {
                throw new LanguagePreferenceException(
                    LanguagePreferenceFailureReason.InvalidLanguage);
            }
            if (!store.UpdatePreferredLanguage(playerId, languageId))
            {
                throw new LanguagePreferenceException(
                    LanguagePreferenceFailureReason.AccountNotFound);
            }
            store.SaveChanges();
        }
    }
}
