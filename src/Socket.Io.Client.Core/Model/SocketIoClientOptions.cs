using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Socket.Io.Client.Core.Model
{
    public class SocketIoClientOptions
    {
        public SocketIoClientOptions() : this(
            new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase},
            NullLogger<SocketIoClient>.Instance, Encoding.UTF8)
        {
        }

        public SocketIoClientOptions(JsonSerializerOptions jsonSerializerOptions, ILogger<SocketIoClient> logger, Encoding encoding)
        {
            JsonSerializerOptions = jsonSerializerOptions;
            Logger = logger;
            Encoding = encoding;
        }

        public JsonSerializerOptions JsonSerializerOptions { get; }

        public ILogger<SocketIoClient> Logger { get; }

        public Encoding Encoding { get; }

        public SocketIoClientOptions With(
            JsonSerializerOptions jsonSerializer = null, 
            ILogger<SocketIoClient> logger = null,
            Encoding encoding = null)
        {
            return new SocketIoClientOptions(jsonSerializer ?? JsonSerializerOptions, logger ?? Logger, encoding ?? Encoding);
        }
    }
}
