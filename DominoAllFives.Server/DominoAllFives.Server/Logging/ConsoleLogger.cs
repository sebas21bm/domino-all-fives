using System;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server.Logging
{
    internal sealed class ConsoleLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => Scope.instance;
        public bool IsEnabled(
            LogLevel logLevel) =>
            logLevel >= LogLevel.Information
            && logLevel < LogLevel.None;
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            Console.Error.WriteLine(
                "{0:O} [{1}] {2}: {3}{4}{5}",
                DateTime.UtcNow,
                logLevel,
                typeof(T).FullName,
                formatter(
                state,
                exception),
                exception == null
                ? string.Empty : Environment.NewLine,
                exception);
        }

        private sealed class Scope : IDisposable
        {
            internal static readonly Scope instance = new Scope();
            public void Dispose()
            {
            }
        }
    }
}
