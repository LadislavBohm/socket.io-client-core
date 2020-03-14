using System;
using System.Text;
using Socket.Io.Client.Core.Reactive.Extensions;
using Socket.Io.Client.Core.Reactive.Model.SocketIo;

namespace Socket.Io.Client.Core.Reactive.Parse
{
    public static class PacketParser
    {
        internal static ReadOnlyMemory<byte> Encode(Packet packet, Encoding encoding) => encoding.GetBytes(Encode(packet));

        internal static string Encode(Packet packet)
        {
            var sb = new StringBuilder();
            sb.Append((int)packet.EngineIoType);

            if (packet.SocketIoType.HasValue)
                sb.Append((int)packet.SocketIoType);

            // implement this mechanism once binary packets are supported
            // if ((obj.type == EVENT || obj.type == ACK) && HasBinary.hasBinary(obj.data)) {
            //     obj.type = obj.type == EVENT ? BINARY_EVENT : BINARY_ACK;
            // }

            if (!string.IsNullOrEmpty(packet.Namespace) && packet.Namespace != SocketIo.DefaultNamespace)
            {
                sb.Append(packet.Namespace);
                sb.Append(",");
            }

            if (packet.Id.HasValue) sb.Append(packet.Id.Value);
            if (!string.IsNullOrEmpty(packet.Data)) sb.Append(packet.Data);
            if (sb[^1] == ',') sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        internal static bool TryDecode(ReadOnlyMemory<byte> data, Encoding encoding, out Packet packet)
        {
            packet = null;
            if (data.IsEmpty) return false;

            string dataString = encoding.GetString(data.Span);
            return TryDecode(dataString, out packet);
        }

        internal static bool TryDecode(string data, out Packet packet)
        {
            packet = null;
            if (string.IsNullOrEmpty(data)) return false;

            var span = data.AsSpan();
            if (!TryParsePacketType(span.Slice(0, 1), out var type))
                return false;

            switch (type)
            {
                case EngineIoType.Open:
                    //handshake packet
                    packet = new Packet(type, SocketIoType.Connect, null, span.Slice(1).ToString(), null, 0, null);
                    return true;
                case EngineIoType.Ping:
                    packet = new Packet(type, null, null, span.Slice(1).ToString(), null, 0, null);
                    return true;
                case EngineIoType.Pong:
                    packet = new Packet(type, null, null, span.Slice(1).ToString(), null, 0, null);
                    return true;
            }

            var subType = ParsePacketSubType(span.Slice(1, 1));
            var withoutTypes = span.Slice(2);
            var @namespace = ParseNamespace(ref withoutTypes);
            var id = ParsePacketId(ref withoutTypes);

            packet = new Packet(type, subType, @namespace, withoutTypes.ToString(), id, 0, null);
            return true;
        }

        private static bool TryParsePacketType(ReadOnlySpan<char> typeSpan, out EngineIoType type)
        {
            type = default;
            if (!int.TryParse(typeSpan, out int intType)) return false;

            type = (EngineIoType)intType;
            if (!Enum.IsDefined(typeof(EngineIoType), type)) return false;
            
            return true;
        }

        private static SocketIoType ParsePacketSubType(ReadOnlySpan<char> typeSpan)
        {
            if (!int.TryParse(typeSpan, out int intType))
                ThrowInvalidDataException();
            var subType = (SocketIoType)intType;
            if (!Enum.IsDefined(typeof(SocketIoType), subType)) ThrowInvalidDataException($"Invalid packet type: {intType}");
            if (subType.IsBinaryType())
                throw new NotImplementedException();
            return subType;
        }

        private static int? ParsePacketId(ref ReadOnlySpan<char> span)
        {
            if (span.IsEmpty || span[0] == '[') return null;
            var sb = new StringBuilder();
            foreach (var c in span)
            {
                if (!char.IsDigit(c)) break;
                sb.Append(c);
            }

            if (!int.TryParse(sb.ToString(), out var packetId))
                return null;

            span = span.Slice(sb.Length);
            return packetId;
        }

        private static string ParseNamespace(ref ReadOnlySpan<char> span)
        {
            if (!span.IsEmpty && span[0] == '/')
            {
                var sb = new StringBuilder();
                foreach (var c in span)
                {
                    if (c == ',') break;
                    sb.Append(c);
                }

                var result = sb.ToString();
                span = span.Slice(result.Length + 1); //remove also the ',' character
                return result;
            }

            return null;
        }

        private static void ThrowInvalidDataException(string reason = null) => throw new ArgumentException(reason ?? $"Invalid packet data.");
    }
}