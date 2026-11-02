using System.Collections.ObjectModel;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class FriendsViewModel : ViewModelBase
    {
        public ObservableCollection<string> OnlineFriends { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> OfflineFriends { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> SearchResults { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> SentRequests { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> ReceivedRequests { get; } = new ObservableCollection<string>();
        public bool IsFriendsListVisible { get; set; }
        public bool IsAddFriendsVisible { get; set; }
        public bool IsFriendRequestsVisible { get; set; }
        public RelayCommand ShowFriendsListCommand { get; }
        public RelayCommand ShowAddFriendsCommand { get; }
        public RelayCommand ShowFriendRequestsCommand { get; }
        public RelayCommand GoBackCommand { get; }

        public FriendsViewModel(IFrameNavigationService navigationService)
        {
            ShowFriendsListCommand = new RelayCommand(() => { }, () => false);
            ShowAddFriendsCommand = new RelayCommand(() => { }, () => false);
            ShowFriendRequestsCommand = new RelayCommand(() => { }, () => false);
            GoBackCommand = new RelayCommand(() => navigationService.GoBack());
        }
    }
}
