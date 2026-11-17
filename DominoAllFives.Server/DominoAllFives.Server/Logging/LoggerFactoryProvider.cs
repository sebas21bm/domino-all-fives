using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DominoAllFives.Server.Logging
{
    /// <summary>
    /// Provides a centralized logger factory instance for the server application.
    /// </summary>
    public static class LoggerFactoryProvider
    {
        private const long MaximumLogFileSizeBytes = 10 * 1024 * 1024;
        private const int RetainedLogFileCount = 14;
        private static readonly object lockObject = new object();
        private static ILoggerFactory loggerFactory;

        /// <summary>
        /// Gets the singleton instance of <see cref="ILoggerFactory"/>.
        /// </summary>
        public static ILoggerFactory Instance
        {
            get
            {
                if (loggerFactory == null)
                {
                    lock (lockObject)
                    {
                        if (loggerFactory == null)
                        {
                            loggerFactory = CreateLoggerFactory();
                        }
                    }
                }

                return loggerFactory;
            }
        }

        /// <summary>
        /// Creates a typed logger instance for a specific class type.
        /// </summary>
        /// <typeparam name="T">The type of class requesting the logger.</typeparam>
        /// <returns>
        /// An <see cref="ILogger{T}"/> instance configured for the specified type.
        /// </returns>
        public static ILogger<T> CreateLogger<T>()
        {
            return Instance.CreateLogger<T>();
        }

        private static ILoggerFactory CreateLoggerFactory()
        {
            string outputTemplate =
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} "
                + "[{Level:u3}] "
                + "[{SourceContext}] "
                + "{Message:lj}"
                + "{NewLine}"
                + "{Exception}";

            try
            {
                string logDirectory =
                    Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "Logs");

                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string logFilePath =
                    Path.Combine(
                        logDirectory,
                        "server_log-.txt");

                Log.Logger =
                    new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.Console(
                            outputTemplate: outputTemplate)
                        .WriteTo.File(
                            path: logFilePath,
                            rollingInterval: RollingInterval.Day,
                            rollOnFileSizeLimit: true,
                            fileSizeLimitBytes: MaximumLogFileSizeBytes,
                            retainedFileCountLimit: RetainedLogFileCount,
                            outputTemplate: outputTemplate)
                        .CreateLogger();

                return LoggerFactory.Create(builder => builder.AddSerilog(dispose: true));
            }
            catch (IOException ex)
            {
                return CreateFallbackLogger(ex, outputTemplate);
            }
            catch (UnauthorizedAccessException ex)
            {
                return CreateFallbackLogger(ex, outputTemplate);
            }
        }

        private static ILoggerFactory CreateFallbackLogger(
            Exception ex,
            string outputTemplate)
        {
            Log.Logger =
                new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console(
                        outputTemplate: outputTemplate)
                    .CreateLogger();

            ILoggerFactory factory =
                LoggerFactory.Create(builder => builder.AddSerilog(dispose: true));

            factory.CreateLogger(typeof(LoggerFactoryProvider).FullName)
                .LogWarning(
                    ex,
                    "File logging is unavailable; using console logging.");

            return factory;
        }
    }
}
