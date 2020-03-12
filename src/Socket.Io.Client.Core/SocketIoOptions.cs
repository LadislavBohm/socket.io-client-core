using System;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Socket.Io.Client.Core
{
    public class SocketIoOptions
    {
        internal SocketIoOptions() : this(
            Encoding.UTF8, 
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, IgnoreNullValues = true }, 
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = true },
            1)
        {
        }

        public SocketIoOptions(Encoding encoding, JsonSerializerOptions jsonSerializerOptions,
            ChannelOptions packetChannelOptions, int packetProcessingThreadCount)
        {
            Encoding = encoding;
            JsonSerializerOptions = jsonSerializerOptions;
            PacketChannelOptions = packetChannelOptions;
            PacketProcessingThreadCount = packetProcessingThreadCount;
        }

        public Encoding Encoding { get; }

        public JsonSerializerOptions JsonSerializerOptions { get; }

        public ChannelOptions PacketChannelOptions { get; }

        public int PacketProcessingThreadCount { get; }
    }
}