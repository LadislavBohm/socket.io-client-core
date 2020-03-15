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
        public class Emit : TestBase
        {
            public Emit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
            {
            }

            [Fact]
            public async Task WithAck_ShouldReceiveAckMessage()
            {
                using var client = CreateClient();

                await client.OpenTestAsync();
                using var called = client.Emit("ack-message").SubscribeCalled(m =>
                {
                    Assert.Equal("ack-response", m.FirstData);
                });
                
                await called.AssertOnceAsync(TimeSpan.FromMilliseconds(100));
            }

            [Fact]
            public async Task WithAck_MultipleMessages_ShouldReceiveOneForEach()
            {
                using var client = CreateClient();

                await client.OpenTestAsync();
                var messages = new List<Called<AckMessageEvent>>();
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

                await client.OpenTestAsync();
                var messages = new List<Called<AckMessageEvent>>();
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

            [Fact]
            public async Task Join_NewRoom_ShouldReceiveInitialRoomMessage()
            {
                using var client = CreateClient();

                await client.OpenTestAsync();

                var roomWelcome = client.On("new-room").SubscribeCalled(e =>
                {
                    Assert.Equal("welcome", e.FirstData);
                });
                var roomJoined = client.Emit("join", "new-room").SubscribeCalled(e =>
                {
                    Assert.Equal("joined", e.FirstData);
                });

                await Task.WhenAll(
                    roomJoined.AssertOnceAsync(TimeSpan.FromMilliseconds(100)),
                    roomWelcome.AssertOnceAsync(TimeSpan.FromMilliseconds(100))
                );
            }

            [Fact]
            public async Task Join_TwoSockets_ShouldBothReceiveMessages()
            {
                using var clientA = CreateClient();
                using var clientB = CreateClient();

                string roomName = Guid.NewGuid().ToString("N");

                await Task.WhenAll(
                    clientA.OpenTestAsync(),
                    clientB.OpenTestAsync());

                await Task.WhenAll(
                    clientA.JoinRoomAsync(roomName),
                    clientB.JoinRoomAsync(roomName));

                bool aSending = true;
                using var calledA = clientA.On(roomName).SubscribeCalled(e =>
                {
                    Assert.Equal(aSending ? "a-message" : "b-message", e.FirstData);
                });
                using var calledB = clientB.On(roomName).SubscribeCalled(e =>
                {
                    Assert.Equal(aSending ? "a-message" : "b-message", e.FirstData);
                });

                clientA.Emit(roomName, "a-message");
                await Task.Delay(150);


                await Task.WhenAll(
                    calledA.AssertOnceAsync(TimeSpan.FromMilliseconds(50)),
                    calledB.AssertOnceAsync(TimeSpan.FromMilliseconds(50))
                );

                aSending = false;

                clientA.Emit(roomName, "b-message");

                await Task.WhenAll(
                    calledA.AssertExactlyAsync(2,TimeSpan.FromMilliseconds(50)),
                    calledB.AssertExactlyAsync(2, TimeSpan.FromMilliseconds(50))
                );
            }
        }
    }
}
