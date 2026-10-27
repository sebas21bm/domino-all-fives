using System;
using System.Collections.ObjectModel;

using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class InviteFriendsViewModel : ViewModelBase
    {
        private readonly Action _onClose;

        private string _roomCode;

        public string RoomCode
        {
            get => _roomCode;
            set => SetProperty(ref _roomCode, value);
        }

        public ObservableCollection<FriendRecord> OnlineFriends { get; }
        public ObservableCollection<FriendRecord> OfflineFriends { get; }

        public RelayCommand CloseInviteModalCommand { get; }
        public RelayCommand SendInviteCommand { get; }


        public InviteFriendsViewModel(Action onClose)
        {
            _onClose = onClose ?? throw new ArgumentNullException(nameof(onClose));

            OnlineFriends = new ObservableCollection<FriendRecord>();
            OfflineFriends = new ObservableCollection<FriendRecord>();

            CloseInviteModalCommand = new RelayCommand(ExecuteCloseInviteModal);
            SendInviteCommand = new RelayCommand(ExecuteSendInvite);
        }

        private void ExecuteCloseInviteModal()
        {
            _onClose?.Invoke();
        }

        private void ExecuteSendInvite(Object parameter)
        {
            if (parameter is FriendRecord friend)
            {
                friend.IsInvited = true;
            }
            
        }
    }
}
