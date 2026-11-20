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
        private const string SpanishLanguageCode = "es-MX";
        private const string EnglishLanguageCode = "en-US";
        private const string PortugueseLanguageCode = "pt-BR";
        private const string DefaultLanguageCode = SpanishLanguageCode;

        private static readonly Lazy<LanguageManager> instance =
            new Lazy<LanguageManager>(CreateInstance);

        private readonly ResourceManager _resourceManager;
        private readonly Func<string> _readDeviceLanguage;
        private readonly Action<string> _saveDeviceLanguage;
        private string _currentLanguageCode;

        public static LanguageManager Instance
        {
            get
            {
                return instance.Value;
            }
        }

        public string CurrentLanguageCode
        {
            get
            {
                return _currentLanguageCode;
            }
        }

        public CultureInfo CurrentCulture
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture;
            }
        }

        public string this[string key]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    return string.Empty;
                }

                string translation =
                    _resourceManager.GetString(
                    key,
                    Thread.CurrentThread.CurrentUICulture);
                return translation ?? $"[{key}]";
            }
        }

        private LanguageManager() : this(ReadDeviceLanguage, SaveDeviceLanguage)
        {
        }

        /// <summary>
        /// Creates a manager with a device preference store.
        /// </summary>
        public LanguageManager(Func<string> readDeviceLanguage, Action<string> saveDeviceLanguage)
        {
            _readDeviceLanguage = readDeviceLanguage ?? throw new ArgumentNullException(
                nameof(readDeviceLanguage));
            _saveDeviceLanguage = saveDeviceLanguage ?? throw new ArgumentNullException(
                nameof(saveDeviceLanguage));
            _resourceManager = new ResourceManager(
                typeof(Resources).FullName, typeof(Resources).Assembly);
            string savedLanguage = _readDeviceLanguage();
            if (string.IsNullOrWhiteSpace(savedLanguage) || !IsSupportedLanguage(savedLanguage))
            {
                savedLanguage = DefaultLanguageCode;
            }

            ApplyCulture(savedLanguage);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void ChangeLanguage(string cultureCode)
        {
            string savedLanguage = PersistDeviceLanguage(cultureCode);
            ApplyLanguage(savedLanguage);
        }

        /// <summary>
        /// Applies the account culture without overwriting device preferences.
        /// </summary>
        public void ApplyAccountLanguage(string languageCode)
        {
            ApplyLanguage(languageCode);
        }

        /// <summary>
        /// Restores the device preference after leaving an account session.
        /// </summary>
        public void RestoreDeviceLanguage()
        {
            ApplyLanguage(_readDeviceLanguage());
        }

        /// <summary>Saves the device preference without applying it.</summary>
        internal string PersistDeviceLanguage(string languageCode)
        {
            string supportedLanguage = IsSupportedLanguage(languageCode)
                ? languageCode
                : DefaultLanguageCode;
            _saveDeviceLanguage(supportedLanguage);
            return supportedLanguage;
        }

        /// <summary>Applies a language and refreshes bindings on the UI thread.</summary>
        internal void ApplyLanguage(string languageCode)
        {
            string supportedLanguage = IsSupportedLanguage(languageCode)
                ? languageCode
                : DefaultLanguageCode;
            ApplyCulture(supportedLanguage);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }

        private static LanguageManager CreateInstance()
        {
            return new LanguageManager();
        }

        private static string ReadDeviceLanguage()
        {
            return Settings.Default.LanguageCode;
        }

        private static bool IsSupportedLanguage(string languageCode)
        {
            return languageCode == SpanishLanguageCode
                || languageCode == EnglishLanguageCode
                || languageCode == PortugueseLanguageCode;
        }

        private static void SaveDeviceLanguage(string language)
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
