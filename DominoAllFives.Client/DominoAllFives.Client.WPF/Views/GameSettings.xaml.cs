using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DominoAllFives.Client.WPF.Localization;

namespace DominoAllFives.Client.WPF.Views
{
    public partial class GameSettings : Page
    {
        private string _selectedLanguageCode;
        
        public GameSettings()
        {
            InitializeComponent();
            _selectedLanguageCode = Properties.Settings.Default.LanguageCode;

            if (string.IsNullOrWhiteSpace(_selectedLanguageCode))
            {
                _selectedLanguageCode = "es-MX";
            }

            SetSelectedLanguageButton(GetLanguageButton(_selectedLanguageCode));
        }
        

        private void AccountButtonClick(object sender,
            RoutedEventArgs eventArgs)
        {
            ShowAccountSection();
        }

        private void LanguageButtonClick(object sender,
            RoutedEventArgs eventArgs)
        {
            ShowLanguageSection();
        }

        
        private void LanguageOptionButtonClick(
            object sender,
            RoutedEventArgs eventArgs)
        {
            if (sender is Button clickedButton && clickedButton.Tag is string languageCode)
            {
                _selectedLanguageCode = languageCode;
                SetSelectedLanguageButton(clickedButton);
            }
        }
        
        
        private void SaveLanguageButtonClick(object sender,
            RoutedEventArgs eventArgs)
        {
            LanguageManager.Instance.ChangeLanguage(_selectedLanguageCode);
        }
        

        private void LogoutButtonClick(object sender,
            RoutedEventArgs eventArgs)
        {
            
        }

        private void BackButtonClick(object sender,
            RoutedEventArgs eventArgs)
        {
            
        }


        private void ShowAccountSection()
        {
            grdAccount.Visibility = Visibility.Visible;
            grdLanguage.Visibility = Visibility.Collapsed;

            SetSelectedNavigationButton(btnAccount);
        }

        private void ShowLanguageSection()
        {
            grdAccount.Visibility = Visibility.Collapsed;
            grdLanguage.Visibility = Visibility.Visible;

            SetSelectedNavigationButton(btnLanguage);
        }

        private void SetSelectedNavigationButton(
            Button selectedButton)
        {
            btnAccount.Style = (Style)FindResource("NavigationTabButton");
            btnLanguage.Style = (Style)FindResource("NavigationTabButton");

            selectedButton.Style = (Style)FindResource("SelectedNavigationTabButton");
        }

        private void SetSelectedLanguageButton(
            Button selectedButton)
        {
            Brush unselectedForeground = (Brush)FindResource("DisabledGrayBrush");
            Brush selectedForeground = (Brush)FindResource("CreamLightBrush");

            btnEnglish.Foreground = unselectedForeground;
            btnSpanish.Foreground = unselectedForeground;
            btnPortuguese.Foreground = unselectedForeground;

            selectedButton.Foreground = selectedForeground;
        }

        
        
        private string GetLanguageCode(Button selectedButton)
        {
            if (selectedButton.Tag is string languageCode)
            {
                return languageCode;
            }

            return "es-MX";
        }
        

        
        private Button GetLanguageButton(string languageCode)
        {
            switch (languageCode)
            {
                case "en-US":
                    return btnEnglish;
                case "pt-BR":
                    return btnPortuguese;
                case "es-MX":
                default:
                    return btnSpanish;
            }
        }
        
    }
}
