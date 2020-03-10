using System;
using System.Threading.Tasks;
using Socket.Io.Csharp.Core.Extensions;
using Socket.Io.Csharp.Core.Model;
using Socket.Io.Csharp.Core.Test.Extensions;
using Xunit;

namespace Socket.Io.Csharp.Core.Test
{
    public class SocketIoClientTest
    {
        public class Connect
        {
            [Fact]
            public async Task InvalidUrl_ShouldThrow()
            {
                var client = new SocketIoClient();
                await Assert.ThrowsAnyAsync<Exception>(() => client.OpenAsync(new Uri("http://abcd")));
            }

            [Fact]
            public async Task LocalServer_ShouldConnect()
            {
                var client = new SocketIoClient();
                var called = client.EventCalled(SocketIoEvent.Connect);
                var url = new Uri("http://localhost:3000").HttpToSocketIoWs();
                await client.OpenAsync(url).WaitForAsync(TimeSpan.FromSeconds(2));

                await called.AssertAtLeastOnceAsync(TimeSpan.FromSeconds(1));
                Assert.Equal(ReadyState.Open, client.State);
            }

            [Fact]
            public async Task LocalServer_Ping_ShouldReceivePong()
            {
                var client = new SocketIoClient();
                var connect = client.EventCalled(SocketIoEvent.Connect);
                var probeSuccess = client.EventCalled(SocketIoEvent.ProbeSuccess);
                var probeError = client.EventCalled(SocketIoEvent.ProbeError);

                var url = new Uri("http://localhost:3000").HttpToSocketIoWs();
                await client.OpenAsync(url).WaitForAsync(TimeSpan.FromSeconds(2));

                await Task.WhenAll(
                    connect.AssertAtLeastOnceAsync(TimeSpan.FromSeconds(1)),
                    probeSuccess.AssertAtLeastOnceAsync(TimeSpan.FromSeconds(1))
                );

                probeError.AssertNever();
            }
        }
    }
}
