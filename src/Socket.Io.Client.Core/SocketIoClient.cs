using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Socket.Io.Client.Core.Json;
using Socket.Io.Client.Core.Model;
using Socket.Io.Client.Core.Model.Response;
using Socket.Io.Client.Core.Model.SocketEvent;
using Socket.Io.Client.Core.Model.SocketIo;
using Socket.Io.Client.Core.Parse;
using Socket.Io.Client.Core.Processing;
using Socket.Io.Client.Core.Extensions;
using Websocket.Client;

namespace Socket.Io.Client.Core
{
    public partial class SocketIoClient : ISocketIoClient
    {
        private IWebsocketClient _socket;

        private IDisposable _disconnectSubscription;
        private IDisposable _messageSubscription;
        private IDisposable _pingPongSubscription;

        private int _packetId = -1;
        private string _namespace;
        private string _query;
        private HandshakeResponse _currentHandshake;

        private bool _disposed = false;

        private readonly IDictionary<EngineIoType, IPacketProcessor> _packetProcessors;
        private readonly ConcurrentQueue<Packet> _sentPingPackets = new ConcurrentQueue<Packet>();

        public SocketIoClient(SocketIoClientOptions options = null)
        {
            Options = options ?? new SocketIoClientOptions();
            Events = new SocketIoEvents();
            _packetProcessors = new Dictionary<EngineIoType, IPacketProcessor>
            {
                { EngineIoType.Open, new OpenPacketProcessor(this, Logger) },
                { EngineIoType.Pong, new PongPacketProcessor(this, Logger) },
                { EngineIoType.Error, new ErrorPacketProcessor(this, Logger) },
                { EngineIoType.Message, new MessagePacketProcessor(this, Logger) }
            };
        }

        #region Private Properties

        private bool HasDefaultNamespace => string.IsNullOrEmpty(_namespace) || _namespace == SocketIo.DefaultNamespace;
        private ILogger<SocketIoClient> Logger => Options.Logger;
        private JsonSerializerOptions JsonOptions => Options.JsonSerializerOptions;
        private Encoding Encoding => Options.Encoding;

        #endregion

        public SocketIoEvents Events { get; private set; }
        public SocketIoClientOptions Options { get; }
        public bool IsRunning => _socket?.IsRunning ?? false;
        public ReadyState State { get; private set; } = ReadyState.Closed;

        #region Public Methods

        public async Task OpenAsync(Uri uri, SocketIoOpenOptions options = null)
        {
            ThrowIfStarted();

            try
            {
                Logger.LogInformation($"Opening socket connection to: {uri}");

                State = ReadyState.Opening;
                _namespace = uri.LocalPath;
                _query = string.IsNullOrEmpty(uri.Query) ? null : uri.Query.TrimStart('?');
                var socketIoUri = uri.ToSocketIoWebSocketUri(path: options?.Path);
                _socket = new WebsocketClient(socketIoUri) { MessageEncoding = Encoding, IsReconnectionEnabled = false };
                
                Events.HandshakeSubject.Subscribe(OnHandshake);
                Events.OnPong.Subscribe(OnPong);
                Events.OnOpen.Subscribe(OnOpen);

                await StartSocketAsync();

                State = ReadyState.Open;
                Logger.LogInformation("Socket connection successfully established.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while starting socket.");
                State = ReadyState.Closed;
                throw;
            }
        }

        public async Task CloseAsync()
        {
            ThrowIfNotStarted();

            Logger.LogInformation("Closing socket connection.");

            try
            {
                State = ReadyState.Closing;

                //don't receive anything from underlying socket anymore
                _pingPongSubscription.Dispose();
                _messageSubscription.Dispose();
                _disconnectSubscription.Dispose();
                
                //clean anything set during socket startup/run time
                _packetId = -1;
                _currentHandshake = null;
                _sentPingPackets.Clear();

                //finally stop the socket
                await _socket.StopOrFail(WebSocketCloseStatus.NormalClosure, string.Empty);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while stopping socket.");
                throw;
            }
            finally
            {
                Logger.LogInformation("Socket connection closed.");
                State = ReadyState.Closed;
            }
        }

        public IObservable<AckMessageEvent> Emit(string eventName) => Emit<object>(eventName, null);

        public IObservable<AckMessageEvent> Emit<TData>(string eventName, TData data)
        {
            ThrowIfInvalidEvent(eventName);
            ThrowIfNotRunning();

            Logger.LogDebug($"Emitting event '{eventName}'.");
            var sb = new StringBuilder()
                     .Append("[\"")
                     .Append(eventName)
                     .Append("\"");

            if (data != null)
            {
                sb.Append(",");
                sb.Append(JsonSerializer.Serialize(data, JsonOptions));
            }

            sb.Append("]");

            var packet = CreatePacket(EngineIoType.Message, SocketIoType.Event, sb.ToString(), GetNextPacketId());
            var result = Events.AckMessageSubject.Where(m => m.Ack == packet.Id).Take(1);
            
            Send(packet);
            return result;
        }

