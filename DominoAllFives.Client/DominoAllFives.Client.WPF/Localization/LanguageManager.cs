using System.Configuration;
using System.Globalization;
using System.Threading;

namespace DominoAllFives.Client.WPF.Localization
{
    /// <summary>
    /// Manages the application language, including loading, applying,
    /// and persisting the user's language preference.
    /// </summary>
    public static class LanguageManager
    {
        public const string SpanishLanguageCode = "es-MX";
        public const string EnglishLanguageCode = "en-US";
        public const string PortugueseLanguageCode = "pt-BR";

        private const string DefaultLanguageCode = SpanishLanguageCode;

        public static string CurrentLanguageCode { get; private set; }
            = DefaultLanguageCode;

        public static void LoadLanguage()
        {
            string languageCode =
                Properties.Settings.Default.LanguageCode;

            if (!IsSupportedLanguage(languageCode))
            {
                languageCode = DefaultLanguageCode;
            }

            ApplyLanguage(languageCode);
        }

        public static bool SaveLanguage(string languageCode)
        {
            if (!IsSupportedLanguage(languageCode))
            {
                return false;
            }

            try
            {
                Properties.Settings.Default.LanguageCode =
                    languageCode;

                Properties.Settings.Default.Save();

                ApplyLanguage(languageCode);

                return true;
            }
            catch (ConfigurationErrorsException)
            {
                return false;
            }
        }

        public static void ApplyLanguage(string languageCode)
        {
            if (!IsSupportedLanguage(languageCode))
            {
                languageCode = DefaultLanguageCode;
            }

            CultureInfo culture = new CultureInfo(languageCode);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Resources.Culture = culture;

            CurrentLanguageCode = languageCode;
        }

        private static bool IsSupportedLanguage(
            string languageCode)
        {
            return languageCode == SpanishLanguageCode
                || languageCode == EnglishLanguageCode
                || languageCode == PortugueseLanguageCode;
        }
    }
}