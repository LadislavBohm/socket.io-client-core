using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Socket.Io.Client.Core.Model;
using Socket.Io.Client.Core.Test.Extensions;
using Xunit;

namespace Socket.Io.Client.Core.Test
{
    public partial class SocketIoClientTest
    {
        public class Close
        {
            [Fact]
            public async Task CloseWithoutOpen_ShouldThrow()
            {
                var client = new SocketIoClient();

                await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.CloseAsync());
            }

            [Fact]
            public async Task Open_Close_ShouldRaiseClosed()
            {
                var client = new SocketIoClient();
                var closed = client.EventCalled(SocketIoEvent.Close);

                await client.ConnectToLocalServerAsync();
                Assert.Equal(ReadyState.Open, client.State);

                await client.CloseAsync();
                closed.AssertOnce();
                Assert.Equal(ReadyState.Closed, client.State);
            }

            [Fact]
            public async Task Open_Close_Reopen_ShouldStayOpen()
            {
                var client = new SocketIoClient();
                var closed = client.EventCalled(SocketIoEvent.Close);
                var message = client.EventCalled("broadcast-message");

                await client.ConnectToLocalServerAsync();
                Assert.Equal(ReadyState.Open, client.State);

                await message.AssertAtLeastOnceAsync(TimeSpan.FromMilliseconds(200));

                await client.CloseAsync();
                closed.AssertOnce();
                Assert.Equal(ReadyState.Closed, client.State);
                
                await client.ConnectToLocalServerAsync();
                Assert.Equal(ReadyState.Open, client.State);

                message = client.EventCalled("broadcast-message");
                await message.AssertAtLeastOnceAsync(TimeSpan.FromMilliseconds(200));
            }
        }
    }
}
