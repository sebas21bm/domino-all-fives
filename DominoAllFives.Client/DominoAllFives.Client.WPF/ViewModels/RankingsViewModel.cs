using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.ProfilePicture;
using DominoAllFives.Client.WPF.ViewModels.Base;
using DominoAllFives.Contracts.DTOs;
using DominoAllFives.Contracts.Services;

namespace DominoAllFives.Client.WPF.ViewModels
{
    /// <summary>
    /// Provides ranking information for the rankings interface.
    /// </summary>
    public class RankingsViewModel : ViewModelBase
    {
        private readonly IFrameNavigationService _navigationService;
        private readonly IRankingService _rankingService;
        private readonly PlayerSession _playerSession;
        private readonly ProfilePictureImageService _profilePictureImageService;

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
            set => SetProperty(ref _currentPlayerRank, value);
        }

        public RelayCommand GoBackCommand { get; }

        public RankingsViewModel(
            IFrameNavigationService navigationService,
            IRankingService rankingService,
            PlayerSession playerSession,
            ProfilePictureImageService profilePictureImageService)
        {
            _navigationService = navigationService
                ?? throw new ArgumentNullException(nameof(navigationService));

            _rankingService = rankingService
                ?? throw new ArgumentNullException(nameof(rankingService));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));

            _profilePictureImageService = profilePictureImageService
                ?? throw new ArgumentNullException(nameof(profilePictureImageService));

            _leaderboard = new ObservableCollection<PlayerRankingRecord>();
            _currentPlayerRank = string.Empty;

            GoBackCommand = new RelayCommand(_ => _navigationService.GoBack());

            LoadRanking();
        }

        private void LoadRanking()
        {
            if (!_playerSession.PlayerId.HasValue)
            {
                return;
            }

            RankingResultDto rankingResult =
                _rankingService.GetTopRanking(_playerSession.PlayerId.Value);

            if (rankingResult == null)
            {
                return;
            }

            Leaderboard.Clear();

            foreach (RankingEntryDto entry in rankingResult.TopPlayers)
            {
                Leaderboard.Add(
                    new PlayerRankingRecord
                    {
                        Rank = entry.Rank,
                        Username = entry.Username,
                        StatisticValue = entry.GamesWon,
                        Avatar =
                            _profilePictureImageService.GetProfilePicture(entry.ProfilePictureData)
                    });
            }

            CurrentPlayerRank = rankingResult.CurrentPlayerRank > 0
                ? rankingResult.CurrentPlayerRank.ToString()
                : "-";
        }
    }
}