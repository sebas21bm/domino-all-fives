using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
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
        private const string DefaultProfilePicture =
            "pack://application:,,,/Assets/Images/ProfilePictures/defaultProfilePic.png";

        private readonly IFrameNavigationService _navigationService;
        private readonly IRankingService _rankingService;
        private readonly PlayerSession _playerSession;

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
            PlayerSession playerSession)
        {
            _navigationService = navigationService
                ?? throw new ArgumentNullException(nameof(navigationService));

            _rankingService = rankingService
                ?? throw new ArgumentNullException(nameof(rankingService));

            _playerSession = playerSession
                ?? throw new ArgumentNullException(nameof(playerSession));

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
                        Avatar = GetProfilePicture(entry.ProfilePictureData)
                    });
            }

            CurrentPlayerRank = rankingResult.CurrentPlayerRank > 0
                ? rankingResult.CurrentPlayerRank.ToString()
                : "-";
        }

        private BitmapImage GetProfilePicture(byte[] profilePictureData)
        {
            if (profilePictureData == null || profilePictureData.Length == 0)
            {
                return GetDefaultProfilePicture();
            }

            try
            {
                using (MemoryStream imageStream = new MemoryStream(profilePictureData))
                {
                    BitmapImage image = new BitmapImage();

                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = imageStream;
                    image.EndInit();
                    image.Freeze();

                    return image;
                }
            }
            catch (Exception ex) when (ex is ArgumentException ||
                                       ex is InvalidOperationException ||
                                       ex is NotSupportedException)
            {
                return GetDefaultProfilePicture();
            }
        }

        private BitmapImage GetDefaultProfilePicture()
        {
            BitmapImage defaultImage = new BitmapImage();

            defaultImage.BeginInit();
            defaultImage.CacheOption = BitmapCacheOption.OnLoad;
            defaultImage.UriSource = new Uri(DefaultProfilePicture, UriKind.Absolute);
            defaultImage.EndInit();
            defaultImage.Freeze();

            return defaultImage;
        }
    }
}