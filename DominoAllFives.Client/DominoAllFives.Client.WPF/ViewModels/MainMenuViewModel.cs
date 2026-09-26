using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;

        public RelayCommand GoToProfileCommand { get; }
        public RelayCommand GoToFriendsCommand { get; }
        public RelayCommand GoToSettingsCommand { get; }
        public RelayCommand ExitGameCommand { get; }

        public MainMenuViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            GoToProfileCommand = new RelayCommand(_ => _navigationService.NavigateTo<ProfileViewModel>());
            GoToFriendsCommand = new RelayCommand(_ => _navigationService.NavigateTo<FriendsViewModel>());
            GoToSettingsCommand = new RelayCommand(_ => _navigationService.NavigateTo<GameSettingsViewModel>());
            ExitGameCommand = new RelayCommand(_ => Application.Current.Shutdown());
        }
    }
}
