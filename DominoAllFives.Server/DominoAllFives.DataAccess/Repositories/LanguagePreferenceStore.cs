using System.Linq;
using DominoAllFives.DataAccess.Interfaces;
using DominoAllFives.DataAccess.Models;

namespace DominoAllFives.DataAccess.Repositories
{
    /// <summary>
    /// Updates an active account inside one Entity Framework context.
    /// </summary>
    public sealed class LanguagePreferenceStore : ILanguagePreferenceStore
    {
        private readonly DominoAllFivesEntities _context = new DominoAllFivesEntities();

        /// <inheritdoc/>
        public bool LanguageExists(string languageId)
        {
            return _context.Language.Any(language => language.IdLanguage == languageId);
        }

        /// <inheritdoc/>
        public bool UpdatePreferredLanguage(int playerId, string languageId)
        {
            Player player = _context.Player.FirstOrDefault(account =>
                account.IdPlayer == playerId && !account.IsDeleted && !account.IsGuest);
            if (player == null)
            {
                return false;
            }
            player.PreferredLanguageId = languageId;
            return true;
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
