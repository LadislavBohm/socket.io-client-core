using Microsoft.Extensions.Logging;
using Socket.Io.Client.Core;
using Socket.Io.Client.Core.Model;
using Xunit.Abstractions;

namespace Socket.Io.Client.Core.Test.Model
{
    public abstract class TestBase
    {
        protected readonly ITestOutputHelper TestOutputHelper;

        protected TestBase(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
        }

        protected ILogger<T> CreateLogger<T>(LogLevel minLogLevel = LogLevel.Trace) => new XUnitLogger<T>(TestOutputHelper, minLogLevel);

        protected SocketIoClient CreateClient(LogLevel minLogLevel = LogLevel.Debug)
        {
            return new SocketIoClient(new SocketIoClientOptions().With(logger: CreateLogger<SocketIoClient>(minLogLevel)));
        } 
    }
}
