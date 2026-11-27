using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Windows.Media;
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
        private const long UnrestrictedVersion = 0;
        private const double InitialBoardWidth = 400;
        private const double InitialBoardHeight = 300;
        private const double BoardMargin = 96;
        private const int RoundSummarySeconds = 4;
        private static readonly BoardEnd[] availableBoardEnds;
        private readonly IFrameNavigationService _navigation;
        private readonly MatchNavigation _context;
        private readonly IMatchClient _client;
        private readonly Dispatcher _dispatcher;
        private readonly DispatcherTimer _roundSummaryTimer;
        private readonly ILogger<MatchViewModel> _logger;
        private MatchView _pendingView;
        private PlayerHand _pendingHand;
        private Tuple<MatchView, PlayerHand> _confirmedSnapshot;
        private MatchView _match;
        private DominoTile _selectedTile;
        private BoardEnd? _selectedEnd;
        private bool _isBusy;
        private bool _disposed;
        private bool _opened;
        private string _status;
        private bool _isClientDisposalStarted;
        private bool _showConnectionAlert;
        private bool _isReconnecting;
        private int _connectionRevision;
        private int _lastPresentedRound;

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

        public IReadOnlyList<EndOption> Ends { get; private set; } = Array.Empty<EndOption>();

        public bool IsStarting
        {
            get
            {
                return _match == null && _isBusy && !IsRoundSummaryVisible
                    && !ShowConnectionAlert;
            }
        }

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

        public bool ShowConnectionAlert
        {
            get
            {
                return _showConnectionAlert;
            }
        }

        public string ConnectionMessage
        {
            get
            {
                string textKey = _isReconnecting
                    ? "Match_msgReconnecting"
                    : "Match_msgConnectionFailed";
                return GetText(textKey);
            }
        }

        public string RoundText { get; private set; }
        public string TurnActionText { get; private set; }
        public string RoundSummaryText { get; private set; }
        public IReadOnlyList<RoundPlayerRecord> RoundPlayers { get; private set; }
        public string WinnerNames { get; private set; }
        public ImageSource WinnerAvatar { get; private set; }

        public bool IsRoundSummaryVisible
        {
            get
            {
                return RoundPlayers != null;
            }
        }

        public bool IsMatchResultVisible
        {
            get
            {
                return _match?.Result != null;
            }
        }

        public bool IsMatchCancelled
        {
            get
            {
                return _match?.Result?.Phase == MatchStatus.Cancelled;
            }
        }

        public bool HasWinner
        {
            get
            {
                return !string.IsNullOrEmpty(WinnerNames);
            }
        }

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
                if (!SetProperty(ref _selectedTile, value))
                {
                    return;
                }

                UpdateEndOptions();
                RaiseCommands();
            }
        }

        public BoardEnd? SelectedEnd
        {
            get
            {
                return _selectedEnd;
            }

            set
            {
                if (SetProperty(ref _selectedEnd, value))
                {
                    RaiseCommands();
                }
            }
        }

        public RelayCommand PlayCommand { get; }
        public RelayCommand DrawCommand { get; }
        public RelayCommand PassCommand { get; }
        public RelayCommand ReconnectCommand { get; }
        public RelayCommand LeaveCommand { get; }

        static MatchViewModel()
        {
            availableBoardEnds = (BoardEnd[])Enum.GetValues(typeof(BoardEnd));
        }

        public MatchViewModel(IFrameNavigationService navigation, MatchNavigation context)
            : this(navigation, context, new MatchClient())
        {
        }

        public MatchViewModel(
            IFrameNavigationService navigation,
            MatchNavigation context,
            IMatchClient client)
        {
            _logger = LoggerFactoryProvider.CreateLogger<MatchViewModel>();
            _dispatcher = Dispatcher.CurrentDispatcher;
            _roundSummaryTimer = new DispatcherTimer(DispatcherPriority.Normal, _dispatcher)
            {
                Interval = TimeSpan.FromSeconds(RoundSummarySeconds)
            };
            _roundSummaryTimer.Tick += EndRoundSummary;
            _navigation = navigation;
            _context = context;
            _client = client;
            _client.MatchChanged += OnMatchChanged;
            _client.HandChanged += OnHandChanged;
            _client.ConnectionLost += OnConnectionLost;
            PlayCommand = new RelayCommand(Play, CanPlay);
            DrawCommand = new RelayCommand(Draw, CanDraw);
            PassCommand = new RelayCommand(Pass, CanPass);
            ReconnectCommand = new RelayCommand(Reconnect, CanReconnect);
            LeaveCommand = new RelayCommand(Leave, CanLeave);
        }

        public void Initialize()
        {
            if (_opened || _isBusy || _disposed || ShowConnectionAlert)
            {
                return;
            }

            StartOperation(new MatchOperation(OperationKind.Open));
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _roundSummaryTimer.Stop();
            _roundSummaryTimer.Tick -= EndRoundSummary;
            _client.MatchChanged -= OnMatchChanged;
            _client.HandChanged -= OnHandChanged;
            _client.ConnectionLost -= OnConnectionLost;
            RaiseCommands();
            if (!_isBusy)
            {
                BeginClientDisposal();
            }
        }

        private bool IsOwnTurn()
        {
            bool canAct = !_disposed && !_isBusy && !ShowConnectionAlert && !IsRoundSummaryVisible;
            return canAct && _match?.Phase == MatchStatus.InProgress
                && _match.CurrentPlayerId == _context.PlayerSession.PlayerId;
        }

        private bool CanPlay()
        {
            return IsOwnTurn() && SelectedTile != null && SelectedEnd.HasValue
                && IsPlayable(SelectedTile, SelectedEnd.Value);
        }

        private void UpdateEndOptions()
        {
            SelectedEnd = null;
            Ends = GetEndOptions();
            OnPropertyChanged(nameof(Ends));

            if (Ends.Count == 1)
            {
                SelectedEnd = Ends[0].End;
            }
        }

        private IReadOnlyList<EndOption> GetEndOptions()
        {
            if (_match == null || SelectedTile == null)
            {
                return Array.Empty<EndOption>();
            }

            List<EndOption> options = new List<EndOption>();
            foreach (BoardEnd end in availableBoardEnds)
            {
                if (IsPlayable(SelectedTile, end))
                {
                    options.Add(new EndOption(end));
                }
            }

            return options.ToArray();
        }

        private bool IsPlayable(DominoTile tile, BoardEnd end)
        {
            if (_match.Board.Tiles.Count == 0)
            {
                return end == BoardEnd.Left && tile.TileId == _match.OpeningTileId;
            }

            foreach (PublicEnd target in _match.Board.Ends)
            {
                if (target.End != end || target.IsBlocked)
                {
                    continue;
                }

                if (tile.FirstValue == target.Value || tile.SecondValue == target.Value)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasMove()
        {
            foreach (DominoTile tile in Hand)
            {
                if (HasPlayableEnd(tile))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasPlayableEnd(DominoTile tile)
        {
            foreach (BoardEnd end in availableBoardEnds)
            {
                if (IsPlayable(tile, end))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanDraw()
        {
            return IsOwnTurn() && _match.StockCount > 0 && !HasMove();
        }

        private bool CanPass()
        {
            return IsOwnTurn() && _match.StockCount == 0 && !HasMove();
        }

        private bool CanReconnect()
        {
            return ShowConnectionAlert && !_isBusy && !_isReconnecting && !_disposed;
        }

        private bool CanLeave()
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
            MatchOperation operation = new MatchOperation(OperationKind.Play)
            {
                Tile = tile,
                End = SelectedEnd.Value,
                Version = _match.Version
            };
            StartOperation(operation);
        }

        private void Draw()
        {
            if (!CanDraw())
            {
                return;
            }

            StartOperation(new MatchOperation(OperationKind.Draw) { Version = _match.Version });
        }

        private void Pass()
        {
            if (!CanPass())
            {
                return;
            }

            StartOperation(new MatchOperation(OperationKind.Pass) { Version = _match.Version });
        }

        private void Reconnect()
        {
            if (!CanReconnect())
            {
                return;
            }

            _pendingView = null;
            _pendingHand = null;
            _confirmedSnapshot = null;
            _isReconnecting = true;
            OnPropertyChanged(nameof(ConnectionMessage));
            MatchOperation operation = new MatchOperation(OperationKind.Reconnect)
            {
                ConnectionRevision = _connectionRevision,
                Version = _match?.Version ?? UnrestrictedVersion
            };
            StartOperation(operation);
        }

        private void CompleteReconnect(int connectionRevision)
        {
            _isReconnecting = false;
            if (connectionRevision == _connectionRevision)
            {
                Status = string.Empty;
                _showConnectionAlert = false;
                ApplySnapshot();
            }

            NotifyConnectionState();
        }

        private void Leave()
        {
            if (_match != null && _match.Phase != MatchStatus.InProgress)
            {
                NavigateToMenu();
                return;
            }

            StartOperation(new MatchOperation(OperationKind.Leave));
        }

        private void NavigateToMenu()
        {
            _navigation.NavigateAsRoot<MainMenuViewModel>();
        }

        private void Synchronize(long minimumVersion)
        {
            for (int attempt = 0; attempt < SynchronizationAttempts; attempt++)
            {
                MatchView view = _client.GetMatch(_context.MatchId);
                PlayerHand hand = _client.GetHand(_context.MatchId);
                if (IsMatchingSnapshot(view, hand) && view.Version >= minimumVersion)
                {
                    _dispatcher.BeginInvoke(
                        new Action<MatchView, PlayerHand>(AcceptConfirmedSnapshot),
                        view,
                        hand);
                    return;
                }
            }

            throw new TimeoutException("Could not retrieve matching match and hand versions.");
        }

        private bool IsMatchingSnapshot(MatchView view, PlayerHand hand)
        {
            return IsMatchView(view) && IsOwnHand(hand) && view.Version == hand.Version;
        }

        private void StartOperation(MatchOperation operation)
        {
            if (_disposed || _isBusy)
            {
                return;
            }

            _isBusy = true;
            OnPropertyChanged(nameof(IsStarting));
            RaiseCommands();
            BackgroundJob<MatchOperation, bool> job = new BackgroundJob<MatchOperation, bool>(
                ExecuteOperation,
                operation,
                OnOperationCompleted,
                OnOperationFailed);
            BackgroundWork.Start(job);
        }

        private bool ExecuteOperation(MatchOperation operation)
        {
            switch (operation.Kind)
            {
                case OperationKind.Open:
                case OperationKind.Reconnect:
                    OpenAndSynchronize(operation.Version);
                    break;
                case OperationKind.Play:
                    PlayAndSynchronize(operation.Tile, operation.End, operation.Version);
                    break;
                case OperationKind.Draw:
                    DrawAndSynchronize(operation.Version);
                    break;
                case OperationKind.Pass:
                    PassAndSynchronize(operation.Version);
                    break;
                case OperationKind.Leave:
                    LeaveMatch();
                    break;
                case OperationKind.Synchronize:
                    Synchronize(UnrestrictedVersion);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation));
            }

            return true;
        }

        private void OnOperationCompleted(bool result, MatchOperation operation)
        {
            _isBusy = false;
            OnPropertyChanged(nameof(IsStarting));
            if (_disposed)
            {
                BeginClientDisposal();
                return;
            }

            RaiseCommands();
            if (operation.Kind == OperationKind.Reconnect)
            {
                CompleteReconnect(operation.ConnectionRevision);
            }
            else if (operation.Kind == OperationKind.Leave)
            {
                NavigateToMenu();
            }
        }

        private void OnOperationFailed(Exception ex, MatchOperation operation)
        {
            _isBusy = false;
            _isReconnecting = false;
            OnPropertyChanged(nameof(IsStarting));
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
                _logger.LogDebug(ex, "Match operation was rejected.");
                if (fault.Detail == MatchIssue.StaleVersion
                    && operation.Kind != OperationKind.Synchronize)
                {
                    StartOperation(new MatchOperation(OperationKind.Synchronize));
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

            NotifyConnectionState();
        }

        private void BeginClientDisposal()
        {
            if (_isClientDisposalStarted)
            {
                return;
            }

            _isClientDisposalStarted = true;
            BackgroundJob<IMatchClient, bool> job = new BackgroundJob<IMatchClient, bool>(
                DisposeClientOnWorker,
                _client,
                OnClientDisposed,
                OnClientDisposalFailed);
            BackgroundWork.Start(job);
        }

        private static bool DisposeClientOnWorker(IMatchClient client)
        {
            client.Dispose();
            return true;
        }

        private static void OnClientDisposed(bool result, IMatchClient client)
        {
        }

        private void OnClientDisposalFailed(Exception ex, IMatchClient client)
        {
            _logger.LogWarning(ex, "Match connection could not be closed cleanly.");
        }

        private void OpenAndSynchronize(long minimumVersion)
        {
            _client.Open(_context.PlayerSession.SessionKey, _context.MatchId);
            _opened = true;
            Synchronize(minimumVersion);
        }

        private void PlayAndSynchronize(DominoTile tile, BoardEnd end, long version)
        {
            _client.Play(_context.MatchId, tile, end, version);
            Synchronize(UnrestrictedVersion);
        }

        private void DrawAndSynchronize(long version)
        {
            _client.Draw(_context.MatchId, version);
            Synchronize(UnrestrictedVersion);
        }

        private void PassAndSynchronize(long version)
        {
            _client.Pass(_context.MatchId, version);
            Synchronize(UnrestrictedVersion);
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
            ShowConnectionFailure();
            _logger.LogWarning(ex, "Match connection failed.");
        }

        private void OnConnectionLost()
        {
            Dispatch(ShowConnectionFailure);
        }

        private void ShowConnectionFailure()
        {
            if (_match != null && _match.Phase != MatchStatus.InProgress)
            {
                return;
            }

            _connectionRevision++;
            _showConnectionAlert = true;
            NotifyConnectionState();
        }

        private void NotifyConnectionState()
        {
            OnPropertyChanged(nameof(ShowConnectionAlert));
            OnPropertyChanged(nameof(ConnectionMessage));
            OnPropertyChanged(nameof(IsStarting));
            RaiseCommands();
        }

        private void AcceptConfirmedSnapshot(MatchView view, PlayerHand hand)
        {
            if (_disposed)
            {
                return;
            }

            if (_confirmedSnapshot == null || view.Version >= _confirmedSnapshot.Item1.Version)
            {
                _confirmedSnapshot = Tuple.Create(view, hand);
            }

            ApplySnapshot();
        }

        private bool IsMatchView(MatchView view)
        {
            return view != null && view.MatchId == _context.MatchId;
        }

        private bool IsOwnHand(PlayerHand hand)
        {
            return hand != null && hand.MatchId == _context.MatchId
                && hand.PlayerId == _context.PlayerSession.PlayerId;
        }

        private void OnMatchChanged(MatchView view)
        {
            _dispatcher.BeginInvoke(new Action<MatchView>(AcceptView), view);
        }

        private void AcceptView(MatchView view)
        {
            if (_disposed || !IsMatchView(view))
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
            _dispatcher.BeginInvoke(new Action<PlayerHand>(AcceptHand), hand);
        }

        private void AcceptHand(PlayerHand hand)
        {
            if (_disposed || !IsOwnHand(hand))
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
            if (_disposed || ShowConnectionAlert || IsRoundSummaryVisible)
            {
                return;
            }

            if (!TryGetSnapshot(out MatchView view, out PlayerHand hand))
            {
                return;
            }

            if (_match != null && view.Version <= _match.Version)
            {
                return;
            }

            UpdateTurnAction(view);
            if (ShouldShowRoundSummary(view))
            {
                ShowRoundSummary(view.LastRound);
                return;
            }

            DisplaySnapshot(view, hand);
        }

        private bool TryGetSnapshot(out MatchView view, out PlayerHand hand)
        {
            view = _pendingView;
            hand = _pendingHand;
            bool hasMatchingCallbacks = view != null && hand != null
                && view.Version == hand.Version;
            if (_confirmedSnapshot != null
                && (!hasMatchingCallbacks || _confirmedSnapshot.Item1.Version > view.Version))
            {
                view = _confirmedSnapshot.Item1;
                hand = _confirmedSnapshot.Item2;
                return true;
            }

            return hasMatchingCallbacks;
        }

        private void DisplaySnapshot(MatchView view, PlayerHand hand)
        {
            _match = view;
            UpdateScoreIndicator();
            Hand = hand.Tiles;
            DisplayBoard(_match.Board);
            UpdatePlayerSummaries(_match.Result?.Scores ?? _match.Scores);
            UpdateMatchResult(_match.Result);
            TurnText = GetTurnText();
            RoundText = string.Format(
                GetText("Match_lblRound"),
                _match.RoundNumber,
                _match.StockCount);
            UpdateMatchStatus();
            SelectedTile = null;
            OnPropertyChanged(nameof(Hand));
            OnPropertyChanged(nameof(TurnText));
            OnPropertyChanged(nameof(RoundText));
            OnPropertyChanged(nameof(IsStarting));
            RaiseCommands();
        }

        private void UpdateMatchStatus()
        {
            if (_match.Phase == MatchStatus.InProgress)
            {
                Status = string.Empty;
                return;
            }

            Status = GetText("Match_msg" + _match.Phase);
        }

        private void DisplayBoard(PublicBoard board)
        {
            BoardTiles = BoardLayout.Arrange(board);
            FitBoard();
            OnPropertyChanged(nameof(BoardTiles));
            OnPropertyChanged(nameof(BoardWidth));
            OnPropertyChanged(nameof(BoardHeight));
        }

        private void UpdatePlayerSummaries(IEnumerable<PlayerScore> scores)
        {
            List<string> scoreDescriptions = new List<string>();
            List<MatchPlayerRecord> players = new List<MatchPlayerRecord>();
            int? currentPlayerId = _context.PlayerSession.PlayerId;
            foreach (PlayerScore score in scores)
            {
                string playerName = GetPlayerName(score.PlayerId);
                string description = string.Format(
                    GetText("Match_lblScore"),
                    playerName,
                    score.Points,
                    score.TileCount);
                scoreDescriptions.Add(description);
                players.Add(new MatchPlayerRecord(score, playerName, currentPlayerId));
            }

            Scores = scoreDescriptions;
            Players = players;
            Opponents = Players
                .Where(player => !player.IsCurrentPlayer)
                .ToList();
            OnPropertyChanged(nameof(Scores));
            OnPropertyChanged(nameof(Players));
            OnPropertyChanged(nameof(Opponents));
        }

        private void UpdateTurnAction(MatchView view)
        {
            TurnActionNotice action = view.LastTurnAction;
            TurnActionText = string.Empty;
            if (action != null && action.Version == view.Version)
            {
                string textKey = action.IsPass ? "Match_msgPassed" : "Match_msgDrew";
                TurnActionText = string.Format(GetText(textKey), GetPlayerName(action.PlayerId));
            }

            OnPropertyChanged(nameof(TurnActionText));
        }

        private bool ShouldShowRoundSummary(MatchView view)
        {
            RoundResult round = view.LastRound;
            if (round?.ClosedBoard == null || round.RoundNumber <= _lastPresentedRound)
            {
                return false;
            }

            if (round.Version == view.Version)
            {
                return true;
            }

            bool isOngoing = _match != null && view.Phase == MatchStatus.InProgress;
            return isOngoing && round.RoundNumber >= _match.RoundNumber;
        }

        private void ShowRoundSummary(RoundResult round)
        {
            RoundPlayers = GetRoundPlayers(round);
            RoundSummaryText = GetRoundSummaryText(round);
            _lastPresentedRound = round.RoundNumber;
            SelectedTile = null;
            DisplayBoard(round.ClosedBoard);
            UpdatePlayerSummaries(round.Scores);
            TurnText = string.Empty;
            OnPropertyChanged(nameof(TurnText));
            OnPropertyChanged(nameof(RoundPlayers));
            OnPropertyChanged(nameof(RoundSummaryText));
            OnPropertyChanged(nameof(IsRoundSummaryVisible));
            OnPropertyChanged(nameof(IsStarting));
            RaiseCommands();
            _roundSummaryTimer.Start();
        }

        private List<RoundPlayerRecord> GetRoundPlayers(RoundResult round)
        {
            Dictionary<int, PlayerHand> revealedHands = round.RevealedHands
                .ToDictionary(hand => hand.PlayerId);
            List<RoundPlayerRecord> players = new List<RoundPlayerRecord>();
            foreach (PlayerScore score in round.Scores)
            {
                string playerName = GetPlayerName(score.PlayerId);
                PlayerHand revealedHand = revealedHands[score.PlayerId];
                players.Add(new RoundPlayerRecord(playerName, score.Points, revealedHand.Tiles));
            }

            return players;
        }

        private string GetRoundSummaryText(RoundResult round)
        {
            if (!round.WinnerId.HasValue)
            {
                return GetText("Match_msgRoundTied");
            }

            string winnerName = GetPlayerName(round.WinnerId.Value);
            return string.Format(GetText("Match_msgRoundAward"), winnerName, round.AwardedPoints);
        }

        private void EndRoundSummary(object sender, EventArgs eventArgs)
        {
            _roundSummaryTimer.Stop();
            RoundPlayers = null;
            ApplySnapshot();
            OnPropertyChanged(nameof(RoundPlayers));
            OnPropertyChanged(nameof(IsRoundSummaryVisible));
            RaiseCommands();
        }

        private void UpdateMatchResult(MatchResult result)
        {
            if (result == null)
            {
                return;
            }

            List<string> winnerNames = new List<string>();
            foreach (int winnerId in result.WinnerIds)
            {
                winnerNames.Add(GetPlayerName(winnerId));
            }

            WinnerNames = string.Join(", ", winnerNames);
            if (result.WinnerIds.Count > 0
                && _context.PlayerAvatars.TryGetValue(result.WinnerIds[0], out ImageSource avatar))
            {
                WinnerAvatar = avatar;
            }

            OnPropertyChanged(nameof(WinnerNames));
            OnPropertyChanged(nameof(WinnerAvatar));
            OnPropertyChanged(nameof(IsMatchResultVisible));
            OnPropertyChanged(nameof(IsMatchCancelled));
            OnPropertyChanged(nameof(HasWinner));
        }

        private string GetTurnText()
        {
            if (!_match.CurrentPlayerId.HasValue)
            {
                return string.Empty;
            }

            string textKey = _match.Board.Tiles.Count == 0 ? "Match_lblOpener" : "Match_lblTurn";
            return string.Format(GetText(textKey), GetPlayerName(_match.CurrentPlayerId.Value));
        }

        private void UpdateScoreIndicator()
        {
            PlayScore score = _match.LastPlay;
            HasScoredPlay = score != null && score.RoundNumber == _match.RoundNumber
                && score.Points > 0;
            AwardedPointsText = string.Empty;
            ScoreMultiplierText = string.Empty;
            if (HasScoredPlay)
            {
                AwardedPointsText = score.Points.ToString("+0", CultureInfo.CurrentCulture);
                if (score.Multiplier > 1)
                {
                    ScoreMultiplierText = string.Format(
                        CultureInfo.CurrentCulture,
                        GetText("Match_lblScoreMultiplier"),
                        score.Multiplier);
                }
            }

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
            if (_context.PlayerNames.TryGetValue(playerId, out string name))
            {
                return name;
            }

            return playerId.ToString();
        }

        private void Dispatch(Action action)
        {
            _dispatcher.BeginInvoke(new Action<Action>(DispatchIfActive), action);
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
            ReconnectCommand.RaiseCanExecuteChanged();
            LeaveCommand.RaiseCanExecuteChanged();
        }

        private static string GetText(string key)
        {
            return Localization.Resources.ResourceManager.GetString(key)
                ?? throw new InvalidOperationException("Missing localization resource: " + key);
        }

        private enum OperationKind
        {
            Open,
            Reconnect,
            Play,
            Draw,
            Pass,
            Leave,
            Synchronize
        }

        private sealed class MatchOperation
        {
            public OperationKind Kind { get; }
            public long Version { get; set; }
            public DominoTile Tile { get; set; }
            public BoardEnd End { get; set; }
            public int ConnectionRevision { get; set; }

            public MatchOperation(OperationKind kind)
            {
                Kind = kind;
            }
        }
    }
}
