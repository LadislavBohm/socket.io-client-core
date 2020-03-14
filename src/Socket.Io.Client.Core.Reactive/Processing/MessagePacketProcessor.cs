using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Socket.Io.Client.Core.Reactive.Model.SocketIo;
using Utf8Json;

namespace Socket.Io.Client.Core.Reactive.Processing
{
    internal class MessagePacketProcessor : IPacketProcessor
    {
        private readonly ISocketIoClient _client;
        private readonly ILogger _logger;

        internal MessagePacketProcessor(ISocketIoClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public void Process(Packet packet)
        {

        }

        //public async ValueTask ProcessAsync(Packet packet)
        //{
        //    if (_logger.IsEnabled(LogLevel.Trace))
        //        _logger.LogTrace($"Received message packet: {packet.Data}");

        //    if (packet.SubType == PacketSubType.Connect)
        //    {
        //        //40 packet
        //        await _emitter.EmitAsync(SocketIoEvent.Connect);
        //    }
        //    else
        //    {
        //        //regular message packet
        //        await _emitter.EmitAsync(SocketIoEvent.Message, packet.Data);

        //        if (packet.SubType == PacketSubType.Event || packet.SubType == PacketSubType.Ack)
        //        {
        //            await ParseAndEmitEventsAsync(packet);
        //        }
        //    }
        //}

        //private async ValueTask ParseAndEmitEventsAsync(Packet packet)
        //{
        //    try
        //    {
        //        var eventArray = JsonSerializer.Deserialize<string[]>(packet.Data);
        //        if (eventArray != null && eventArray.Length > 0)
        //        {
        //            //first element should contain event name
        //            //we can have zero, one or multiple arguments after event name so emit based on number of them
        //            var args = eventArray.Length == 1 ? MessageEventArgs.Empty : new MessageEventArgs(eventArray[1..]);
        //            await _emitter.EmitAsync(eventArray[0], args);

        //            if (packet.SubType == PacketSubType.Ack && packet.Id.HasValue)
        //            {
        //                await _emitter.NotifyAckAsync(packet.Id.Value, args);
        //            }
        //        }
        //    }
        //    catch (JsonException ex)
        //    {
        //        _logger.LogError(ex, $"Error while deserializing event message. Packet: {packet}");
        //    }
        //}
    }
}