        public IObservable<EventMessageEvent> On(string eventName)
        {
            ThrowIfInvalidEvent(eventName);
            return Events.EventMessageSubject.Where(m => m.EventName == eventName);
        }

        #endregion

        private async Task StartSocketAsync()
        {
            var disconnects = new List<DisconnectEvent>();
            var messages = new List<ResponseMessage>();

            //start temporary collecting messages and disconnects to local collection
            using IDisposable disconnectSubscription = _socket.DisconnectionHappened
                .Select(info => new DisconnectEvent(info.CloseStatus, info.CloseStatusDescription, info.Exception)).Subscribe(disconnects.Add);
            using IDisposable messageSubscription = _socket.MessageReceived.Subscribe(messages.Add);

            //wait until start finishes
            await _socket.StartOrFail();

            //start real subscriptions and prepend any temporary collected data to them
            _disconnectSubscription = _socket.DisconnectionHappened
                .Select(info => new DisconnectEvent(info.CloseStatus, info.CloseStatusDescription, info.Exception))
                .StartWith(disconnects)
                .Subscribe(Events.DisconnectSubject);
            _messageSubscription = _socket.MessageReceived.StartWith(messages).Subscribe(OnMessage);
        }

        private void OnMessage(ResponseMessage message)
        {
            try
            {
                if (!PacketParser.TryDecode(message.Text, out var packet))
                {
                    Logger.LogError($"Could not decode packet from data: {message}");
                }

                if (Logger.IsEnabled(LogLevel.Trace))
                    Logger.LogTrace($"Processing packet: {packet}");

                Events.PacketSubject.OnNext(packet);
                if (_packetProcessors.TryGetValue(packet.EngineIoType, out var processor))
                {
                    processor.Process(packet);
                }
                else
                {
                    Logger.LogWarning($"Unsupported packet type: {packet.EngineIoType}. Data: {packet}");
                    //give any notice to user?
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error while processing socket data. Data: {message}");
                Events.ErrorSubject.OnNext(new ErrorEvent(ex, $"Error while processing socket data. Data: {message}"));
            }
        }

        private void OnHandshake(HandshakeResponse data)
        {
            _currentHandshake = data;
            try
            {
                _pingPongSubscription?.Dispose();
            }
            finally
            {
                var pingInterval = _currentHandshake.PingInterval == 0
                    ? SocketIo.DefaultPingInterval
                    : _currentHandshake.PingInterval;

                _pingPongSubscription = Observable
                    .Interval(TimeSpan.FromMilliseconds(pingInterval))
                    .Subscribe(i =>
                    {
                        Logger.LogDebug("Sending PING packet");
                        var ping = CreatePacket(EngineIoType.Ping, null, null, null);
                        Send(ping);
                        _sentPingPackets.Enqueue(ping);
                    });
            }
        }

        private void OnOpen(Unit obj)
        {
            if (!HasDefaultNamespace)
            {
                Logger.LogDebug($"Sending connect to namespace: {_namespace}");

                //we need to send query string with this packet
                var query = string.IsNullOrEmpty(_query) ? null : _query;
                var packet = new Packet(EngineIoType.Message, SocketIoType.Connect, _namespace, null, null, 0, query);
                Send(packet);
            }
        }

        private void OnPong(PongResponse pong)
        {
            if (_sentPingPackets.TryDequeue(out var ping))
            {
                //if data is not empty then it was 'probe' packet so we should check if server responded with same data
                if (!string.IsNullOrEmpty(ping.Data))
                {
                    if (ping.Data != pong.Data)
                    {
                        Logger.LogWarning($"Invalid PONG packet received on probe packet. PING: {ping.Data} PONG: {pong.Data}");
                        Events.ProbeErrorSubject.OnNext(new ProbeErrorEvent(ping.Data, pong.Data));
                    }
                }
            }
            else
            {
                Logger.LogWarning($"No corresponding PING packet found for PONG: {pong}");
            }
        }
        
        private void Send(Packet packet)
        { 
            ThrowIfNotRunning();
            _socket.Send(PacketParser.Encode(packet));
        }

        private Packet CreatePacket(EngineIoType engineType, SocketIoType? socketType, string data, int? id)
        {
            return new Packet(engineType, socketType, engineType == EngineIoType.Message ? _namespace : null, data, id,
                0, null);
        }

        private int GetNextPacketId() => Interlocked.Increment(ref _packetId);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            Logger.LogInformation("Disposing socket.");
            if (disposing)
            {
                //first dispose ping/pong and message subscriptions because we don't want to receive those now
                _pingPongSubscription?.Dispose();
                _messageSubscription?.Dispose();

                //dispose socket so it fires disconnect event
                _socket?.Dispose();

                //now dispose disconnect once socket it disposed
                _disconnectSubscription?.Dispose();

                //dispose all events because user should not keep subscriptions or create new ones
                Events?.Dispose();
            }

            _disposed = true;
        }
    }
}
