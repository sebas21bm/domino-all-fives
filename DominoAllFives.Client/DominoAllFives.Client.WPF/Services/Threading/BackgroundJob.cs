using System;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.Services.Threading
{
    public sealed class BackgroundJob<TState, TResult> : IBackgroundJob
    {
        private readonly object _stateLock = new object();
        private bool _isCancelled;
        public Func<TState, TResult> Work { get; }
        public TState State { get; }
        public Action<TResult, TState> OnCompleted { get; }
        public Action<Exception, TState> OnFailed { get; }
        public Action<TResult, TState> OnCompletedAfterCancel { get; }
        public TResult Result { get; private set; }
        public Exception Error { get; private set; }

        public bool IsCancelled
        {
            get
            {
                lock (_stateLock)
                {
                    return _isCancelled;
                }
            }
        }

        public BackgroundJob(
            Func<TState, TResult> work,
            TState state,
            Action<TResult, TState> onCompleted,
            Action<Exception, TState> onFailed) : this(
            work,
            state,
            onCompleted,
            onFailed,
            null)
        {
        }

        public BackgroundJob(
            Func<TState, TResult> work,
            TState state,
            Action<TResult, TState> onCompleted,
            Action<Exception, TState> onFailed,
            Action<TResult, TState> onCompletedAfterCancel)
        {
            Work = work ?? throw new ArgumentNullException(nameof(work));
            OnCompleted = onCompleted ?? throw new ArgumentNullException(nameof(onCompleted));
            OnFailed = onFailed ?? throw new ArgumentNullException(nameof(onFailed));
            State = state;
            OnCompletedAfterCancel = onCompletedAfterCancel;
        }

        public void Cancel()
        {
            lock (_stateLock)
            {
                _isCancelled = true;
            }
        }

        void IBackgroundJob.Execute()
        {
            Task<TResult> operation = new Task<TResult>(() => Work(State));
            operation.RunSynchronously();
            if (operation.IsFaulted)
            {
                Error = operation.Exception.InnerException;
            }
            else
            {
                Result = operation.Result;
            }
        }

        void IBackgroundJob.Complete()
        {
            if (Error != null)
            {
                if (!IsCancelled)
                {
                    OnFailed(Error, State);
                }

                return;
            }

            if (IsCancelled)
            {
                OnCompletedAfterCancel?.Invoke(Result, State);
            }
            else
            {
                OnCompleted(Result, State);
            }
        }
    }
}
