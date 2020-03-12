using System;
using System.Threading.Tasks;
using Socket.Io.Client.Core.Extensions;
using Socket.Io.Client.Core.Test.Model;

namespace Socket.Io.Client.Core.Test.Extensions
{
    internal static class SocketIoClientExtensions
    {
        internal static async Task ConnectToLocalServerAsync(this SocketIoClient client, string url = "http://localhost:3000",
            TimeSpan? openTimeout = null, TimeSpan? connectTimeout = null)
        {
            openTimeout ??= TimeSpan.FromSeconds(2);
            connectTimeout ??= TimeSpan.FromSeconds(1);

            var connect = client.EventCalled(SocketIoEvent.Connect);

            var uri = new Uri(url);
            await client.OpenAsync(uri).TimeoutAfterAsync(openTimeout.Value);
            await connect.AssertAtLeastOnceAsync(connectTimeout.Value);
        }

        internal static Called EventCalled(this SocketIoClient client, SocketIoEvent ioEvent)
        {
            Called result = null;
            ValueTask Callback()
            {
                result.Increment();
                return default;
            }

            var callback = (Func<ValueTask>) Callback;
            result = new Called(callback);
            client.On(ioEvent, callback);

            return result;
        }

        internal static Called EventCalled(this SocketIoClient client, string eventName)
        {
            Called result = null;
            ValueTask Callback()
            {
                result.Increment();
                return default;
            }

            var callback = (Func<ValueTask>) Callback;
            result = new Called(callback);
            client.On(eventName, callback);

            return result;
        }

        internal static Called<T> EventCalled<T>(this SocketIoClient client, string eventName, Action<T> callback)
        {
            Called<T> result = null;
            ValueTask Callback(T args)
            {
                result.Increment();
                callback(args);
                return default;
            }

            var innerCallback = (Func<T, ValueTask>) Callback;
            result = new Called<T>(innerCallback);
            client.On(eventName, innerCallback);
            
            return result;
        }
    }
}
