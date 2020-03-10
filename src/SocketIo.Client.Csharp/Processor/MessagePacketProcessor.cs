using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Socket.Io.Csharp.Core.Extensions;
using Socket.Io.Csharp.Core.EventArguments;
using Socket.Io.Csharp.Core.Model;

namespace Socket.Io.Csharp.Core.Processor
{
    internal class MessagePacketProcessor : IPacketProcessor
    {
        private readonly IEventEmitter _emitter;
        private readonly ILogger _logger;

        internal MessagePacketProcessor(IEventEmitter emitter, ILogger logger)
        {
            _emitter = emitter;
            _logger = logger;
        }

        public async ValueTask ProcessAsync(Packet packet)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"Received message packet: {packet.Data}");

            if (packet.SubType == PacketSubType.Connect)
            {
                //40 packet
                await _emitter.EmitAsync(SocketIoEvent.Connect);
            }
            else
            {
                //regular message packet
                await _emitter.EmitAsync(SocketIoEvent.Message, packet.Data);

                if (packet.SubType == PacketSubType.Event || packet.SubType == PacketSubType.Ack)
                {
                    await ParseAndEmitEventsAsync(packet);
                }
            }
        }

        private async ValueTask ParseAndEmitEventsAsync(Packet packet)
        {
            try
            {
                var eventArray = JsonSerializer.Deserialize<string[]>(packet.Data);
                if (eventArray != null && eventArray.Length > 0)
                {
                    //first element should contain event name
                    //we can have zero, one or multiple arguments after event name so emit based on number of them
                    var args = eventArray.Length == 1 ? MessageEventArgs.Empty : new MessageEventArgs(eventArray[1..]);
                    await _emitter.EmitAsync(eventArray[0], args);

                    if (packet.SubType == PacketSubType.Ack && packet.Id.HasValue)
                    {
                        await _emitter.NotifyAckAsync(packet.Id.Value, args);
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Error while deserializing event message. Packet: {packet}");
            }
        }
    }
}