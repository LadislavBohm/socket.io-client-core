using Socket.Io.Client.Core.Reactive.Model;
using System;
using System.Threading.Tasks;

namespace Socket.Io.Client.Core.Reactive
{
    public interface ISocketIoClient
    {
        SocketIoEvents Events { get; }
        bool IsRunning { get; }
        SocketIoClientOptions Options { get; }

        Task CloseAsync();
        void Dispose();
        Task OpenAsync(Uri uri);
    }
}