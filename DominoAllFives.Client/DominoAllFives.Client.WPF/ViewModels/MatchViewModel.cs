using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows.Threading;
using DominoAllFives.Client.WPF.Commands;
using DominoAllFives.Client.WPF.Models;
using DominoAllFives.Client.WPF.Services;
using DominoAllFives.Client.WPF.Services.Threading;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using DominoAllFives.Client.WPF.Services.Wcf.Services;
using DominoAllFives.Client.WPF.ViewModels.Base;
using MatchService;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.ViewModels
{
    public sealed class MatchViewModel : ViewModelBase, IDisposable
    {
        private const int SynchronizationAttempts = 8;
        private const double InitialBoardWidth = 400;
        private const double InitialBoardHeight = 300;
        private const double BoardMargin = 96;
        private readonly IFrameNavigationService _navigation;
        private readonly MatchNavigation _context;
        private readonly IMatchClient _client;
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private readonly ILogger<MatchViewModel> _logger =
            LoggerFactoryProvider.CreateLogger<MatchViewModel>();
        private MatchView _pendingView;
        private PlayerHand _pendingHand;
        private MatchView _match;
        private DominoTile _selectedTile;
        private BoardEnd _selectedEnd;
        private bool _isBusy;
        private bool _disposed;
        private bool _opened;
        private string _status;
        private bool _isClientDisposalStarted;
        public List<DominoTile> Hand { get; private set; } = new List<DominoTile>();
        public List<TilePosition> BoardTiles { get; private set; } = new List<TilePosition>();
        public double BoardWidth { get; private set; } = InitialBoardWidth;
        public double BoardHeight { get; private set; } = InitialBoardHeight;
        public List<string> Scores { get; private set; } = new List<string>();
        public List<MatchPlayerRecord> Players { get; private set; } =
            new List<MatchPlayerRecord>();
        public List<MatchPlayerRecord> Opponents { get; private set; } =
            new List<MatchPlayerRecord>();

        public int TargetPoints
        {
            get
            {
                return _context.TargetPoints;
            }
        }

        public List<string> RevealedHands { get; private set; } = new List<string>();
        public List<EndOption> Ends { get; } = Enum.GetValues(typeof(BoardEnd)).Cast<BoardEnd>()
            .Select(end => new EndOption { End = end })
            .ToList();

        public string Status
        {
            get
            {
                return _status;
            }

            private set
            {
                SetProperty(ref _status, value);
            }
        }

        public string TurnText { get; private set; }
        public string RoundText { get; private set; }
        public string AwardedPointsText { get; private set; } = string.Empty;
        public string ScoreMultiplierText { get; private set; } = string.Empty;
        public bool HasScoredPlay { get; private set; }

        public DominoTile SelectedTile
        {
            get
            {
                return _selectedTile;
            }

            set
            {
                SetProperty(ref _selectedTile, value);
                RaiseCommands();
            }
        }

        public BoardEnd SelectedEnd
        {
            get
            {
                return _selectedEnd;
            }

            set
            {
                SetProperty(ref _selectedEnd, value);
                RaiseCommands();
            }
        }

        public RelayCommand PlayCommand { get; }
        public RelayCommand DrawCommand { get; }
        public RelayCommand PassCommand { get; }
        public RelayCommand RefreshCommand { get; }
        public RelayCommand LeaveCommand { get; }

        public MatchViewModel(IFrameNavigationService navigation,
            MatchNavigation context) : this(navigation, context, new MatchRemoteService())
        {
        }

        public MatchViewModel(IFrameNavigationService navigation, MatchNavigation context,
            IMatchClient client)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _client.MatchChanged += OnMatchChanged;
            _client.HandChanged += OnHandChanged;
            PlayCommand = new RelayCommand(Play, CanPlay);
            DrawCommand = new RelayCommand(Draw, CanDraw);
            PassCommand = new RelayCommand(Pass, CanPass);
            RefreshCommand = new RelayCommand(Refresh, CanRefresh);
            LeaveCommand = new RelayCommand(Leave, () => !_isBusy && !_disposed);
        }

        public void Initialize()
        {
            if (_opened || _isBusy || _disposed)
            {
                return;
            }

            StartOperation(OpenAndSynchronize);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _client.MatchChanged -= OnMatchChanged;
            _client.HandChanged -= OnHandChanged;
            RaiseCommands();
            if (!_isBusy)
            {
                BeginClientDisposal();
            }
        }

        private bool IsOwnTurn()
        {
            return !_disposed && !_isBusy && _match != null
                && _match.Phase == MatchPhase.InProgress
                && _match.CurrentPlayerId == _context.PlayerSession.PlayerId;
        }

        private bool CanPlay()
        {
            return IsOwnTurn() && SelectedTile != null && IsPlayable(SelectedTile, SelectedEnd);
        }

        private bool IsPlayable(DominoTile tile, BoardEnd end)
        {
            if (_match.Board.Tiles.Count == 0)
            {
                return end == BoardEnd.Left && tile.TileId == _match.OpeningTileId;
            }

            return _match.Board.Ends.Any(target => target.End == end && !target.IsBlocked && (
                tile.FirstValue == target.Value || tile.SecondValue == target.Value));
        }

        private bool HasMove()
        {
            return Hand.Any(tile => Ends.Any(end => IsPlayable(tile, end.End)));
        }

        private bool CanDraw()
        {
            return IsOwnTurn() && _match.StockCount > 0 && !HasMove();
        }

        private bool CanPass()
        {
            return IsOwnTurn() && _match.StockCount == 0 && !HasMove();
        }

        private bool CanRefresh()
        {
            return !_isBusy && !_disposed;
        }

        private void Play()
        {
            if (!CanPlay())
            {
                return;
            }

            DominoTile tile = new DominoTile
            {
                TileId = SelectedTile.TileId,
                FirstValue = SelectedTile.FirstValue,
                SecondValue = SelectedTile.SecondValue,
                SpecialPower = SelectedTile.SpecialPower
            };
            BoardEnd end = SelectedEnd;
            long version = _match.Version;
            StartOperation(() => PlayAndSynchronize(tile, end, version));
        }

        private void Draw()
        {
            if (!CanDraw())
            {
                return;
            }

            long version = _match.Version;
            StartOperation(() => DrawAndSynchronize(version));
        }

        private void Pass()
        {
            if (!CanPass())
            {
                return;
            }

            long version = _match.Version;
            StartOperation(() => PassAndSynchronize(version));
        }

        private void Refresh()
        {
            StartOperation(OpenAndSynchronize);
        }

        private void Leave()
        {
            StartOperation(LeaveMatch, NavigateToMenu);
        }

        private void NavigateToMenu()
        {
            _navigation.NavigateAsRoot<MainMenuViewModel>();
        }

        private void Synchronize()
        {
            for (int attempt = 0; attempt < SynchronizationAttempts; attempt++)
            {
                MatchView view = _client.GetMatch(_context.MatchId);
                PlayerHand hand = _client.GetHand(_context.MatchId);
                OnMatchChanged(view);
                OnHandChanged(hand);
                if (view.Version == hand.Version)
                {
                    return;
                }
            }
        }

        private void StartOperation(Action action, Action onCompleted = null)
        {
            if (_disposed || _isBusy)
            {
                return;
            }

            _isBusy = true;
            RaiseCommands();
            BackgroundJob<Action, bool> job = new BackgroundJob<Action, bool>(
                ExecuteOperation, action,
                (result, state) => OnOperationCompleted(onCompleted), OnOperationFailed);
            BackgroundWork.Start(job);
        }

        private static bool ExecuteOperation(Action action)
        {
            action();
            return true;
        }

        private void OnOperationCompleted(Action onCompleted)
        {
            _isBusy = false;
            if (_disposed)
            {
                BeginClientDisposal();
                return;
            }

            RaiseCommands();
            onCompleted?.Invoke();
        }

        private void OnOperationFailed(Exception ex, Action action)
        {
            _isBusy = false;
            if (_disposed)
            {
                _logger.LogWarning(ex, "Match operation failed after the view was disposed.");
                BeginClientDisposal();
                return;
            }

            FaultException<MatchIssue> fault = ex as FaultException<MatchIssue>;
            if (fault != null)
            {
                Status = GetText("Match_msgRejected");
                _logger.LogWarning(ex, "Match operation was rejected.");
                if (fault.Detail == MatchIssue.StaleVersion && action != (Action)Synchronize)
                {
                    StartOperation(Synchronize);
                    return;
                }
            }
            else if (ex is CommunicationException || ex is TimeoutException)
            {
                ReportConnectionFailure(ex);
            }
            else
            {
                Status = GetText("Match_msgOperationFailed");
                _logger.LogError(ex, "Unexpected match operation failure.");
            }

            RaiseCommands();
        }

        private void BeginClientDisposal()
        {
            if (_isClientDisposalStarted)
            {
                return;
            }

            _isClientDisposalStarted = true;
            BackgroundJob<IMatchClient, bool> job = new BackgroundJob<IMatchClient, bool>(
                DisposeClientOnWorker, _client, OnClientDisposed, OnClientDisposalFailed);
            BackgroundWork.Start(job);
        }

        private static bool DisposeClientOnWorker(IMatchClient client)
        {
            client.Dispose();
            return true;
        }

        private void OnClientDisposed(bool result, IMatchClient client)
        {
            // Disposal has no UI continuation because this view is no longer active.
        }

        private void OnClientDisposalFailed(Exception ex, IMatchClient client)
        {
            _logger.LogWarning(ex, "Match connection could not be closed cleanly.");
        }

        private void OpenAndSynchronize()
        {
            OnMatchChanged(_client.Open(_context.PlayerSession.SessionKey, _context.MatchId));
            _opened = true;
            Synchronize();
        }

        private void PlayAndSynchronize(DominoTile tile, BoardEnd end, long version)
        {
            OnMatchChanged(_client.Play(_context.MatchId, tile, end, version));
            Synchronize();
        }

        private void DrawAndSynchronize(long version)
        {
            OnMatchChanged(_client.Draw(_context.MatchId, version));
            Synchronize();
        }

        private void PassAndSynchronize(long version)
        {
            OnMatchChanged(_client.Pass(_context.MatchId, version));
            Synchronize();
        }

        private void LeaveMatch()
        {
            if (_opened)
            {
                _client.Leave(_context.MatchId);
            }
        }

        private void ReportConnectionFailure(Exception ex)
        {
            Status = GetText("Match_msgConnectionFailed");
            _logger.LogWarning(ex, "Match connection failed.");
        }

        private void OnMatchChanged(MatchView view)
        {
            Dispatch(() => AcceptView(view));
        }

        private void AcceptView(MatchView view)
        {
            if (view == null || view.MatchId != _context.MatchId)
            {
                return;
            }

            if (_pendingView == null || view.Version >= _pendingView.Version)
            {
                _pendingView = view;
            }

            ApplySnapshot();
        }

        private void OnHandChanged(PlayerHand hand)
        {
            Dispatch(() => AcceptHand(hand));
        }

        private void AcceptHand(PlayerHand hand)
        {
            if (hand == null || hand.MatchId != _context.MatchId
                || hand.PlayerId != _context.PlayerSession.PlayerId)
            {
                return;
            }

            if (_pendingHand == null || hand.Version >= _pendingHand.Version)
            {
                _pendingHand = hand;
            }

            ApplySnapshot();
        }

        private void ApplySnapshot()
        {
            if (_disposed || _pendingView == null || _pendingHand == null)
            {
                return;
            }

            if (_pendingView.Version != _pendingHand.Version)
            {
                return;
            }

            if (_match != null && _pendingView.Version <= _match.Version)
            {
                return;
            }

            _match = _pendingView;
            UpdateScoreIndicator();
            Hand = _pendingHand.Tiles;
            BoardTiles = BoardLayout.Arrange(_match.Board);
            FitBoard();
            Scores = _match.Scores.Select(score => string.Format(GetText("Match_lblScore"),
                GetPlayerName(score.PlayerId), score.Points, score.TileCount))
                .ToList();
            Players = _match.Scores.Select(score => new MatchPlayerRecord(score, GetPlayerName(
                score.PlayerId), _context.PlayerSession.PlayerId))
                .ToList();
            Opponents = Players.Where(player => !player.IsCurrentPlayer)
                .ToList();
            TurnText = string.Format(GetText("Match_lblTurn"), _match.CurrentPlayerId.HasValue
                ? GetPlayerName(_match.CurrentPlayerId.Value) : string.Empty);
            RoundText = string.Format(GetText("Match_lblRound"), _match.RoundNumber,
                _match.StockCount);
            Status = _match.Phase == MatchPhase.InProgress ? string.Empty : GetText("Match_msg"
                + _match.Phase);
            RevealedHands = _match.LastRound == null ? new List<string>(
                ) : _match.LastRound.RevealedHands.Select(hand => string.Format(GetText(
                "Match_lblRevealed"), GetPlayerName(hand.PlayerId), string.Join(", ",
                hand.Tiles.Select(tile => tile.FirstValue + "|" + tile.SecondValue))))
                .ToList();
            SelectedTile = null;
            OnPropertyChanged(nameof(Hand));
            OnPropertyChanged(nameof(BoardTiles));
            OnPropertyChanged(nameof(BoardWidth));
            OnPropertyChanged(nameof(BoardHeight));
            OnPropertyChanged(nameof(Scores));
            OnPropertyChanged(nameof(Players));
            OnPropertyChanged(nameof(Opponents));
            OnPropertyChanged(nameof(TurnText));
            OnPropertyChanged(nameof(RoundText));
            OnPropertyChanged(nameof(RevealedHands));
            RaiseCommands();
        }

        private void UpdateScoreIndicator()
        {
            PlayScore score = _match.LastPlay;
            HasScoredPlay = score != null && score.Points > 0;
            AwardedPointsText = HasScoredPlay ? score.Points.ToString("+0",
                System.Globalization.CultureInfo.CurrentCulture) : string.Empty;
            ScoreMultiplierText = HasScoredPlay && score.Multiplier > 1 ? "×"
                + score.Multiplier.ToString(
                System.Globalization.CultureInfo.CurrentCulture) : string.Empty;
            OnPropertyChanged(nameof(AwardedPointsText));
            OnPropertyChanged(nameof(ScoreMultiplierText));
            OnPropertyChanged(nameof(HasScoredPlay));
        }

        private void FitBoard()
        {
            if (BoardTiles.Count == 0)
            {
                return;
            }

            double minimumX = BoardTiles.Min(tile => tile.X);
            double minimumY = BoardTiles.Min(tile => tile.Y);
            BoardWidth = BoardTiles.Max(tile => tile.X) - minimumX + BoardMargin * 2;
            BoardHeight = BoardTiles.Max(tile => tile.Y) - minimumY + BoardMargin * 2;
            foreach (TilePosition tile in BoardTiles)
            {
                tile.X += BoardMargin / 2 - minimumX;
                tile.Y += BoardMargin / 2 - minimumY;
            }
        }

        private string GetPlayerName(int playerId)
        {
            string name;
            return _context.PlayerNames.TryGetValue(playerId, out name)
                ? name : playerId.ToString();
        }

        private void Dispatch(Action action)
        {
            _dispatcher.BeginInvoke(new Action(() => DispatchIfActive(action)));
        }

        private void DispatchIfActive(Action action)
        {
            if (!_disposed)
            {
                action();
            }
        }

        private void RaiseCommands()
        {
            PlayCommand.RaiseCanExecuteChanged();
            DrawCommand.RaiseCanExecuteChanged();
            PassCommand.RaiseCanExecuteChanged();
            RefreshCommand.RaiseCanExecuteChanged();
            LeaveCommand.RaiseCanExecuteChanged();
        }

        private static string GetText(string key)
        {
            return Localization.Resources.ResourceManager.GetString(key)
                ?? throw new InvalidOperationException("Missing localization resource: " + key);
        }
    }
}
