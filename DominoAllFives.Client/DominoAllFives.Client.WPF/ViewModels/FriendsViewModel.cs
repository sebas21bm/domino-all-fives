using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class FriendsViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;

        private bool _isFriendsListVisible;
        private bool _isAddFriendsVisible;
        private bool _isFriendRequestsVisible;

        public bool IsFriendsListVisible
        {
            get => _isFriendsListVisible;
            set => SetProperty(ref _isFriendsListVisible, value);
        }

        public bool IsAddFriendsVisible
        {
            get => _isAddFriendsVisible;
            set => SetProperty(ref _isAddFriendsVisible, value);
        }

        public bool IsFriendRequestsVisible
        {
            get => _isFriendRequestsVisible;
            set => SetProperty(ref _isFriendRequestsVisible, value);
        }

        public RelayCommand ShowFriendsListCommand { get; }
        public RelayCommand ShowAddFriendsCommand { get; }
        public RelayCommand ShowFriendRequestsCommand { get; }
        public RelayCommand GoBackCommand { get; }

        public FriendsViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            _isFriendsListVisible = true;
            _isAddFriendsVisible = false;
            _isFriendRequestsVisible = false;

            ShowFriendsListCommand = new RelayCommand(_ => ExecuteShowFriendsList());
            ShowAddFriendsCommand = new RelayCommand(_ => ExecuteShowAddFriends());
            ShowFriendRequestsCommand = new RelayCommand(_ => ExecuteShowFriendRequests());
            GoBackCommand = new RelayCommand(_ => _navigationService.NavigateTo<MainMenuViewModel>());
        }

        private void ExecuteShowFriendsList()
        {
            IsFriendsListVisible = true;
            IsAddFriendsVisible = false;
            IsFriendRequestsVisible = false;
        }

        private void ExecuteShowAddFriends()
        {
            IsFriendsListVisible = false;
            IsAddFriendsVisible = true;
            IsFriendRequestsVisible = false;
        }

        private void ExecuteShowFriendRequests()
        {
            IsFriendsListVisible = false;
            IsAddFriendsVisible = false;
            IsFriendRequestsVisible = true;
        }
    }
}
