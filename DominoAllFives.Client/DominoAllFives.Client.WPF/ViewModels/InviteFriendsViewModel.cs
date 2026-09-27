using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

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

            LoadMockFriends();
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

        private void LoadMockFriends()
        {
            OnlineFriends.Add(new FriendRecord
            {
                Username = "pepe123",
                AvatarPath = "pack://application:,,,/Assets/Images/Avatars/avatar1.png",
                IsInvited = true
            });

            OnlineFriends.Add(new FriendRecord
            {
                Username = "sebas_wow",
                AvatarPath = "pack://application:,,,/Assets/Images/Avatars/avatar3.png",
                IsInvited = false
            });

            OfflineFriends.Add(new FriendRecord
            {
                Username = "loulou",
                AvatarPath = "pack://application:,,,/Assets/Images/Avatars/avatar5.png",
                IsInvited = false
            });
        }
    }
}
