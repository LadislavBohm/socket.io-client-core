using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Socket.Io.Client.Core.Json;
using Utf8Json.Resolvers;

namespace Socket.Io.Client.Core.Model
{
    public class SocketIoClientOptions
    {
        public SocketIoClientOptions() : this(new Utf8JsonSerializer(StandardResolver.CamelCase), NullLogger<SocketIoClient>.Instance, Encoding.UTF8)
        {
        }

        public SocketIoClientOptions(IJsonSerializer jsonSerializer, ILogger<SocketIoClient> logger, Encoding encoding)
        {
            JsonSerializer = jsonSerializer;
            Logger = logger;
            Encoding = encoding;
        }

        public IJsonSerializer JsonSerializer { get; }

        public ILogger<SocketIoClient> Logger { get; }

        public Encoding Encoding { get; }

        public SocketIoClientOptions With(
            IJsonSerializer jsonSerializer = null, 
            ILogger<SocketIoClient> logger = null,
            Encoding encoding = null)
        {
            return new SocketIoClientOptions(jsonSerializer ?? JsonSerializer, logger ?? Logger, encoding ?? Encoding);
        }
    }
}
