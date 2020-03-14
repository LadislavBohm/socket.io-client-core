using Socket.Io.Client.Core.Reactive.Model;
using System;
using System.Threading.Tasks;
using Socket.Io.Client.Core.Reactive.Model.SocketEvent;

namespace Socket.Io.Client.Core.Reactive
{
    public interface ISocketIoClient
    {
        SocketIoEvents Events { get; }
        bool IsRunning { get; }
        SocketIoClientOptions Options { get; }

        Task OpenAsync(Uri uri);
        Task CloseAsync();
        void Dispose();

        IObservable<MessageEvent> Emit(string eventName);
        IObservable<MessageEvent> Emit<TData>(string eventName, TData data);
    }
}