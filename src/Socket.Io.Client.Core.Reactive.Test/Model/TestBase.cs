using Microsoft.Extensions.Logging;
using Socket.Io.Client.Core.Reactive.Model;
using Xunit.Abstractions;

namespace Socket.Io.Client.Core.Reactive.Test.Model
{
    public abstract class TestBase
    {
        private readonly ITestOutputHelper _testOutputHelper;

        protected TestBase(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        protected ILogger<T> CreateLogger<T>(LogLevel minLogLevel = LogLevel.Trace) => new XUnitLogger<T>(_testOutputHelper, minLogLevel);

        protected SocketIoClient CreateClient(LogLevel minLogLevel = LogLevel.Debug)
        {
            return new SocketIoClient(new SocketIoClientOptions().With(logger: CreateLogger<SocketIoClient>(minLogLevel)));
        } 
    }
}
