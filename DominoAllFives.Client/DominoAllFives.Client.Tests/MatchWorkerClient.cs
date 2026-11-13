using System;
using System.Collections.Generic;
using System.Threading;
using DominoAllFives.Client.WPF.Services.Wcf.Interfaces;
using MatchService;

namespace DominoAllFives.Client.Tests
{
    internal sealed class MatchWorkerClient : IMatchClient
    {
        private const int WorkTimeoutMilliseconds = 5000;
        private int _openCalls;
        private int _playCalls;
        private int _drawCalls;
        private int _passCalls;
        private int _getMatchCalls;
        private int _leaveCalls;
        private int _disposeCalls;
        public MatchView View { get; set; } = CreateView(1);
        public PlayerHand Hand { get; set; } = CreateHand(1);
        public ManualResetEventSlim WorkEntered { get; } = new ManualResetEventSlim();
        public ManualResetEventSlim ReleaseWork { get; } = new ManualResetEventSlim();
        public ManualResetEventSlim DisposeEntered { get; } = new ManualResetEventSlim();
        public MatchWorkerOperation BlockedOperation { get; set; }
        public Exception OpenError { get; set; }
        public Exception PlayError { get; set; }
        public Exception QueryError { get; set; }
        public Exception LeaveError { get; set; }
        public Exception DisposeError { get; set; }
        public DominoTile PlayedTile { get; private set; }
        public BoardEnd PlayedEnd { get; private set; }
        public long PlayedVersion { get; private set; }
        public int WorkerThreadId { get; private set; }
        public int DisposeThreadId { get; private set; }
        public int OpenCalls => _openCalls;
        public int PlayCalls => _playCalls;
        public int DrawCalls => _drawCalls;
        public int PassCalls => _passCalls;
        public int GetMatchCalls => _getMatchCalls;
        public int LeaveCalls => _leaveCalls;
        public int DisposeCalls => _disposeCalls;

        public event Action<MatchView> MatchChanged;
        public event Action<PlayerHand> HandChanged;

        public MatchView Open(string sessionKey, int matchId)
        {
            Interlocked.Increment(ref _openCalls);
            WaitForRelease(MatchWorkerOperation.Open);
            if (OpenError != null)
            {
                throw OpenError;
            }

            return View;
        }

        public MatchView GetMatch(int matchId)
        {
            Interlocked.Increment(ref _getMatchCalls);
            WaitForRelease(MatchWorkerOperation.GetMatch);
            if (QueryError != null)
            {
                throw QueryError;
            }

            return View;
        }

        public PlayerHand GetHand(int matchId)
        {
            return Hand;
        }

        public MatchView Play(int matchId, DominoTile tile, BoardEnd end, long version)
        {
            Interlocked.Increment(ref _playCalls);
            WaitForRelease(MatchWorkerOperation.Play);
            PlayedTile = tile;
            PlayedEnd = end;
            PlayedVersion = version;
            if (PlayError != null)
            {
                throw PlayError;
            }

            return View;
        }

        public MatchView Draw(int matchId, long version)
        {
            Interlocked.Increment(ref _drawCalls);
            WaitForRelease(MatchWorkerOperation.Draw);
            return View;
        }

        public MatchView Pass(int matchId, long version)
        {
            Interlocked.Increment(ref _passCalls);
            WaitForRelease(MatchWorkerOperation.Pass);
            return View;
        }

        public void Leave(int matchId)
        {
            Interlocked.Increment(ref _leaveCalls);
            WaitForRelease(MatchWorkerOperation.Leave);
            if (LeaveError != null)
            {
                throw LeaveError;
            }
        }

        public void Dispose()
        {
            Interlocked.Increment(ref _disposeCalls);
            DisposeThreadId = Thread.CurrentThread.ManagedThreadId;
            DisposeEntered.Set();
            if (DisposeError != null)
            {
                throw DisposeError;
            }
        }

        public void Emit(MatchView view, PlayerHand hand)
        {
            MatchChanged?.Invoke(view);
            HandChanged?.Invoke(hand);
        }

        public static MatchView CreateView(long version)
        {
            return new MatchView
            {
                MatchId = 1,
                Version = version,
                OpeningTileId = 1,
                CurrentPlayerId = 1,
                Phase = MatchPhase.InProgress,
                Board = new PublicBoard
                {
                    Tiles = new List<PlacedTile>(), Ends = new List<PublicEnd>()
                },
                Scores = new List<PlayerScore>()
            };
        }

        public static PlayerHand CreateHand(long version)
        {
            return new PlayerHand
            {
                MatchId = 1, PlayerId = 1, Version = version,
                Tiles = new List<DominoTile>
                {
                    new DominoTile { TileId = 1, FirstValue = 2, SecondValue = 3 }
                }
            };
        }

        private void WaitForRelease(MatchWorkerOperation operation)
        {
            WorkerThreadId = Thread.CurrentThread.ManagedThreadId;
            if (BlockedOperation == operation)
            {
                WorkEntered.Set();
                if (!ReleaseWork.Wait(WorkTimeoutMilliseconds))
                {
                    throw new TimeoutException("The test did not release the match operation.");
                }
            }
        }
    }
}
