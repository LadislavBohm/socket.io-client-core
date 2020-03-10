using System;
using System.Threading.Tasks;
using Socket.Io.Csharp.Core.Model;

namespace Socket.Io.Csharp.Core
{
    public interface ISocketIoClient
    {
        ReadyState State { get; }
        
        Task OpenAsync(Uri uri);
        Task CloseAsync();
        internal ValueTask SendAsync(Packet packet);
    }
}