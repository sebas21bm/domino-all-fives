using System;
using System.Windows.Media.Imaging;

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
        private readonly ProfilePictureImageService _profilePictureImageService;
        private readonly PlayerSession _playerSession;

        private string _username;
        private BitmapImage _profilePicture;
        private int _wins;
        private int _totalPoints;
        private int _gamesPlayed;

        public string Username
        {
            get => _username;
            private set => SetProperty(ref _username, value);
        }

        public BitmapImage ProfilePicture
        {
            get => _profilePicture;
            private set => SetProperty(ref _profilePicture, value);
        }

        public int Wins
        {
            get => _wins;
            private set => SetProperty(ref _wins, value);
        }

        public int TotalPoints
        {
            get => _totalPoints;
            private set => SetProperty(ref _totalPoints, value);
        }

        public int GamesPlayed
        {
            get => _gamesPlayed;
            private set => SetProperty(ref _gamesPlayed, value);
        }

        public RelayCommand GoBackCommand { get; }

        public RelayCommand GoToEditProfileCommand { get; }

        public RelayCommand GoToMatchHistoryCommand { get; }

        public ProfileViewModel(
            IFrameNavigationService navigationService,
            IAccountService accountService,
            IDialogService dialogService,
            PlayerSession playerSession,
            ProfilePictureImageService profilePictureImageService)
        {
            _navigationService = navigationService
                ?? throw new ArgumentNullException(nameof(navigationService));

            _accountService = accountService
                ?? throw new ArgumentNullException(nameof(accountService));

            _dialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));

            _profilePictureImageService = profilePictureImageService
                ?? throw new ArgumentNullException(nameof(profilePictureImageService));

            GoBackCommand =
                new RelayCommand(
                    _ => _navigationService.GoBack());

            GoToEditProfileCommand =
                new RelayCommand(
                    _ => ExecuteGoToEditProfile());

            GoToMatchHistoryCommand =
                new RelayCommand(
                    _ => ExecuteGoToMatchHistory());

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

            if (profile.FailureReason !=
                RetrieveInformationFailureReason.None)
            {
                ShowRetrieveInformationFailure(
                    profile.FailureReason);

                return;
            }

            Username = profile.Username;
            ProfilePicture =
                _profilePictureImageService.GetProfilePicture(profile.ProfilePictureData);
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
                    _dialogService.ShowDialog(
                        new DialogRequest
                        {
                            Type = DialogType.Error,
                            TitleKey = "Global_msgNotFoundTitle",
                            MessageKey = "Global_msgNotFound"
                        });
                    break;

                case RetrieveInformationFailureReason.ServiceUnavailable:
                    _dialogService.ShowDialog(
                        new DialogRequest
                        {
                            Type = DialogType.Error,
                            TitleKey = "Global_msgConnectionErrorTitle",
                            MessageKey = "Global_msgConnectionError"
                        });
                    break;

                default:
                    _dialogService.ShowDialog(
                        new DialogRequest
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