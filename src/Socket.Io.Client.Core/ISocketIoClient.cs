using System;
using System.Threading.Tasks;
using Socket.Io.Client.Core.Model;

namespace Socket.Io.Client.Core
{
    public interface ISocketIoClient : IDisposable
    {
        ReadyState State { get; }
        
        Task OpenAsync(Uri uri);
        Task CloseAsync();
        internal ValueTask SendAsync(Packet packet);
    }
}