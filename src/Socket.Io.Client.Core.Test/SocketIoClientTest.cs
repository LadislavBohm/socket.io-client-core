using System;
using System.Threading.Tasks;
using Socket.Io.Client.Core.EventArguments;
using Socket.Io.Client.Core.Extensions;
using Socket.Io.Client.Core.Model;
using Socket.Io.Client.Core.Test.Extensions;
using Xunit;

namespace Socket.Io.Client.Core.Test
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
            public async Task Ping_ShouldReceivePong()
            {
                var client = new SocketIoClient();
                var probeSuccess = client.EventCalled(SocketIoEvent.ProbeSuccess);
                var probeError = client.EventCalled(SocketIoEvent.ProbeError);

                await client.ConnectToLocalServerAsync();

                await probeSuccess.AssertAtLeastOnceAsync(TimeSpan.FromSeconds(1));
                probeError.AssertNever();
            }

            [Fact]
            public async Task OnMessage_ShouldReceiveMessages()
            {
                var client = new SocketIoClient();
                var message = client.EventCalled<MessageEventArgs>("broadcast", args =>
                {
                    Assert.Equal("broadcast message", args.FirstData);
                });
                
                await client.ConnectToLocalServerAsync();

                await message.AssertAtLeastAsync(4, TimeSpan.FromSeconds(1));
            }

            [Fact]
            public async Task OnNonExistingEvent_ShouldNotBeCalled()
            {
                var client = new SocketIoClient();
                var message = client.EventCalled<MessageEventArgs>("broadcast2", args =>
                {
                    Assert.Equal("broadcast message", args.FirstData);
                });

                await client.ConnectToLocalServerAsync();
                
                await message.AssertNeverAsync(TimeSpan.FromSeconds(1));
            }

            [Fact]
            public async Task OnMessage_OffMessage_ShouldNotReceiveAfterOff()
            {
                var client = new SocketIoClient();
                var message = client.EventCalled<MessageEventArgs>("broadcast", args =>
                {
                    Assert.Equal("broadcast message", args.FirstData);
                });

                await client.ConnectToLocalServerAsync();

                await message.AssertAtLeastAsync(4, TimeSpan.FromMilliseconds(500));
                var calledCount = message.CalledTimes;
                client.Off("broadcast", message.Callback);

                await Task.Delay(500);
                Assert.Equal(calledCount, message.CalledTimes);

                client.On("broadcast", message.Callback);
                await message.AssertAtLeastAsync(calledCount + 4, TimeSpan.FromMilliseconds(500));
            }
        }
    }
}
