using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Socket.Io.Client.Core.Reactive.Model.SocketEvent;
using Socket.Io.Client.Core.Reactive.Test.Extensions;
using Socket.Io.Client.Core.Reactive.Test.Model;
using Xunit;
using Xunit.Abstractions;

namespace Socket.Io.Client.Core.Reactive.Test
{
    public partial class SocketIoClientTest
    {
        public class Emit : TestBase
        {
            public Emit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
            {
            }

            [Fact]
            public async Task WithAck_ShouldReceiveAckMessage()
            {
                using var client = CreateClient();

                await client.OpenAsync(new Uri("http://localhost:3000"));
                using var called = client.Emit("ack-message").SubscribeCalled(m =>
                {
                    Assert.Equal("ack-response", m.FirstData);
                });
                
                await called.AssertExactlyOnceAsync(TimeSpan.FromMilliseconds(100));
            }

            [Fact]
            public async Task WithAck_MultipleMessages_ShouldReceiveOneForEach()
            {
                using var client = CreateClient();

                await client.OpenAsync(new Uri("http://localhost:3000"));
                var messages = new List<Called<MessageEvent>>();
                try
                {
                    for (int i = 0; i < 10; i++)
                    {
                        messages.Add(client.Emit("ack-message").SubscribeCalled(m =>
                        {
                            Assert.Equal("ack-response", m.FirstData);
                        }));
                    }

                    await Task.Delay(100);
                    messages.ForEach(m => m.AssertOnce());
                }
                finally
                {
                    messages.ForEach(m => m.Dispose());
                }
            }

            [Fact]
            public async Task WithAck_MultipleMessages_Parallel_ShouldReceiveOneForEach()
            {
                using var client = CreateClient();

                await client.OpenAsync(new Uri("http://localhost:3000"));
                var messages = new List<Called<MessageEvent>>();
                try
                {
                    Parallel.For(0, 10, i =>
                    {
                        messages.Add(client.Emit("ack-message").SubscribeCalled(m =>
                        {
                            Assert.Equal("ack-response", m.FirstData);
                        }));
                    });

                    await Task.Delay(100);
                    messages.ForEach(m => m.AssertOnce());
                }
                finally
                {
                    messages.ForEach(m => m.Dispose());
                }
            }
        }
    }
}
