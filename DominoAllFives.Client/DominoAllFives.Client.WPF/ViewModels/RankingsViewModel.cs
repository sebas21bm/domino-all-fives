using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.ServiceModel;
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
    /// <summary>
    /// Provides ranking information for the rankings interface.
    /// </summary>
    public class RankingsViewModel : ViewModelBase
    {
        private IFrameNavigationService _navigationService;
        private IRankingClient _rankingService;
        private IDialogService _dialogService;
        private readonly PlayerSession _playerSession;
        private ProfilePictureImageService _profilePictureImageService;
        private ObservableCollection<PlayerRankingRecord> _leaderboard;
        private string _currentPlayerRank;
        public ObservableCollection<PlayerRankingRecord> Leaderboard
        {
            get
            {
                return _leaderboard;
            }
            set
            {
                SetProperty(ref _leaderboard, value);
            }
        }
        public string CurrentPlayerRank
        {
            get
            {
                return _currentPlayerRank;
            }
            set
            {
                SetProperty(ref _currentPlayerRank, value);
            }
        }
        public RelayCommand GoBackCommand { get; }

        public RankingsViewModel(PlayerSession playerSession)
        {
            _playerSession = playerSession ?? throw new ArgumentNullException(
                nameof(playerSession));
            _leaderboard = new ObservableCollection<PlayerRankingRecord>();
            _currentPlayerRank = string.Empty;
            GoBackCommand = new RelayCommand(unusedParameter => _navigationService.GoBack());
        }

        public void Initialize(
            IFrameNavigationService navigationService,
            IRankingClient rankingService,
            ProfilePictureImageService profilePictureImageService,
            IDialogService dialogService)
        {
            if (_navigationService != null)
            {
                throw new InvalidOperationException(
                    "RankingsViewModel has already been initialized.");
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            if (rankingService == null)
            {
                throw new ArgumentNullException(nameof(rankingService));
            }

            if (profilePictureImageService == null)
            {
                throw new ArgumentNullException(nameof(profilePictureImageService));
            }

            if (dialogService == null)
            {
                throw new ArgumentNullException(nameof(dialogService));
            }

            _navigationService = navigationService;
            _rankingService = rankingService;
            _profilePictureImageService = profilePictureImageService;
            _dialogService = dialogService;
            LoadRanking();
        }

        private void LoadRanking()
        {
            if (!_playerSession.PlayerId.HasValue)
            {
                return;
            }

            BackgroundJob<int, Ranking> job = new BackgroundJob<int, Ranking>(
                RetrieveRanking,
                _playerSession.PlayerId.Value,
                CompleteRanking,
                HandleRankingFailure);
            BackgroundWork.Start(job);
        }

        private Ranking RetrieveRanking(int playerId)
        {
            List<RankingEntry> topPlayers = _rankingService.GetTopWinsRanking();
            if (topPlayers == null)
            {
                return null;
            }

            int currentPlayerRank = _rankingService.GetWinsRankingPositionById(playerId);
            return new Ranking(topPlayers, currentPlayerRank);
        }

        private void HandleRankingFailure(Exception ex, int playerId)
        {
            ShowRankingFailure(ex);
        }

        private void CompleteRanking(Ranking rankingResult, int playerId)
        {
            if (rankingResult == null)
            {
                return;
            }

            Leaderboard.Clear();
            foreach (RankingEntry entry in rankingResult.TopPlayers)
            {
                Leaderboard.Add(
                    new PlayerRankingRecord {
                        Rank = entry.Rank,
                        Username = entry.Username,
                        StatisticValue = entry.GamesWon,
                        Avatar = _profilePictureImageService.GetProfilePicture(
                    entry.Picture.ImageData) });
            }

            CurrentPlayerRank = rankingResult.CurrentPlayerRank > 0
                ? rankingResult.CurrentPlayerRank.ToString() : "-";
        }

        private void ShowRankingFailure(Exception ex)
        {
            LoggerFactoryProvider.CreateLogger<RankingsViewModel>().LogError(
                ex,
                "Ranking retrieval failed.");
            _dialogService.ShowDialog(
                new DialogRequest {
                    Type = DialogType.Error,
                    TitleKey = "Global_msgConnectionErrorTitle",
                    MessageKey = "Global_msgConnectionError" });
        }
    }
}
