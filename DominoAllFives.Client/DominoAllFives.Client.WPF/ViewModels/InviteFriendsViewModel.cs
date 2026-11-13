using System;
using System.Collections.ObjectModel;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class InviteFriendsViewModel : ViewModelBase
    {
        public string RoomCode { get; set; } = string.Empty;
        public ObservableCollection<FriendRecord> OnlineFriends { get; } =
            new ObservableCollection<FriendRecord>();
        public ObservableCollection<FriendRecord> OfflineFriends { get; } =
            new ObservableCollection<FriendRecord>();
        public RelayCommand CloseInviteModalCommand { get; }
        public RelayCommand SendInviteCommand { get; }

        public InviteFriendsViewModel(Action onCancel = null)
        {
            CloseInviteModalCommand = new RelayCommand(() => onCancel?.Invoke());
            SendInviteCommand = new RelayCommand(() =>
            {
            }, () => false);
        }
    }
}
