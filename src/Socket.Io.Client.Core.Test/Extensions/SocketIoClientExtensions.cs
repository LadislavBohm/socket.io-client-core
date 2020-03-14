using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Socket.Io.Client.Core.Test.Extensions
{
    public static class SocketIoClientExtensions
    {
        public static async Task JoinRoomAsync(this SocketIoClient client, string roomName)
        {
            using var called = client.Emit("join", roomName).SubscribeCalled(e =>
                {
                    Assert.Equal("joined", e.FirstData);
                });

            await called.AssertOnceAsync(TimeSpan.FromMilliseconds(100));
        }
    }
}
