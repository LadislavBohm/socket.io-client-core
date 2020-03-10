using System;
using System.Threading.Tasks;
using Socket.Io.Csharp.Core.EventArguments;

namespace Socket.Io.Csharp.Core
{
    public interface IEventEmitter
    {
        void On<TData>(string eventName, Func<TData, ValueTask> callback);
        void On(string eventName, Func<ValueTask> callback);
        void On(SocketIoEvent ioEvent, Func<ValueTask> callback);
        void On<TData>(SocketIoEvent ioEvent, Func<TData, ValueTask> callback);

        void OnOnce<TData>(SocketIoEvent ioEvent, Func<TData, ValueTask> callback);
        void OnOnce(SocketIoEvent ioEvent, Func<ValueTask> callback);

        void Off<TData>(string eventName, Func<TData, ValueTask> callback);
        void Off(string eventName, Func<ValueTask> callback);
        void Off(SocketIoEvent ioEvent, Func<ValueTask> callback);
        void Off<TData>(SocketIoEvent ioEvent, Func<TData, ValueTask> callback);

        ValueTask EmitAsync<TData>(string eventName, TData data);
        ValueTask EmitAsync<TData>(string eventName, TData data, Func<MessageEventArgs, ValueTask> callback);
        ValueTask EmitAsync(string eventName);
        ValueTask EmitAsync(string eventName, Func<MessageEventArgs, ValueTask> callback);

        internal ValueTask NotifyAckAsync(int packetId, MessageEventArgs args);
    }
}