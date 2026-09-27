using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Localization;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class JoinGameViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;

        private bool _isJoinByCodeMode = true;
        public bool IsJoinByCodeMode
        {
            get => _isJoinByCodeMode;
            set => SetProperty(ref _isJoinByCodeMode, value);
        }

        private string _roomCode = string.Empty;
        public string RoomCode
        {
            get => _roomCode;
            set
            {
                if (SetProperty(ref _roomCode, value))
                {
                    if (HasError)
                    {
                        HasError = false;
                        ErrorMessage = string.Empty;
                    }
                }
            }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public ObservableCollection<GameInvitationRecord> Invitations { get; set; }

        public RelayCommand GoBackCommand { get; }
        public RelayCommand SwitchModeCommand { get; }
        public RelayCommand JoinRoomCommand { get; }
        public RelayCommand AcceptInvitationCommand { get; }

        public JoinGameViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            GoBackCommand = new RelayCommand(_ => _navigationService.GoBack());
            SwitchModeCommand = new RelayCommand(ExecuteSwitchMode);
            JoinRoomCommand = new RelayCommand(ExecuteJoinRoom);
            AcceptInvitationCommand = new RelayCommand(ExecuteAcceptInvitation);

            Invitations = new ObservableCollection<GameInvitationRecord>();
        }


        private void ExecuteSwitchMode()
        {
            IsJoinByCodeMode = !IsJoinByCodeMode;

            HasError = false;
            ErrorMessage = string.Empty;
        }

        private void ExecuteJoinRoom()
        {
        }

        private void ExecuteAcceptInvitation(object parameter)
        {
        }
    }
}