using System;
using System.Windows.Media.Imaging;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class ProfileViewModel : ViewModelBase
    {
        private IFrameNavigationService _navigationService;
        public string Username { get; set; } = string.Empty;
        public BitmapImage ProfilePicture { get; set; }
        public int Wins { get; set; }
        public int TotalPoints { get; set; }
        public int GamesPlayed { get; set; }
        public RelayCommand GoBackCommand { get; }
        public RelayCommand GoToEditProfileCommand { get; }
        public RelayCommand GoToMatchHistoryCommand { get; }

        public ProfileViewModel()
        {
            GoBackCommand = new RelayCommand(() => _navigationService.GoBack());
            GoToEditProfileCommand = new RelayCommand(() =>
            {
            }, () => false);
            GoToMatchHistoryCommand = new RelayCommand(() =>
            {
            }, () => false);
        }

        public void Initialize(IFrameNavigationService navigationService)
        {
            if (_navigationService != null)
            {
                throw new InvalidOperationException(
                    "ProfileViewModel has already been initialized.");
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            _navigationService = navigationService;
        }
    }
}
