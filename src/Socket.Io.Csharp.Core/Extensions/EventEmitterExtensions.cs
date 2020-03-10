using System.Threading.Tasks;

namespace Socket.Io.Csharp.Core.Extensions
{
    internal static class EventEmitterExtensions
    {
        internal static ValueTask EmitAsync<TData>(this IEventEmitter emitter, SocketIoEvent ioEvent, TData data)
        {
            return emitter.EmitAsync(SocketIo.Event.Name[ioEvent], data);
        }

        internal static ValueTask EmitAsync(this IEventEmitter emitter, SocketIoEvent ioEvent)
        {
            return emitter.EmitAsync(SocketIo.Event.Name[ioEvent]);
        }
    }
}