using System.Text;
using System.Text.Json;

namespace Socket.Io.Client.Core
{
    public class SocketIoOptions
    {
        internal SocketIoOptions() : this(Encoding.UTF8, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, IgnoreNullValues = true
        })
        {
        }

        public SocketIoOptions(Encoding encoding, JsonSerializerOptions jsonSerializerOptions)
        {
            Encoding = encoding;
            JsonSerializerOptions = jsonSerializerOptions;
        }
        
        public Encoding Encoding { get; }
        
        public JsonSerializerOptions JsonSerializerOptions { get; }
    }
}