using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Socket.Io.Client.Core.Model;
using Socket.Io.Client.Core.Model.SocketIo;
using Socket.Io.Client.Core.Test.Extensions;
using Socket.Io.Client.Core.Test.Model;
using Xunit;
using Xunit.Abstractions;

namespace Socket.Io.Client.Core.Test
{
    public partial class SocketIoClientTest
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
                await client.OpenTestAsync();

                await called.AssertAtLeastOnceAsync(TimeSpan.FromMilliseconds(150));
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

                await client.OpenTestAsync();

                await pong.AssertAtLeastOnceAsync(TimeSpan.FromMilliseconds(100));
                probeError.AssertNever();
            }

            [Fact]
            public async Task Ping_KeepConnectionAlive_ShouldCallMultiplePings()
            {
                using var client = CreateClient();
                using var called = client.Events.OnPong.SubscribeCalled();
                using var probeError = client.Events.OnProbeError.SubscribeCalled();

                await client.OpenTestAsync();

                //ping interval is set to 50ms
                await called.AssertAtLeastAsync(5, TimeSpan.FromMilliseconds(400));
                probeError.AssertNever();
            }

            [Fact]
            public async Task DefaultNamespace_ShouldNotReceiveMessagesFromCustomNamespace()
            {
                using var client = CreateClient();

                var called = client.On("namespace-message").SubscribeCalled(m =>
                {
                    Assert.Equal("namespace-message", m.EventName);
                    Assert.Equal("namespace-message", m.Data[0].ToString());
                });

                await client.OpenTestAsync();

                await called.AssertNeverAsync(TimeSpan.FromMilliseconds(100));
            }

            [Fact]
            public async Task CustomNamespace_ShouldReceiveMessagesFromNamespace()
            {
                using var client = CreateClient();

                var called = client.On("namespace-message").SubscribeCalled(m =>
                {
                    Assert.Equal("namespace-message", m.EventName);
                    Assert.Equal("namespace-message", m.Data[0].ToString());
                });

                await client.OpenTestAsync(new Uri("http://localhost:8764/some-namespace"));

                await called.AssertAtLeastAsync(3, TimeSpan.FromMilliseconds(100));
            }

            [Fact]
            public async Task WithQuery_ShouldPassQueryWithConnect()
            {
                using var client = CreateClient();
                var called = client.On("test-room").SubscribeCalled(e =>
                {
                    Assert.Equal("welcome", e.Data[0].ToString());
                });

                await client.OpenTestAsync(new Uri("http://localhost:8764/some-namespace?roomId=test-room"));

                await called.AssertOnceAsync(TimeSpan.FromMilliseconds(100));
            }

            [Fact]
            public async Task Disconnect_ShouldCloseSocket()
            {
                using var client = CreateClient();
                using var called = client.Events.OnDisconnect.SubscribeCalled();

                await client.OpenTestAsync();

                //wait for disconnect ACK just to verify it's working
                _ = client.Emit("disconnect-me");

                //wait for handling of the disconnect by the socket
                await called.AssertOnceAsync(TimeSpan.FromMilliseconds(100));
            }
        }
    }
}
