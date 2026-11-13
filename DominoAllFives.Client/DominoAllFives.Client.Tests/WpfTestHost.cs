using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace DominoAllFives.Client.Tests
{
    internal static class WpfTestHost
    {
        private const int StartupTimeoutMilliseconds = 5000;
        private static readonly object syncRoot = new object();
        private static readonly ManualResetEventSlim started = new ManualResetEventSlim();
        private static Dispatcher dispatcher;
        private static Thread uiThread;

        public static void EnsureStarted()
        {
            lock (syncRoot)
            {
                if (dispatcher != null)
                {
                    return;
                }

                if (Application.Current != null)
                {
                    dispatcher = Application.Current.Dispatcher;
                    return;
                }

                uiThread = new Thread(RunDispatcher) { IsBackground = true };
                uiThread.SetApartmentState(ApartmentState.STA);
                uiThread.Start();
                if (!started.Wait(StartupTimeoutMilliseconds))
                {
                    throw new TimeoutException("The test UI dispatcher did not start.");
                }
            }
        }

        public static void Invoke(Action action)
        {
            EnsureStarted();
            dispatcher.Invoke(action);
        }

        public static TResult Invoke<TResult>(Func<TResult> action)
        {
            EnsureStarted();
            return dispatcher.Invoke(action);
        }

        public static void Stop()
        {
            if (uiThread == null)
            {
                return;
            }

            dispatcher.Invoke(StopApplication);
            if (!uiThread.Join(StartupTimeoutMilliseconds))
            {
                throw new TimeoutException("The test UI dispatcher did not stop.");
            }
        }

        private static void StopApplication()
        {
            // Other fixtures create windows on their own STA threads; stop only this test pump.
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }

        private static void RunDispatcher()
        {
            Application application = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            dispatcher = application.Dispatcher;
            started.Set();
            Dispatcher.Run();
        }
    }
}
