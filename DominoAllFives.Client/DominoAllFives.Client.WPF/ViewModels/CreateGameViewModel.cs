using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public class CreateGameViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;

        private bool _is100PointsSelected = true;
        public bool Is100PointsSelected
        {
            get => _is100PointsSelected;
            set => SetProperty(ref _is100PointsSelected, value);
        }

        private bool _is150PointsSelected;
        public bool Is150PointsSelected
        {
            get => _is150PointsSelected;
            set => SetProperty(ref _is150PointsSelected, value);
        }

        private bool _is200PointsSelected;
        public bool Is200PointsSelected
        {
            get => _is200PointsSelected;
            set => SetProperty(ref _is200PointsSelected, value);
        }

        private bool _is250PointsSelected;
        public bool Is250PointsSelected
        {
            get => _is250PointsSelected;
            set => SetProperty(ref _is250PointsSelected, value);
        }

        private bool _is2PlayersSelected = true;
        public bool Is2PlayersSelected
        {
            get => _is2PlayersSelected;
            set => SetProperty(ref _is2PlayersSelected, value);
        }

        private bool _is3PlayersSelected;
        public bool Is3PlayersSelected
        {
            get => _is3PlayersSelected;
            set => SetProperty(ref _is3PlayersSelected, value);
        }

        private bool _is4PlayersSelected;
        public bool Is4PlayersSelected
        {
            get => _is4PlayersSelected;
            set => SetProperty(ref _is4PlayersSelected, value);
        }

        public RelayCommand GoBackCommand { get; }
        public RelayCommand CreateGameCommand { get; }

        public CreateGameViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            GoBackCommand = new RelayCommand(ExecuteGoBack);
            CreateGameCommand = new RelayCommand(ExecuteCreateGame);
        }

        private void ExecuteGoBack()
        {
            _navigationService.GoBack();
        }

        private void ExecuteCreateGame()
        {
            int selectedPoints = GetSelectedPoints();
            int selectedPlayers = GetSelectedPlayers();

        }

        private int GetSelectedPoints()
        {
            if (Is100PointsSelected)
            {
                return 100;
            }
            if (Is150PointsSelected)
            {
                return 150;
            }
            if (Is200PointsSelected)
            {
                return 200;
            }
            if (Is250PointsSelected)
            {
                return 250;
            }
            return 100;
        }

        private int GetSelectedPlayers()
        {
            if (Is2PlayersSelected)
            {
                return 2;
            }
            if (Is3PlayersSelected)
            {
                return 3;
            }
            if (Is4PlayersSelected)
            {
                return 4;
            }
            return 2; 
        }
    }
}
