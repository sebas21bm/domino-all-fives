using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DominoAllFives.Client.WPF.Services
{
    /// <summary>
    /// Provides a centralized logger factory instance for the application.
    /// </summary>
    public static class LoggerFactoryProvider
    {
        private static readonly object lockObject = new object();
        private static ILoggerFactory loggerFactory;
        /// <summary>
        /// Gets the singleton instance of <see cref = "ILoggerFactory"/>.
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
        /// <typeparam name = "T">The type of class requesting the logger.</typeparam>
        /// <returns>An <see cref = "ILogger{T}"/> instance configured for the
        /// specified type.</returns>
        public static ILogger<T> CreateLogger<T>()
        {
            return Instance.CreateLogger<T>();
        }

        private static ILoggerFactory CreateLoggerFactory()
        {
            try
            {
                string logDirectory =
                    Path.Combine(
                    Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                    "DominoAllFives",
                    "Logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                string logFilePath = Path.Combine(logDirectory, "client_log-.txt");
                Serilog.Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} "
                        + "[{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();
                return LoggerFactory.Create(ConfigureLogging);
            }
            catch (IOException ex)
            {
                return CreateFallbackLogger(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                return CreateFallbackLogger(ex);
            }
        }

        private static void ConfigureLogging(ILoggingBuilder builder)
        {
            builder.AddSerilog(dispose: true);
            builder.AddDebug();
        }

        private static ILoggerFactory CreateFallbackLogger(Exception ex)
        {
            ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddDebug());
            factory.CreateLogger(typeof(LoggerFactoryProvider).FullName).LogWarning(
                ex, "File logging is unavailable; using debug logging.");
            return factory;
        }
    }
}
