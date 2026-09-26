using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;

using DominoAllFives.Client.WPF.Properties;

namespace DominoAllFives.Client.WPF.Localization
{
    /// <summary>
    /// Manages application culture and handles dynamic translation updates for MVVM bindings.
    /// </summary>
    public class LanguageManager : INotifyPropertyChanged
    {
        private const string DefaultLanguageCode = "es-MX";
        private const string SpanishLanguageCode = "es-MX";
        private const string EnglishLanguageCode = "en-US";
        private const string PortugueseLanguageCode = "pt-BR";

        private static readonly Lazy<LanguageManager> _instance = new Lazy<LanguageManager>(() => new LanguageManager());

        private readonly ResourceManager _resourceManager;
        private string _currentLanguageCode;

        public static LanguageManager Instance => _instance.Value;

        public string CurrentLanguageCode => _currentLanguageCode;

        public CultureInfo CurrentCulture => Thread.CurrentThread.CurrentUICulture;

        public event PropertyChangedEventHandler PropertyChanged;

        private LanguageManager()
        {
            _resourceManager = new ResourceManager("DominoAllFives.Client.WPF.Localization.Resources",
                typeof(Resources).Assembly);

            string savedLanguage = Settings.Default.LanguageCode;
            if (string.IsNullOrWhiteSpace(savedLanguage) || !IsSupportedLanguage(savedLanguage))
            {
                savedLanguage = DefaultLanguageCode;
            }

            ApplyCulture(savedLanguage);
        }

        public string this[string key]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    return string.Empty;
                }

                string translation = _resourceManager.GetString(key, Thread.CurrentThread.CurrentUICulture);
                return translation ?? $"[{key}]";
            }
        }

        public void ChangeLanguage(string cultureCode)
        {
            if (string.IsNullOrWhiteSpace(cultureCode) || !IsSupportedLanguage(cultureCode))
            {
                cultureCode = DefaultLanguageCode;
            }

            ApplyCulture(cultureCode);
            Settings.Default.LanguageCode = cultureCode;
            Settings.Default.Save();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }

        private static bool IsSupportedLanguage(string languageCode)
        {
            return languageCode == SpanishLanguageCode
                || languageCode == EnglishLanguageCode
                || languageCode == PortugueseLanguageCode;
        }

        private void ApplyCulture(string cultureCode)
        {
            CultureInfo culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            _currentLanguageCode = cultureCode;
        }
    }
}