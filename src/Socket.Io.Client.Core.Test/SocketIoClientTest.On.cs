using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Socket.Io.Client.Core.Model.SocketEvent;
using Socket.Io.Client.Core.Test.Extensions;
using Socket.Io.Client.Core.Test.Model;
using Xunit;
using Xunit.Abstractions;

namespace Socket.Io.Client.Core.Test
{
    public partial class SocketIoClientTest
    {
        public class On : TestBase
        {
            public On(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
            {
            }

            [Fact]
            public async Task BroadcastMessage_ShouldReceiveOnlyBroadcastMessages()
            {
                using var client = CreateClient();

                await client.OpenTestAsync();
                using var called = client.On("broadcast-message").SubscribeCalled(m =>
                {
                    Assert.Equal("broadcast-message", m.FirstData);
                });

                await called.AssertAtLeastAsync(4, TimeSpan.FromMilliseconds(150));
            }

            [Fact]
            public async Task BroadcastMessage_OnMultipleTimes_ShouldReceiveMultipleTimes()
            {
                using var client = CreateClient();

                await client.OpenTestAsync();

                var called = new List<Called<EventMessageEvent>>();
                try
                {
                    for (int i = 0; i < 5; i++)
                    {
                        called.Add(client.On("broadcast-message").SubscribeCalled(m =>
                        {
                            Assert.Equal("broadcast-message", m.FirstData);
                        }));
                    }

                    await Task.Delay(150);
                    called.ForEach(c => c.AssertAtLeast(4));
                }
                finally
                {
                    called.ForEach(c => c.Dispose());
                }
            }
        }
    }
}
