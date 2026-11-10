using System;
using System.Threading;
using System.Windows;

namespace DominoAllFives.Client.WPF.Services.Threading
{
    public static class BackgroundWork
    {
        public static void Start<TState, TResult>(BackgroundJob<TState, TResult> job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));
            ThreadPool.QueueUserWorkItem(ExecuteJob, job);
        }

        private static void ExecuteJob(object state)
        {
            IBackgroundJob job = state as IBackgroundJob;
            if (job == null) throw new ArgumentException(null, nameof(state));
            job.Execute();
            Application.Current.Dispatcher.BeginInvoke(new Action(job.Complete));
        }
    }
}
