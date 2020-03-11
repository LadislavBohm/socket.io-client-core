using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Socket.Io.Client.Core.EventArguments;
using Socket.Io.Client.Core.Test.Extensions;
using Xunit;

namespace Socket.Io.Client.Core.Test
{
    public partial class SocketIoClientTest
    {
        public class OnMessage
        {
            [Fact]
            public async Task BroadcastMessage_ShouldReceiveMessages()
            {
                var client = new SocketIoClient();
                var message = client.EventCalled<MessageEventArgs>("broadcast-message",
                    args =>
                    {
                        Assert.Equal("broadcast-message", args.FirstData);
                    });

                await client.ConnectToLocalServerAsync();

                await message.AssertAtLeastAsync(4, TimeSpan.FromSeconds(1));
            }

            [Fact]
            public async Task OnNonExistingEvent_ShouldNotBeCalled()
            {
                var client = new SocketIoClient();
                var message = client.EventCalled<MessageEventArgs>("broadcast2",
                    args =>
                    {
                        Assert.Equal("broadcast-message", args.FirstData);
                    });

                await client.ConnectToLocalServerAsync();

                await message.AssertNeverAsync(TimeSpan.FromSeconds(1));
            }

            [Fact]
            public async Task OnOff_ShouldNotReceiveAfterOff()
            {
                var client = new SocketIoClient();
                var message = client.EventCalled<MessageEventArgs>("broadcast-message",
                    args =>
                    {
                        Assert.Equal("broadcast-message", args.FirstData);
                    });

                await client.ConnectToLocalServerAsync();

                await message.AssertAtLeastAsync(4, TimeSpan.FromMilliseconds(500));
                var calledCount = message.CalledTimes;
                client.Off("broadcast-message", message.Callback);

                await Task.Delay(500);
                Assert.Equal(calledCount, message.CalledTimes);

                client.On("broadcast-message", message.Callback);
                await message.AssertAtLeastAsync(calledCount + 4, TimeSpan.FromMilliseconds(500));
            }

            [Fact]
            public async Task ConnectToRoom_ShouldReceiveRoomMessage()
            {
                var client = new SocketIoClient();
                var message = client.EventCalled<MessageEventArgs>("room-message",
                    args =>
                    {
                        Assert.Equal("room-message", args.FirstData);
                    });
                await client.ConnectToLocalServerAsync();

                await message.AssertAtLeastOnceAsync(TimeSpan.FromSeconds(1));
            }

            [Fact]
            public async Task ConnectToNamespace_ShouldReceiveNamespaceMessage()
            {
                var client = new SocketIoClient();
                var message = client.EventCalled<MessageEventArgs>("namespace-message",
                    args =>
                    {
                        Assert.Equal("namespace-message", args.FirstData);
                    });
                await client.ConnectToLocalServerAsync("http://localhost:3000/some-namespace");

                await message.AssertAtLeastOnceAsync(TimeSpan.FromSeconds(1));
            }
        }
    }
}
