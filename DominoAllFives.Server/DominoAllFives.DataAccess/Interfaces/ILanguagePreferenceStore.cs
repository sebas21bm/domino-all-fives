using System;

namespace DominoAllFives.DataAccess.Interfaces
{
    /// <summary>
    /// Owns the persistence scope for an account language update.
    /// </summary>
    public interface ILanguagePreferenceStore : IDisposable
    {
        bool LanguageExists(string languageId);
        bool UpdatePreferredLanguage(int playerId, string languageId);
        void SaveChanges();
    }
}
