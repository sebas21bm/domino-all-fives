using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>Presentation retained for a future remote use case implementation.</summary>
    public class CreateGameViewModel : ViewModelBase
    {
        public bool Is100PointsSelected { get; set; }
        public bool Is150PointsSelected { get; set; }
        public bool Is200PointsSelected { get; set; }
        public bool Is250PointsSelected { get; set; }
        public bool Is2PlayersSelected { get; set; }
        public bool Is3PlayersSelected { get; set; }
        public bool Is4PlayersSelected { get; set; }
        public RelayCommand GoBackCommand { get; }
        public RelayCommand CreateGameCommand { get; }

        public CreateGameViewModel(IFrameNavigationService navigationService)
        {
            GoBackCommand = new RelayCommand(() => navigationService.GoBack());
            CreateGameCommand = new RelayCommand(() => { }, () => false);
        }
    }
}
