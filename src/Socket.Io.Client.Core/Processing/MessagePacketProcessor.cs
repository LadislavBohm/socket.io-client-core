using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Socket.Io.Client.Core.Model.SocketEvent;
using Socket.Io.Client.Core.Model.SocketIo;

namespace Socket.Io.Client.Core.Processing
{
    internal class MessagePacketProcessor : IPacketProcessor
    {
        private static readonly List<JsonElement> EmptyList = new List<JsonElement>();
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
                using var jsonData = JsonDocument.Parse(packet.Data);
                var arrayLength = jsonData.RootElement.GetArrayLength();

                if (packet.Id.HasValue && _logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"Received packet with ACK: {packet.Id.Value}");

                if (packet.SocketIoType == SocketIoType.Ack && packet.Id.HasValue)
                {
                    var data = jsonData.RootElement.EnumerateArray().ToList();
                    _client.Events.AckMessageSubject.OnNext(new AckMessageEvent(packet.Id.Value, data));
                }
                else if (arrayLength > 0)
                {
                    //first element should contain event name
                    //we can have zero, one or multiple arguments after event name so emit based on number of them
                    JsonElement? ioEvent = null;
                    var data = arrayLength > 1 ? new List<JsonElement>(arrayLength - 1) : EmptyList;
                    foreach (var element in jsonData.RootElement.EnumerateArray())
                    {
                        if (!ioEvent.HasValue)
                        {
                            ioEvent = element;
                        }
                        else
                        {
                            data.Add(element);
                        }
                    }
                    
                    var message = new EventMessageEvent(ioEvent.ToString(), data);
                    _client.Events.EventMessageSubject.OnNext(message);
                }
                else
                {
                    _logger.LogWarning($"Unable to process ACK packet: {packet}. Data: {jsonData.RootElement}");
                    _client.Events.ErrorSubject.OnNext(new ErrorEvent($"Unable to process packet: {packet}"));
                }
            }
            catch (JsonException ex)
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