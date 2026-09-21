using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace DominoAllFives.Client.WPF.Localization
{
    /// <summary>
    /// Manages the application language, including loading, applying,
    /// and persisting the user's language preference using a text file 
    /// in the local application data folder.
    /// </summary>
    public static class LanguageManager
    {
        public const string SpanishLanguageCode = "es-MX";
        public const string EnglishLanguageCode = "en-US";
        public const string PortugueseLanguageCode = "pt-BR";

        private const string DefaultLanguageCode = SpanishLanguageCode;
        private const string ApplicationFolderName = "DominoAllFives";
        private const string LanguageFileName = "language.txt";

        public static string CurrentLanguageCode { get; private set; }
            = DefaultLanguageCode;

        public static void LoadLanguage()
        {
            string languageCode = ReadSavedLanguage();

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
                string directoryPath = GetApplicationDataPath();

                Directory.CreateDirectory(directoryPath);

                string filePath = Path.Combine(
                    directoryPath,
                    LanguageFileName);

                File.WriteAllText(filePath, languageCode);

                ApplyLanguage(languageCode);

                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
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

        private static string ReadSavedLanguage()
        {
            try
            {
                string filePath = Path.Combine(
                    GetApplicationDataPath(),
                    LanguageFileName);

                if (!File.Exists(filePath))
                {
                    return DefaultLanguageCode;
                }

                string languageCode = File.ReadAllText(filePath).Trim();

                if (!IsSupportedLanguage(languageCode))
                {
                    return DefaultLanguageCode;
                }

                return languageCode;
            }
            catch (IOException)
            {
                return DefaultLanguageCode;
            }
            catch (UnauthorizedAccessException)
            {
                return DefaultLanguageCode;
            }
        }

        private static bool IsSupportedLanguage(string languageCode)
        {
            return languageCode == SpanishLanguageCode
                || languageCode == EnglishLanguageCode
                || languageCode == PortugueseLanguageCode;
        }

        private static string GetApplicationDataPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                ApplicationFolderName);
        }
    }
}