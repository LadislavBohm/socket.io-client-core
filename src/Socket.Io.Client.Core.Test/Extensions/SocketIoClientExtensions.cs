using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Socket.Io.Client.Core.Model;
using Xunit;

namespace Socket.Io.Client.Core.Test.Extensions
{
    public static class SocketIoClientExtensions
    {
        public static Task OpenTestAsync(this SocketIoClient client, Uri uri = null, string path = "some-path")
        {
            uri ??= new Uri("http://localhost:8764");
            return client.OpenAsync(uri, new SocketIoOpenOptions(path));
        }

        public static async Task JoinRoomAsync(this SocketIoClient client, string roomName)
        {
            using var called = client.Emit("join", roomName).SubscribeCalled(e =>
                {
                    Assert.Equal("joined", e.Data[0].ToString());
                });

            await called.AssertOnceAsync(TimeSpan.FromMilliseconds(100));
        }
    }
}
