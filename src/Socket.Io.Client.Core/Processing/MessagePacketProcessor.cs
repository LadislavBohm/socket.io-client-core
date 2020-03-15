using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Socket.Io.Client.Core.Model.SocketEvent;
using Socket.Io.Client.Core.Model.SocketIo;
using Utf8Json;

namespace Socket.Io.Client.Core.Processing
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
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Processing message packet: {packet}");

            switch (packet.SocketIoType)
            {
                case SocketIoType.Event:
                case SocketIoType.Ack:
                    ProcessAckAndEvent(packet);
                    break;
                case SocketIoType.Connect:
                    //40 packet
                    _client.Events.ConnectSubject.OnNext(Unit.Default);
                    break;
                case SocketIoType.Disconnect:
                    ProcessDisconnect(packet);
                    break;
                case SocketIoType.Error:
                    ProcessError(packet);
                    break;
                case SocketIoType.BinaryEvent:
                case SocketIoType.BinaryAck:
                    throw new NotSupportedException();
                case null:
                    _logger.LogWarning($"Cannot handle message packet without SocketIo type. Packet: {packet}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessAckAndEvent(Packet packet)
        {
            try
            {
                var eventArray = JsonSerializer.Deserialize<string[]>(packet.Data);
                if (eventArray != null && eventArray.Length > 0)
                {
                    if (packet.Id.HasValue && _logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug($"Received packet with ACK: {packet.Id.Value}");

                    if (packet.SocketIoType == SocketIoType.Ack && packet.Id.HasValue)
                    {
                        _client.Events.AckMessageSubject.OnNext(new AckMessageEvent(packet.Id.Value, eventArray));
                    }
                    else
                    {
                        //first element should contain event name
                        //we can have zero, one or multiple arguments after event name so emit based on number of them
                        var message = eventArray.Length == 1
                            ? new EventMessageEvent(eventArray[0], new List<string>())
                            : new EventMessageEvent(eventArray[0], eventArray[1..]);
                        _client.Events.EventMessageSubject.OnNext(message);
                    }
                }
            }
            catch (JsonParsingException ex)
            {
                _logger.LogError(ex, $"Error while deserializing event message. Packet: {packet}");
                _client.Events.ErrorSubject.OnNext(new ErrorEvent(ex, "Error while deserializing event message"));
            }
        }

        private void ProcessDisconnect(Packet packet)
        {
            _logger.LogWarning($"Received disconnect packet from server. Data: {packet.Data}");
            try
            {
                //according to JS client we should completely destroy the socket since it will get disconnected anyways
                //this might be a place to do reconnect in the future?
                _client.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while processing disconnect packet. Packet: {packet}");
                _client.Events.DisconnectSubject.OnError(ex);
            }
        }

        private void ProcessError(Packet packet)
        {
            _logger.LogWarning($"Received error packet from server. Data: {packet.Data}");

            //probably don't deserialize here because we don't know how data object might look like or if it even is an object
            _client.Events.ErrorSubject.OnNext(new ErrorEvent(packet.Data));
        }
    }
}