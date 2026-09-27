using DominoAllFives.Client.WPF.Commands;
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
using System.Windows.Navigation;

namespace DominoAllFives.Client.WPF.ViewModels
{

    public class RankingsViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;
        private ObservableCollection<PlayerRankingRecord> _leaderboard;
        private string _currentPlayerRank;

        public ObservableCollection<PlayerRankingRecord> Leaderboard
        {
            get => _leaderboard;
            set => SetProperty(ref _leaderboard, value);
        }

        public string CurrentPlayerRank
        {
            get => _currentPlayerRank;
            set
            {
                _currentPlayerRank = value;
                OnPropertyChanged(nameof(CurrentPlayerRank));
            }
        }

        public RelayCommand GoBackCommand { get; }
        public RankingsViewModel(IFrameNavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            _leaderboard = new ObservableCollection<PlayerRankingRecord>();
            _currentPlayerRank = string.Empty;

            GoBackCommand = new RelayCommand(_ => _navigationService.GoBack());
        }
        
    }
}
