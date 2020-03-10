using System;
using System.Threading.Tasks;
using Socket.Io.Csharp.Core.EventArguments;
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
                await client.OpenAsync(url).TimoutAfterAsync(TimeSpan.FromSeconds(2));

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
                await client.OpenAsync(url).TimoutAfterAsync(TimeSpan.FromSeconds(2));

                await Task.WhenAll(
                    connect.AssertAtLeastOnceAsync(TimeSpan.FromSeconds(1)),
                    probeSuccess.AssertAtLeastOnceAsync(TimeSpan.FromSeconds(1))
                );

                probeError.AssertNever();
            }

            [Fact]
            public async Task LocalServer_OnMessage_ShouldReceiveMessages()
            {
                var client = new SocketIoClient();
                var connect = client.EventCalled(SocketIoEvent.Connect);
                var message = client.EventCalled<MessageEventArgs>("broadcast", args =>
                {
                    Assert.Equal("broadcast message", args.FirstData);
                });
                var url = new Uri("http://localhost:3000").HttpToSocketIoWs();
                
                await client.OpenAsync(url).TimoutAfterAsync(TimeSpan.FromSeconds(2));
                await connect.AssertAtLeastOnceAsync(TimeSpan.FromSeconds(1));

                await message.AssertAtLeastAsync(4, TimeSpan.FromSeconds(1));
            }

            [Fact]
            public async Task LocalServer_OnNonExistingEvent_ShouldNotBeCalled()
            {
                var client = new SocketIoClient();
                var connect = client.EventCalled(SocketIoEvent.Connect);
                var message = client.EventCalled<MessageEventArgs>("broadcast2", args =>
                {
                    Assert.Equal("broadcast message", args.FirstData);
                });
                var url = new Uri("http://localhost:3000").HttpToSocketIoWs();
                
                await client.OpenAsync(url).TimoutAfterAsync(TimeSpan.FromSeconds(2));
                await connect.AssertAtLeastOnceAsync(TimeSpan.FromSeconds(1));

                await message.AssertNeverAsync(TimeSpan.FromSeconds(1));
            }
        }
    }
}
