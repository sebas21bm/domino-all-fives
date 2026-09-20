using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class Friends : Window
    {

        public Friends()
        {
            InitializeComponent();
            ShowFriendListSection();
        }

        private void FriendListButtonClick(object sender, RoutedEventArgs eventArgs)
        {
            ShowFriendListSection();
        }

        private void AddFriendsButtonClick(object sender, RoutedEventArgs eventArgs)
        {
            ShowAddFriendsSection();
        }

        private void RequestsButtonClick(object sender, RoutedEventArgs eventArgs)
        {
            ShowFriendRequestsSection();
        }

        private void ShowFriendListSection()
        {
            grdFriendList.Visibility = Visibility.Visible;
            grdAddFriends.Visibility = Visibility.Collapsed;
            grdFriendRequests.Visibility = Visibility.Collapsed;

            SetSelectedNavigationButton(btnFriendList);
        }

        private void ShowAddFriendsSection()
        {
            grdFriendList.Visibility = Visibility.Collapsed;
            grdAddFriends.Visibility = Visibility.Visible;
            grdFriendRequests.Visibility = Visibility.Collapsed;

            SetSelectedNavigationButton(btnAddFriends);
        }

        private void ShowFriendRequestsSection()
        {
            grdFriendList.Visibility = Visibility.Collapsed;
            grdAddFriends.Visibility = Visibility.Collapsed;
            grdFriendRequests.Visibility = Visibility.Visible;

            SetSelectedNavigationButton(btnRequests);
        }

        private void SetSelectedNavigationButton(Button selectedButton)
        {
            btnFriendList.Style =
                (Style)FindResource("NavigationTabButton");

            btnAddFriends.Style =
                (Style)FindResource("NavigationTabButton");

            btnRequests.Style =
                (Style)FindResource("NavigationTabButton");

            selectedButton.Style =
                (Style)FindResource("SelectedNavigationTabButton");
        }
    }
}