using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Socket.Io.Csharp.Core.Test.Model;
using Xunit;

namespace Socket.Io.Csharp.Core.Test
{
    internal static class SocketIoClientExtensions
    {
        internal static Called EventCalled(this SocketIoClient client, SocketIoEvent ioEvent)
        {
            var result = new Called();
            client.On(ioEvent, () =>
            {
                result.Increment();
                return default;
            });

            return result;
        }
    }
}
