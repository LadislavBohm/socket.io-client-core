using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Socket.Io.Client.Core.Test.Model
{
    public abstract class TestBase
    {
        private readonly ITestOutputHelper _testOutputHelper;

        protected TestBase(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        protected ILogger<T> CreateLogger<T>() => new XUnitLogger<T>(_testOutputHelper);

        protected SocketIoClient CreateClient() => new SocketIoClient(null, CreateLogger<SocketIoClient>());
    }
}
