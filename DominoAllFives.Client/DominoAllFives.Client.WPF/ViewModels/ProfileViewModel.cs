using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;

        private string _username;
        private string _profilePicturePath;
        private int _wins;
        private int _totalPoints;
        private int _gamesPlayed;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string ProfilePicturePath
        {
            get => _profilePicturePath;
            set => SetProperty(ref _profilePicturePath, value);
        }

        public int Wins
        {
            get => _wins;
            set => SetProperty(ref _wins, value);
        }

        public int TotalPoints
        {
            get => _totalPoints;
            set => SetProperty(ref _totalPoints, value);
        }

        public int GamesPlayed
        {
            get => _gamesPlayed;
            set => SetProperty(ref _gamesPlayed, value);
        }

        public RelayCommand GoBackCommand { get; }
        public RelayCommand GoToEditProfileCommand { get; }
        public RelayCommand GoToMatchHistoryCommand { get; }

        public ProfileViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            // TODO: Cargar datos reales desde el servicio de sesión WCF
            Username = "nickname31";
            ProfilePicturePath = "/Assets/Images/default_avatar.png";
            Wins = 10;
            TotalPoints = 5800;
            GamesPlayed = 45;

            GoBackCommand = new RelayCommand(_ => _navigationService.NavigateTo<MainMenuViewModel>());
            GoToEditProfileCommand = new RelayCommand(_ => ExecuteGoToEditProfile());
            GoToMatchHistoryCommand = new RelayCommand(_ => ExecuteGoToMatchHistory());
        }

        private void ExecuteGoToEditProfile()
        {
            // TODO: Navegar a EditProfileViewModel cuando se implemente la pantalla de edición (CU-07)
        }

        private void ExecuteGoToMatchHistory()
        {
            // TODO: Navegar a MatchHistoryViewModel cuando se implemente el historial
        }
    }
}
