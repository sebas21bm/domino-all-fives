using System;

using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Services;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>
    /// Represents the view model for the profile view,
    /// used for displaying and navigating through the user's profile information.
    /// </summary>
    public class ProfileViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;
        private readonly IAccountService _accountService;
        private readonly IDialogService _dialogService;
        private readonly PlayerSession _playerSession;

        private string _username;
        private byte[] _profilePictureData;
        private int _wins;
        private int _totalPoints;
        private int _gamesPlayed;

        private string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private byte[] ProfilePictureData
        {
            get => _profilePictureData;
            set => SetProperty(ref _profilePictureData, value);
        }

        private int Wins
        {
            get => _wins;
            set => SetProperty(ref _wins, value);
        }

        private int TotalPoints
        {
            get => _totalPoints;
            set => SetProperty(ref _totalPoints, value);
        }

        private int GamesPlayed
        {
            get => _gamesPlayed;
            set => SetProperty(ref _gamesPlayed, value);
        }

        public RelayCommand GoBackCommand { get; }
        public RelayCommand GoToEditProfileCommand { get; }
        public RelayCommand GoToMatchHistoryCommand { get; }

        public ProfileViewModel(
            IFrameNavigationService navigationService,
            IAccountService accountService,
            IDialogService dialogService,
            PlayerSession playerSession)
        {
            _navigationService = navigationService 
                ?? throw new ArgumentNullException(nameof(navigationService));

            _accountService = accountService
                ?? throw new ArgumentNullException(nameof(accountService));

            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));

            GoBackCommand = new RelayCommand(_ => _navigationService.GoBack());
            GoToEditProfileCommand = new RelayCommand(_ => ExecuteGoToEditProfile());
            GoToMatchHistoryCommand = new RelayCommand(_ => ExecuteGoToMatchHistory());

            ShowProfileInformation();
        }

        private void ExecuteGoToEditProfile()
        {
            _navigationService.NavigateTo<EditProfileViewModel>();
        }

        private void ExecuteGoToMatchHistory()
        {
            _navigationService.NavigateTo<MatchHistoryViewModel>();
        }

        private void ShowProfileInformation()
        {
            if (!_playerSession.PlayerId.HasValue)
            {
                ShowRetrieveInformationFailure(
                    RetrieveInformationFailureReason.NotFound);
                return;
            }

            ProfileDto profile =
                _accountService.GetProfile(_playerSession.PlayerId.Value);

            if (profile.FailureReason != RetrieveInformationFailureReason.None)
            {
                ShowRetrieveInformationFailure(profile.FailureReason);
                return;
            }

            Username = profile.Username;
            ProfilePictureData = profile.ProfilePictureData;
            Wins = profile.Wins;
            TotalPoints = profile.TotalPoints;
            GamesPlayed = profile.GamesPlayed;
        }

        private void ShowRetrieveInformationFailure(
            RetrieveInformationFailureReason reason)
        {
            switch (reason)
            {
                case RetrieveInformationFailureReason.NotFound:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgNotFoundTitle",
                        MessageKey = "Global_msgNotFound"
                    });
                    break;
                case RetrieveInformationFailureReason.ServiceUnavailable:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgConnectionErrorTitle",
                        MessageKey = "Global_msgConnectionError"
                    });
                    break;
                default:
                    _dialogService.ShowDialog(new DialogRequest
                    {
                        Type = DialogType.Error,
                        TitleKey = "Global_msgDefaultErrorTitle",
                        MessageKey = "Global_msgDefaultError"
                    });
                    break;
            }
        }



    }
}
