using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Socket.Io.Client.Core.Reactive.Model.SocketIo;
using Socket.Io.Client.Core.Reactive.Test.Extensions;
using Socket.Io.Client.Core.Reactive.Test.Model;
using Xunit;
using Xunit.Abstractions;

namespace Socket.Io.Client.Core.Reactive.Test
{
    public class SocketIoClientTest
    {
        public class Connect : TestBase
        {
            public Connect(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
            {
            }

            [Fact]
            public async Task InvalidUrl_ShouldThrow()
            {
                using var client = CreateClient();
                await Assert.ThrowsAnyAsync<Exception>(() => client.OpenAsync(new Uri("http://abcd")));
            }

            [Fact]
            public async Task Open_ThenClose_ShouldEmitHandshakeWhileOpen()
            {
                using var client = CreateClient();

                using var called = client.Events.OnHandshake.SubscribeCalled();
                await client.OpenAsync(new Uri("http://localhost:3000"));

                await called.AssertAtLeastOnceAsync(TimeSpan.FromMilliseconds(100));
                Assert.Equal(ReadyState.Open, client.State);
                
                await client.CloseAsync();
                Assert.Equal(ReadyState.Closed, client.State);
            }

            [Fact]
            public async Task Ping_ShouldReceivePong()
            {
                using var client = CreateClient();
                using var pong = client.Events.OnPong.SubscribeCalled();
                using var probeError = client.Events.OnProbeError.SubscribeCalled();
                
                await client.OpenAsync(new Uri("http://localhost:3000"));

                await pong.AssertAtLeastOnceAsync(TimeSpan.FromMilliseconds(100));
                probeError.AssertNever();
            }

            [Fact]
            public async Task Ping_KeepConnectionAlive_ShouldCallMultiplePings()
            {
                using var client = CreateClient();
                using var called = client.Events.OnPong.SubscribeCalled();
                using var probeError = client.Events.OnProbeError.SubscribeCalled();

                await client.OpenAsync(new Uri("http://localhost:3000"));

                //ping interval is set to 50ms
                await called.AssertAtLeastAsync(5, TimeSpan.FromMilliseconds(300));
                probeError.AssertNever();
            }
        }
    }
}
