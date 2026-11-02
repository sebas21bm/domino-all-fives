using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Server.Tests
{
    internal sealed class TestLogger<T> : ILogger<T>
    {
        public List<LogLevel> Levels { get; } = new List<LogLevel>();
        public List<Exception> Exceptions { get; } = new List<Exception>();
        public List<string> Messages { get; } = new List<string>();
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel < LogLevel.Warning) return;
            Levels.Add(logLevel);
            Exceptions.Add(exception);
            Messages.Add(formatter(state, exception));
        }
    }
}
