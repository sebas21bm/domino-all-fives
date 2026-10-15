using System;
using System.ComponentModel;
using System.Configuration;
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

        private static readonly Lazy<LanguageManager> instance = 
            new Lazy<LanguageManager>(() => new LanguageManager());

        private readonly ResourceManager _resourceManager;
        private string _currentLanguageCode;
        private readonly Func<string> _readDeviceLanguage;
        private readonly Action<string> _saveDeviceLanguage;

        public static LanguageManager Instance => instance.Value;

        public string CurrentLanguageCode => _currentLanguageCode;

        public CultureInfo CurrentCulture => Thread.CurrentThread.CurrentUICulture;

        public event PropertyChangedEventHandler PropertyChanged;

        private LanguageManager()
            : this(() => Settings.Default.LanguageCode, language =>
            {
                string previousLanguage = Settings.Default.LanguageCode;
                Settings.Default.LanguageCode = language;
                try
                {
                    Settings.Default.Save();
                }
                catch (ConfigurationErrorsException)
                {
                    Settings.Default.LanguageCode = previousLanguage;
                    throw;
                }
            })
        {
        }

        /// <summary>Creates a manager with a device preference store.</summary>
        public LanguageManager(
            Func<string> readDeviceLanguage,
            Action<string> saveDeviceLanguage)
        {
            _readDeviceLanguage = readDeviceLanguage
                ?? throw new ArgumentNullException(nameof(readDeviceLanguage));
            _saveDeviceLanguage = saveDeviceLanguage
                ?? throw new ArgumentNullException(nameof(saveDeviceLanguage));
            _resourceManager = new ResourceManager(
                "DominoAllFives.Client.WPF.Localization.Resources", typeof(Resources).Assembly);

            string savedLanguage = _readDeviceLanguage();
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

                string translation = _resourceManager.GetString(key, 
                    Thread.CurrentThread.CurrentUICulture);
                return translation ?? $"[{key}]";
            }
        }

        public void ChangeLanguage(string cultureCode)
        {
            if (string.IsNullOrWhiteSpace(cultureCode) || !IsSupportedLanguage(cultureCode))
            {
                cultureCode = DefaultLanguageCode;
            }

            _saveDeviceLanguage(cultureCode);
            ApplyCulture(cultureCode);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }

        /// <summary>Applies the account culture without overwriting device preferences.</summary>
        public void ApplyAccountLanguage(string languageCode)
        {
            if (!IsSupportedLanguage(languageCode))
            {
                languageCode = DefaultLanguageCode;
            }
            ApplyCulture(languageCode);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }

        /// <summary>Restores the device preference after leaving an account session.</summary>
        public void RestoreDeviceLanguage()
        {
            string languageCode = _readDeviceLanguage();
            ApplyCulture(IsSupportedLanguage(languageCode) ? languageCode : DefaultLanguageCode);
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