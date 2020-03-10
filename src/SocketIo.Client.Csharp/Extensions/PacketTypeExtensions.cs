using Socket.Io.Csharp.Core.Model;

namespace Socket.Io.Csharp.Core.Extensions
{
    public static class PacketTypeExtensions
    {
        public static bool IsBinaryType(this PacketSubType type) =>
            type == PacketSubType.BinaryAck || type == PacketSubType.BinaryEvent;
    }
}