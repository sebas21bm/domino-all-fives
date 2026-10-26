using System.Collections.ObjectModel;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class JoinGameViewModel : ViewModelBase
    {
        public bool IsJoinByCodeMode { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public bool HasError { get; set; }
        public ObservableCollection<GameInvitationRecord> Invitations { get; } = new ObservableCollection<GameInvitationRecord>();
        public RelayCommand GoBackCommand { get; }
        public RelayCommand SwitchModeCommand { get; }
        public RelayCommand JoinRoomCommand { get; }
        public RelayCommand AcceptInvitationCommand { get; }

        public JoinGameViewModel(IFrameNavigationService navigationService)
        {
            GoBackCommand = new RelayCommand(() => navigationService.GoBack());
            SwitchModeCommand = new RelayCommand(() => { }, () => false);
            JoinRoomCommand = new RelayCommand(() => { }, () => false);
            AcceptInvitationCommand = new RelayCommand(() => { }, () => false);
        }
    }
}
