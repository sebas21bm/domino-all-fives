using System;
using System.Windows.Media.Imaging;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFivesService;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Loads the authenticated player's profile and provides profile navigation.</summary>
    public class ProfileViewModel : ViewModelBase
    {
        private IFrameNavigationService _navigationService;
        private IProfileClient _profileClient;
        private IDialogService _dialogService;
        private ProfilePictureImageService _imageService;
        private readonly PlayerSession _playerSession;
        private ILogger<ProfileViewModel> _logger;
        private string _username;
        private BitmapImage _profilePicture;
        private int _wins;
        private int _totalPoints;
        private int _gamesPlayed;
        private bool _isBusy;
        private bool _isLoaded;

        public string Username
        {
            get
            {
                return _username;
            }
            private set
            {
                SetProperty(ref _username, value);
            }
        }

        public BitmapImage ProfilePicture
        {
            get
            {
                return _profilePicture;
            }
            private set
            {
                SetProperty(ref _profilePicture, value);
            }
        }

        public int Wins
        {
            get
            {
                return _wins;
            }
            private set
            {
                SetProperty(ref _wins, value);
            }
        }

        public int TotalPoints
        {
            get
            {
                return _totalPoints;
            }
            private set
            {
                SetProperty(ref _totalPoints, value);
            }
        }

        public int GamesPlayed
        {
            get
            {
                return _gamesPlayed;
            }
            private set
            {
                SetProperty(ref _gamesPlayed, value);
            }
        }

        public RelayCommand GoBackCommand { get; }
        public RelayCommand GoToEditProfileCommand { get; }
        public RelayCommand GoToMatchHistoryCommand { get; }

        /// <summary>Creates profile presentation state and commands without loading data.</summary>
        /// <param name = "playerSession">The current local player session.</param>
        public ProfileViewModel(PlayerSession playerSession)
        {
            _playerSession = playerSession;
            _username = string.Empty;
            GoBackCommand = new RelayCommand(GoBack, CanGoBack);
            GoToEditProfileCommand = new RelayCommand(GoToEditProfile, CanUseProfile);
            GoToMatchHistoryCommand = new RelayCommand(GoToMatchHistory, CanUseProfile);
        }

        /// <summary>Configures diagnostics before initializing the profile services.</summary>
        /// <param name = "logger">The profile logger.</param>
        public void ConfigureLogger(ILogger<ProfileViewModel> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Injects profile services and starts loading after configuration is complete.
        /// </summary>
        /// <param name = "navigationService">The application navigation service.</param>
        /// <param name = "profileClient">The remote profile client.</param>
        /// <param name = "imageService">The profile picture presentation service.</param>
        /// <param name = "dialogService">The localized dialog service.</param>
        public void Initialize(
            IFrameNavigationService navigationService,
            IProfileClient profileClient,
            ProfilePictureImageService imageService,
            IDialogService dialogService)
        {
            _navigationService = navigationService;
            _imageService = imageService;
            _dialogService = dialogService;
            _profileClient = profileClient;
            SetBusy(false);
            LoadProfile();
        }

        private void LoadProfile()
        {
            if (!_playerSession.HasServerSession)
            {
                ShowFailure(ProfileIssue.PlayerNotFound);
                return;
            }

            BackgroundJob<string, PlayerProfile> job = new BackgroundJob<string, PlayerProfile>(
                _profileClient.GetMyProfile,
                _playerSession.SessionKey,
                CompleteProfile,
                HandleProfileFailure);
            SetBusy(true);
            BackgroundWork.Start(job);
        }

        private void CompleteProfile(PlayerProfile profile, string sessionKey)
        {
            SetBusy(false);
            if (_playerSession.SessionKey != sessionKey)
            {
                return;
            }

            if (profile == null || profile.Statistics == null || profile.Picture == null
                || string.IsNullOrWhiteSpace(profile.Username))
            {
                _logger.LogError("Profile service returned an incomplete profile.");
                ShowFailure();
                return;
            }

            Username = profile.Username;
            ProfilePicture = _imageService.GetProfilePicture(profile.Picture.ImageData);
            Wins = profile.Statistics.GamesWon;
            TotalPoints = profile.Statistics.TotalPointsScored;
            GamesPlayed = profile.Statistics.GamesPlayed;
            _isLoaded = true;
            GoToEditProfileCommand.RaiseCanExecuteChanged();
            GoToMatchHistoryCommand.RaiseCanExecuteChanged();
        }

        private void HandleProfileFailure(Exception ex, string sessionKey)
        {
            SetBusy(false);
            if (_playerSession.SessionKey != sessionKey)
            {
                return;
            }

            ProfileFailureDialogs.HandleRemoteFailure(ex, _logger, "Profile query", ShowFailure);
        }

        private bool CanGoBack()
        {
            return _profileClient != null && !_isBusy;
        }

        private bool CanUseProfile()
        {
            return _profileClient != null && !_isBusy && _isLoaded;
        }

        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;
            GoBackCommand.RaiseCanExecuteChanged();
            GoToEditProfileCommand.RaiseCanExecuteChanged();
            GoToMatchHistoryCommand.RaiseCanExecuteChanged();
        }

        private void GoBack()
        {
            if (CanGoBack())
            {
                _navigationService.GoBackToRefresh<MainMenuViewModel>();
            }
        }

        private void GoToEditProfile()
        {
            if (CanUseProfile())
            {
                _navigationService.NavigateTo<EditProfileViewModel>();
            }
        }

        private void GoToMatchHistory()
        {
            if (CanUseProfile())
            {
                _navigationService.NavigateTo<MatchHistoryViewModel>();
            }
        }

        private void ShowFailure(ProfileIssue? issue = null, bool connectionFailure = false)
        {
            DialogRequest dialog = issue == ProfileIssue.PlayerNotFound
                ? ProfileFailureDialogs.ProfileNotFound()
                : FailureDialogs.ForTechnicalFailure(!issue.HasValue && connectionFailure);
            _dialogService.ShowDialog(dialog);
        }
    }
}
