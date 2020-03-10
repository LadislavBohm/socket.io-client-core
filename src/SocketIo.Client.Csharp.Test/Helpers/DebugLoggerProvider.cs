using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SocketIo.Client.Csharp.Test.Helpers
{
    public class DebugLoggerProvider : ILoggerProvider
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DebugLogger();
        }
    }

    public class DebugLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var log = formatter(state, exception);
            if (exception != null)
            {
                Debug.WriteLine($"[{logLevel}] - [{eventId}] {log}. Exception: {exception}");
            }
            else
            {
                Debug.WriteLine($"[{logLevel}] - [{eventId}] {log}.");
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}