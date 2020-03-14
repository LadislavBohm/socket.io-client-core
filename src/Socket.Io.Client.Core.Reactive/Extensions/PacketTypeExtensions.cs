using Socket.Io.Client.Core.Reactive.Model.SocketIo;

namespace Socket.Io.Client.Core.Reactive.Extensions
{
    public static class PacketTypeExtensions
    {
        public static bool IsBinaryType(this SocketIoType type) =>
            type == SocketIoType.BinaryAck || type == SocketIoType.BinaryEvent;
    }
}