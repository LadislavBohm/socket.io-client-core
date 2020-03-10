using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Socket.Io.Csharp.Core.EventArguments;
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

        internal static Called EventCalled(this SocketIoClient client, string eventName)
        {
            var result = new Called();
            client.On(eventName, () =>
            {
                result.Increment();
                return default;
            });

            return result;
        }

        internal static Called EventCalled<T>(this SocketIoClient client, string eventName, Action<T> callback)
        {
            var result = new Called();
            client.On<T>(eventName, arg =>
            {
                result.Increment();
                callback(arg);
                return default;
            });
            
            return result;
        }
    }
}
