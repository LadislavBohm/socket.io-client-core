using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Socket.Io.Client.Core.EventArguments;
using Socket.Io.Client.Core.Model;
using Socket.Io.Client.Core.Extensions;

namespace Socket.Io.Client.Core.Processor
{
    internal class OpenPacketProcessor : IPacketProcessor
    {
        private readonly IEventEmitter _eventEmitter;
        private readonly JsonSerializerOptions _options;
        private readonly ILogger _logger;

        internal OpenPacketProcessor(IEventEmitter eventEmitter,
                                     JsonSerializerOptions options,
                                     ILogger logger)
        {
            _eventEmitter = eventEmitter;
            _options = options;
            _logger = logger;
        }

        public ValueTask ProcessAsync(Packet packet)
        {
            if (string.IsNullOrEmpty(packet.Data))
            {
                _logger.LogError($"Missing data in {packet.Type} packet.");
                _eventEmitter.EmitAsync(SocketIoEvent.Error,
                    new ErrorEventArgs("Missing data in {packet.Type} packet."));
                return default;
            }
            
            var data = JsonSerializer.Deserialize<HandshakeData>(packet.Data, _options);
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Received handshake data: {data}.");

            _eventEmitter.EmitAsync(SocketIoEvent.Handshake, data);
            _eventEmitter.EmitAsync(SocketIoEvent.Open);

            return default;
        }
    }
}