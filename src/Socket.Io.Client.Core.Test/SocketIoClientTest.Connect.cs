using System;
using System.Threading.Tasks;
using Socket.Io.Client.Core.EventArguments;
using Socket.Io.Client.Core.Extensions;
using Socket.Io.Client.Core.Model;
using Socket.Io.Client.Core.Test.Extensions;
using Xunit;

namespace Socket.Io.Client.Core.Test
{
    public partial class SocketIoClientTest
    {
        public class Connect
        {
            [Fact]
            public async Task InvalidUrl_ShouldThrow()
            {
                using var client = new SocketIoClient();
                await Assert.ThrowsAnyAsync<Exception>(() => client.OpenAsync(new Uri("http://abcd")));
            }

            [Fact]
            public async Task LocalServer_ShouldConnect()
            {
                using var client = new SocketIoClient();
                var called = client.EventCalled(SocketIoEvent.Connect);
                var url = new Uri("http://localhost:3000");
                await client.OpenAsync(url).TimeoutAfterAsync(TimeSpan.FromSeconds(2));

                await called.AssertAtLeastOnceAsync(TimeSpan.FromSeconds(1));
                Assert.Equal(ReadyState.Open, client.State);
            }

            [Fact]
            public async Task Ping_ShouldReceivePong()
            {
                using var client = new SocketIoClient();
                var probeSuccess = client.EventCalled(SocketIoEvent.ProbeSuccess);
                var probeError = client.EventCalled(SocketIoEvent.ProbeError);

                await client.ConnectToLocalServerAsync();

                await probeSuccess.AssertAtLeastOnceAsync(TimeSpan.FromSeconds(1));
                probeError.AssertNever();
            }

            [Fact]
            public async Task Ping_KeepConnectionAlive_ShouldCallMultiplePings()
            {
                using var client = new SocketIoClient();
                var probeSuccess = client.EventCalled(SocketIoEvent.ProbeSuccess);
                var probeError = client.EventCalled(SocketIoEvent.ProbeError);

                await client.ConnectToLocalServerAsync();

                //ping interval is set to 50ms
                await probeSuccess.AssertAtLeastAsync(15, TimeSpan.FromSeconds(1));
                probeError.AssertNever();
            }
        }
    }
}
