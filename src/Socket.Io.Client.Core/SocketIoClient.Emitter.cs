using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Socket.Io.Client.Core.EventArguments;

namespace Socket.Io.Client.Core
{
    public partial class SocketIoClient : IEventEmitter
    {
        public void On(SocketIoEvent ioEvent, Func<ValueTask> callback) => On(SocketIo.Event.Name[ioEvent], callback);

        public void On<TData>(SocketIoEvent ioEvent, Func<TData, ValueTask> callback) => On(SocketIo.Event.Name[ioEvent], callback);

        public void Off<TData>(string eventName, Func<TData, ValueTask> callback)
        {
            if (_events.TryGetValue(eventName, out var callbacks))
                callbacks.Remove(callback);
        }

        public void Off(string eventName, Func<ValueTask> callback)
        {
            if (_events.TryGetValue(eventName, out var callbacks))
                callbacks.Remove(callback);
        }

        public void Off(SocketIoEvent ioEvent, Func<ValueTask> callback) => Off(SocketIo.Event.Name[ioEvent], callback);

        public void Off<TData>(SocketIoEvent ioEvent, Func<TData, ValueTask> callback) => Off(SocketIo.Event.Name[ioEvent], callback);

        public void On(string eventName, Func<ValueTask> callback)
        {
            if (!_events.TryGetValue(eventName, out var callbacks))
            {
                callbacks = new List<object>();
                _events[eventName] = callbacks;
            }

            callbacks.Add(callback);
        }

        public void On<TData>(string eventName, Func<TData, ValueTask> callback)
        {
            if (!_events.TryGetValue(eventName, out var callbacks))
            {
                callbacks = new List<object>();
                _events[eventName] = callbacks;
            }

            callbacks.Add(callback);
        }

        public void OnOnce<TData>(SocketIoEvent ioEvent, Func<TData, ValueTask> callback)
        {
            async ValueTask Wrapper(TData data)
            {
                try
                {
                    await callback(data);
                }
                finally
                {
                    Off<TData>(ioEvent, Wrapper);
                }
            }
            On(ioEvent, (Func<TData, ValueTask>)Wrapper);
        }

        public void OnOnce(SocketIoEvent ioEvent, Func<ValueTask> callback)
        {
            async ValueTask Wrapper()
            {
                try
                {
                    await callback();
                }
                finally
                {
                    Off(ioEvent, Wrapper);
                }
            }
            On(ioEvent, Wrapper);
        }

        public ValueTask EmitAsync<TData>(string eventName, TData data) => EmitAsync(eventName, data, null);

        public ValueTask EmitAsync(string eventName) => EmitAsync(eventName, (Func<ValueTask>)null);

        public async ValueTask EmitAsync<TData>(string eventName, TData data, Func<MessageEventArgs, ValueTask> callback)
        {
            if (_events.TryGetValue(eventName, out var callbacks))
            {
                //need to do for-loop since callbacks might be modified (removed)
                for (int i = 0; i < callbacks.Count; i++)
                {
                    if (callbacks[i] is Func<TData, ValueTask> withData)
                    {
                        await withData(data);
                    }
                    else if (callbacks[i] is Func<ValueTask> withoutData)
                    {
                        await withoutData();
                    }
                }
            }
            else
            {
                await SendEmitAsync(eventName, data, StoreCallbackWithPacketId(callback));
            }
        }

        public async ValueTask EmitAsync(string eventName, Func<MessageEventArgs, ValueTask> callback)
        {
            if (_events.TryGetValue(eventName, out var callbacks))
            {
                //need to do for-loop since callbacks might be modified (removed via OnOnce)
                for (int i = 0; i < callbacks.Count; i++)
                {
                    if (callbacks[i] is Func<MessageEventArgs, ValueTask> withData)
                    {
                        await withData(MessageEventArgs.Empty);
                    }
                    else if (callbacks[i] is Func<ValueTask> withoutData)
                    {
                        await withoutData();
                    }
                }
            }
            else
            {
                await SendEmitAsync<string>(eventName, null, StoreCallbackWithPacketId(callback));
            }
        }

        ValueTask IEventEmitter.NotifyAckAsync(int packetId, MessageEventArgs args)
        {
            if (!_callbacks.TryGetValue(packetId, out var callback))
            {
                _logger.LogWarning($"Could not find ACK callback for packet with ID: {packetId}");
                return default;
            }

            return ((Func<MessageEventArgs, ValueTask>)callback)(args);
        }

        private int? StoreCallbackWithPacketId(Func<MessageEventArgs, ValueTask> callback)
        {
            int? packetId = null;
            if (callback != null)
            {
                packetId = GetNextPacketId();
                _callbacks.AddOrUpdate(packetId.Value, callback, (i, o) => callback);
            }

            return packetId;
        }
    }
}