using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DominoAllFives.Client.WPF.Localization;

namespace DominoAllFives.Client.WPF.Views
{
    public partial class Settings : Window
    {
        private string _selectedLanguageCode;

        public Settings()
        {
            InitializeComponent();

            _selectedLanguageCode = LanguageManager.CurrentLanguageCode;

            SetSelectedLanguageButton(
                GetLanguageButton(_selectedLanguageCode));
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

        private void LanguageOptionButtonClick(object sender,
            RoutedEventArgs eventArgs)
        {
            if (sender is not Button selectedButton)
            {
                return;
            }

            _selectedLanguageCode =
                GetLanguageCode(selectedButton);

            SetSelectedLanguageButton(selectedButton);
        }

        private void SaveLanguageButtonClick(object sender,
            RoutedEventArgs eventArgs)
        {
            bool languageSaved =
                LanguageManager.SaveLanguage(_selectedLanguageCode);

            if (!languageSaved)
            {
                return;
            }

            Settings settingsWindow = new Settings();
            settingsWindow.ShowLanguageSection();
            settingsWindow.Show();

            Close();
        }

        private void LogoutButtonClick(object sender,
            RoutedEventArgs eventArgs)
        {
            //HomePage homePageWindow = new HomePage();
            //homePageWindow.Show();
            //Close();
        }

        private void BackButtonClick(object sender,
            RoutedEventArgs eventArgs)
        {
            MainMenu mainMenuWindow = new MainMenu();
            mainMenuWindow.Show();
            Close();
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
            btnAccount.Style =
                (Style)FindResource("NavigationTabButton");

            btnLanguage.Style =
                (Style)FindResource("NavigationTabButton");

            selectedButton.Style =
                (Style)FindResource("SelectedNavigationTabButton");
        }

        private void SetSelectedLanguageButton(
            Button selectedButton)
        {
            Brush unselectedForeground =
                (Brush)FindResource("DisabledGrayBrush");

            Brush selectedForeground =
                (Brush)FindResource("CreamLightBrush");

            btnEnglish.Foreground = unselectedForeground;
            btnSpanish.Foreground = unselectedForeground;
            btnPortuguese.Foreground = unselectedForeground;

            selectedButton.Foreground = selectedForeground;
        }

        private string GetLanguageCode(Button selectedButton)
        {
            if (selectedButton == btnEnglish)
            {
                return LanguageManager.EnglishLanguageCode;
            }

            if (selectedButton == btnPortuguese)
            {
                return LanguageManager.PortugueseLanguageCode;
            }

            return LanguageManager.SpanishLanguageCode;
        }

        private Button GetLanguageButton(string languageCode)
        {
            if (languageCode ==
                LanguageManager.EnglishLanguageCode)
            {
                return btnEnglish;
            }

            if (languageCode ==
                LanguageManager.PortugueseLanguageCode)
            {
                return btnPortuguese;
            }

            return btnSpanish;
        }
    }
}
