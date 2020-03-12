using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Socket.Io.Client.Core.Test.Model
{
    public class XUnitLogger<T> : ILogger<T>, IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly LogLevel _minLogLevel;

        public XUnitLogger(ITestOutputHelper output, LogLevel minLogLevel)
        {
            _output = output;
            _minLogLevel = minLogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if ((int)logLevel >= (int)_minLogLevel)
                _output.WriteLine($"[{DateTime.Now:dd.MM.yyyy HH:mm:ss.fff}] [{logLevel.ToString().ToUpper()}] {formatter(state, exception)}");
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        public void Dispose()
        {
        }
    }
}
