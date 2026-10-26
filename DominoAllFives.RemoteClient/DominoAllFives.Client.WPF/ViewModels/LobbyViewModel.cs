using System.Collections.ObjectModel;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class LobbyViewModel : ViewModelBase
    {
        public ViewModelBase CurrentModal { get; set; }
        public bool IsModalVisible { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public bool IsHost { get; set; }
        public ObservableCollection<LobbyPlayerRecord> Players { get; } = new ObservableCollection<LobbyPlayerRecord>();
        public RelayCommand GoBackCommand { get; }
        public RelayCommand InviteFriendsCommand { get; }
        public RelayCommand StartGameCommand { get; }
        public RelayCommand ViewProfileCommand { get; }
        public RelayCommand KickPlayerCommand { get; }

        public LobbyViewModel(IFrameNavigationService navigationService)
        {
            GoBackCommand = new RelayCommand(() => navigationService.GoBack());
            InviteFriendsCommand = new RelayCommand(() => { }, () => false);
            StartGameCommand = new RelayCommand(() => { }, () => false);
            ViewProfileCommand = new RelayCommand(() => { }, () => false);
            KickPlayerCommand = new RelayCommand(() => { }, () => false);
        }
    }
}
