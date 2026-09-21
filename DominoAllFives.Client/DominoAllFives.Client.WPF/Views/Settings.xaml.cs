using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DominoAllFives.Client.WPF.Views
{
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
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
            if (sender is Button button)
            {
                SetSelectedLanguageButton(button);
            }
        }

        private void SaveLanguageButtonClick(object sender,
            RoutedEventArgs eventArgs)
        {
            //TODO
        }

        private void LogoutButtonClick(object sender, RoutedEventArgs eventArgs)
        {
            //TODO: Add logout logic and navigating to the HomePage.   
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

        private void SetSelectedNavigationButton(Button selectedButton)
        {
            btnAccount.Style = 
                (Style)FindResource("NavigationTabButton");
            btnLanguage.Style = 
                (Style)FindResource("NavigationTabButton");
            selectedButton.Style =
                (Style)FindResource("SelectedNavigationTabButton");
        }

        private void SetSelectedLanguageButton(Button selectedButton)
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
    }
}
