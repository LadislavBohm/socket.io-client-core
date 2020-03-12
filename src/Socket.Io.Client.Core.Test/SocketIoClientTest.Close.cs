using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Socket.Io.Client.Core.Model;
using Socket.Io.Client.Core.Test.Extensions;
using Socket.Io.Client.Core.Test.Model;
using Xunit;
using Xunit.Abstractions;

namespace Socket.Io.Client.Core.Test
{
    public partial class SocketIoClientTest
    {
        public class Close : TestBase
        {
            public Close(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
            {
            }

            [Fact]
            public async Task CloseWithoutOpen_ShouldThrow()
            {
                using var client = CreateClient();
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.CloseAsync());
            }

            [Fact]
            public async Task Open_Close_ShouldRaiseClosed()
            {
                using var client = CreateClient();
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
                using var client = CreateClient();
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
