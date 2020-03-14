using System;
using System.Threading.Tasks;
using Socket.Io.Client.Core.Model;
using Socket.Io.Client.Core.Model.SocketEvent;

namespace Socket.Io.Client.Core
{
    public interface ISocketIoClient
    {
        SocketIoEvents Events { get; }
        bool IsRunning { get; }
        SocketIoClientOptions Options { get; }

        Task OpenAsync(Uri uri);
        Task CloseAsync();
        void Dispose();

        IObservable<AckMessageEvent> Emit(string eventName);
        IObservable<AckMessageEvent> Emit<TData>(string eventName, TData data);

        IObservable<EventMessageEvent> On(string eventName);
    }
}