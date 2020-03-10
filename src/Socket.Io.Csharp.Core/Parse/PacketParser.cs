using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using EnumsNET;
using Socket.Io.Csharp.Core.Extensions;
using Socket.Io.Csharp.Core.Model;

namespace Socket.Io.Csharp.Core.Parse
{
    public static class PacketParser
    {
        internal static ReadOnlyMemory<byte> Encode(Packet packet, Encoding encoding)
        {
            var sb = new StringBuilder();
            sb.Append((int)packet.Type);

            if (packet.SubType.HasValue)
                sb.Append((int)packet.SubType);

            // implement this mechanism once binary packets are supported
            // if ((obj.type == EVENT || obj.type == ACK) && HasBinary.hasBinary(obj.data)) {
            //     obj.type = obj.type == EVENT ? BINARY_EVENT : BINARY_ACK;
            // }

            if (!string.IsNullOrEmpty(packet.Namespace) && packet.Namespace != "/")
            {
                sb.Append(packet.Namespace);
                sb.Append(",");
            }

            if (packet.Id.HasValue) sb.Append(packet.Id.Value);
            if (!string.IsNullOrEmpty(packet.Data)) sb.Append(packet.Data);

            return encoding.GetBytes(sb.ToString());
        }

        internal static bool TryDecode(ReadOnlyMemory<byte> data, Encoding encoding, out Packet packet)
        {
            packet = null;
            if (data.IsEmpty) return false;

            string dataString = encoding.GetString(data.Span);
            var span = dataString.AsSpan();
            if (!TryParsePacketType(span.Slice(0, 1), out var type))
                return false;

            switch (type)
            {
                case PacketType.Open:
                    //handshake packet
                    packet = new Packet(type, PacketSubType.Connect, null, span.Slice(1).ToString(), null, 0, null);
                    return true;
                case PacketType.Ping:
                    packet = new Packet(type, null, null, span.Slice(1).ToString(), null, 0, null);
                    return true;
                case PacketType.Pong:
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

        private static bool TryParsePacketType(ReadOnlySpan<char> typeSpan, out PacketType type)
        {
            type = default;
            if (!int.TryParse(typeSpan, out int intType)) return false;

            type = (PacketType)intType;
            if (!type.IsDefined()) return false;
            
            return true;
        }

        private static PacketSubType ParsePacketSubType(ReadOnlySpan<char> typeSpan)
        {
            if (!int.TryParse(typeSpan, out int intType))
                ThrowInvalidDataException();
            var subType = (PacketSubType)intType;
            if (!subType.IsDefined()) ThrowInvalidDataException($"Invalid packet type: {intType}");
            if (subType.IsBinaryType())
                throw new NotImplementedException();
            return subType;
        }

        private static int? ParsePacketId(ref ReadOnlySpan<char> span)
        {
            if (span.IsEmpty) return null;
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
                span = span.Slice(result.Length);
                return result;
            }

            return null;
        }

        private static void ThrowInvalidDataException(string reason = null) => throw new ArgumentException(reason ?? $"Invalid packet data.");
    }
}