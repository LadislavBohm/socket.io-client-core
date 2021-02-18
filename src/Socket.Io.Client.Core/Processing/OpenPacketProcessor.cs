﻿using System.IO;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Socket.Io.Client.Core.Model.Response;
using Socket.Io.Client.Core.Model.SocketEvent;
using Socket.Io.Client.Core.Model.SocketIo;

namespace Socket.Io.Client.Core.Processing
{
    internal class OpenPacketProcessor : IPacketProcessor
    {
        private readonly ISocketIoClient _socket;
        private readonly ILogger _logger;

        internal OpenPacketProcessor(ISocketIoClient socket, ILogger logger)
        {
            _socket = socket;
            _logger = logger;
        }

        public void Process(Packet packet)
        {
            if (string.IsNullOrEmpty(packet.Data))
            {
                _logger.LogError($"Missing data in {packet.EngineIoType} packet.");
                _socket.Events.ErrorSubject.OnNext(new ErrorEvent($"Missing data in {packet.EngineIoType} packet."));
            }

            var handshake = JsonSerializer.Deserialize<HandshakeResponse>(packet.Data, _socket.Options.JsonSerializerOptions);
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Received handshake data: {handshake}.");

            _socket.Events.HandshakeSubject.OnNext(handshake);
            _socket.Events.OpenSubject.OnNext(Unit.Default);
        }
    }
}