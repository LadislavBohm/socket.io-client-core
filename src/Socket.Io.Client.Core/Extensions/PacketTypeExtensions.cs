using Socket.Io.Client.Core.Model;

namespace Socket.Io.Client.Core.Extensions
{
    public static class PacketTypeExtensions
    {
        public static bool IsBinaryType(this PacketSubType type) =>
            type == PacketSubType.BinaryAck || type == PacketSubType.BinaryEvent;
    }
}