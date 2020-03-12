using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ClientWebSocket.Pipeline;
using ClientWebSocket.Pipeline.EventArguments;
using EnumsNET;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Socket.Io.Client.Core.EventArguments;
using Socket.Io.Client.Core.Model;
using Socket.Io.Client.Core.Parse;
using Socket.Io.Client.Core.Processor;
using Socket.Io.Client.Core.Extensions;

namespace Socket.Io.Client.Core
{
    public partial class SocketIoClient : ISocketIoClient, IDisposable
    {
        private readonly ILogger<SocketIoClient> _logger;
        private readonly PipelineWebSocket _socket;
        private readonly IDictionary<PacketType, IPacketProcessor> _packetProcessors;
        private readonly ConcurrentDictionary<string, List<object>> _events;
        private readonly ConcurrentDictionary<int, object> _callbacks;
        private readonly Channel<Packet> _packetChannel;

        private int _packetId = -1;
        private string _namespace = "/";
        private TimeSpan _pingInterval = TimeSpan.FromSeconds(5);
        private CancellationTokenSource _cts;

        public SocketIoClient(SocketIoOptions options = null, ILogger<SocketIoClient> logger = null)
        {
            State = ReadyState.Closed;
            Options = options ?? new SocketIoOptions();
            _logger = logger ?? NullLogger<SocketIoClient>.Instance;
            _packetProcessors = new Dictionary<PacketType, IPacketProcessor>
            {
                { PacketType.Open, new OpenPacketProcessor(this, Options.JsonSerializerOptions, _logger) },
                { PacketType.Pong, new PongPacketProcessor(this, _logger) },
                { PacketType.Error, new ErrorPacketProcessor(this, _logger) },
                { PacketType.Message, new MessagePacketProcessor(this, _logger) },
            };
            _callbacks = new ConcurrentDictionary<int, object>();
            _events = new ConcurrentDictionary<string, List<object>>();
            InitializeSocketIoEvents(_events);
            _cts = new CancellationTokenSource();
            _socket = new PipelineWebSocket();
            switch (Options.PacketChannelOptions)
            {
                case UnboundedChannelOptions unboundedOptions:
                    _packetChannel = Channel.CreateUnbounded<Packet>(unboundedOptions);
                    break;
                case BoundedChannelOptions boundedOptions:
                    _packetChannel = Channel.CreateBounded<Packet>(boundedOptions);
                    break;
                default:
                    throw new NotSupportedException($"Channel options of type: {Options.PacketChannelOptions.GetType()} are not supported.");
            }
            _socket.OnMessage += OnSocketMessageAsync;
        }

        private bool HasDefaultNamespace => string.IsNullOrEmpty(_namespace) || _namespace == SocketIo.DefaultNamespace;

        private SocketIoOptions Options { get; }

        public ReadyState State { get; private set; }

        #region ISocketIoClient Implementation

        public async Task OpenAsync(Uri uri)
        {
            _logger.LogInformation($"Opening socket connection to: {uri}");
            
            _cts = new CancellationTokenSource();
            SubscribeToEvents();
            State = ReadyState.Opening;

            _namespace = uri.LocalPath;
            var socketIoUri = uri.HttpToSocketIoWs(path: !HasDefaultNamespace ? _namespace : null);
            _ = StartPacketProcessingAsync();
            await _socket.StartAsync(socketIoUri);

            _logger.LogInformation("Socket connection successfully established");
        }

        public async Task CloseAsync()
        {
            if (State == ReadyState.Closing || State == ReadyState.Closed)
                throw new InvalidOperationException($"Socket is in state: {State} and cannot be closed.");

            _logger.LogInformation("Closing socket connection.");
            try
            {
                State = ReadyState.Closing;
                await SendAsync(PacketType.Message, PacketSubType.Disconnect, null);
                await this.EmitAsync(SocketIoEvent.Close);
                await _socket.StopAsync();
            }
            finally
            {
                await CleanupAsync();
                State = ReadyState.Closed;
                _logger.LogInformation("Socket connection closed.");
            }
        }

        ValueTask ISocketIoClient.SendAsync(Packet packet)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"Sending packet: {packet}");

            var data = PacketParser.Encode(packet, Options.Encoding);
            return _socket.SendAsync(data);
        }

        #endregion

        private async ValueTask OnSocketMessageAsync(IMemoryOwner<byte> data)
        {
            try
            {
                if (!PacketParser.TryDecode(data.Memory, Options.Encoding, out var packet))
                {
                    _logger.LogError($"Could not decode packet from data: {Options.Encoding.GetString(data.Memory.Span)}");
                }
                
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"Processing packet: {packet}");

                if (await _packetChannel.Writer.WaitToWriteAsync(_cts.Token))
                {
                    var writeResult = _packetChannel.Writer.TryWrite(packet);
                    if (!writeResult)
                        _logger.LogWarning($"Failed to write packet: {packet} to channel.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing socket data");
                await this.EmitAsync(SocketIoEvent.Error, new ErrorEventArgs(ex));
            }
        }

        private Task StartPacketProcessingAsync()
        {
            var processingTasks = Enumerable.Repeat(Task.Run(async () =>
            {
                while (await _packetChannel.Reader.WaitToReadAsync(_cts.Token))
                {
                    while (_packetChannel.Reader.TryRead(out var packet))
                    {
                        try
                        {
                            if (!_packetProcessors.TryGetValue(packet.Type, out var processor))
                            {
                                _logger.LogWarning($"Unsupported packet type: {packet.Type}. Data: {packet}");
                            }
                            else
                            {
                                await processor.ProcessAsync(packet);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error while processing socket data");
                            await this.EmitAsync(SocketIoEvent.Error, new ErrorEventArgs(ex));
                        }
                    }
                }
            }, _cts.Token), Options.PacketProcessingThreadCount);

            return Task.WhenAll(processingTasks);
        }

        private void StartPingPong()
        {
            Task.Run(async () =>
            {
                while (State == ReadyState.Open && !_cts.IsCancellationRequested)
                {
                    try
                    {
                        await SendAndValidatePingAsync();
                        _cts.Token.ThrowIfCancellationRequested();
                    }
                    catch (OperationCanceledException) { }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error while performing ping/pong.");
                        await this.EmitAsync(SocketIoEvent.Error, new ErrorEventArgs(ex));
                    }
                    finally
                    {
                        await Task.Delay(_pingInterval, _cts.Token);
                    }
                }
            });
        }

        private async ValueTask SendAndValidatePingAsync()
        {
            var data = string.Empty;

            //ping packet does not have subtype
            await SendAsync(PacketType.Ping, null, data);
            OnOnce(SocketIoEvent.Pong, (Func<PongEventArgs, ValueTask>)OnPong);

            ValueTask OnPong(PongEventArgs args)
            {
                if (data != args.Pong.Data)
                {
                    _logger.LogError($"Server responded with incorrect pong packet. Ping data: [{data}] Pong packet: [{args.Pong}]");
                    return this.EmitAsync(SocketIoEvent.ProbeError, new ErrorEventArgs("Server responded with incorrect pong."));
                }

                return this.EmitAsync(SocketIoEvent.ProbeSuccess);
            }
        }

        private ValueTask CleanupAsync()
        {
            _cts.Cancel();
            _callbacks.Clear();
            _events.Clear();
            return default;
        }

        #region Helpers

        private void SubscribeToEvents()
        {
            On(SocketIoEvent.Connect, OnConnect);
            On(SocketIoEvent.Handshake, (Func<HandshakeData, ValueTask>)OnHandshake);
            On(SocketIoEvent.Open, OnOpen);
        }

        private int GetNextPacketId() => Interlocked.Increment(ref _packetId);

        private void InitializeSocketIoEvents(IDictionary<string, List<object>> events)
        {
            foreach (var ioEvent in Enums.GetValues<SocketIoEvent>())
                events[SocketIo.Event.Name[ioEvent]] = new List<object>();
        }

        private ValueTask SendAsync(Packet packet) => ((ISocketIoClient)this).SendAsync(packet);

        private ValueTask SendAsync(PacketType type, PacketSubType? subType, string data, int? id = null)
        {
            return SendAsync(new Packet(type, subType, type == PacketType.Message ? _namespace : null, data, id, 0, null));
        }

        private ValueTask SendEmitAsync<TData>(string eventName, TData data = default, int? packetId = null)
        {
            var sb = new StringBuilder()
                     .Append("[\"")
                     .Append(eventName)
                     .Append("\"");

            if (data != null)
            {
                sb.Append(",");
                sb.Append(JsonSerializer.Serialize(data, Options.JsonSerializerOptions));
            }

            sb.Append("]");

            return SendAsync(PacketType.Message, PacketSubType.Event, sb.ToString(), packetId);
        }

        private ValueTask OnHandshake(HandshakeData arg)
        {
            _pingInterval = TimeSpan.FromMilliseconds(arg.PingInterval);
            return default;
        }

        private ValueTask OnConnect()
        {
            State = ReadyState.Open;
            StartPingPong();
            return default;
        }

        private async ValueTask OnOpen()
        {
            if (!HasDefaultNamespace)
            {
                _logger.LogDebug($"Sending connect to namespace: {_namespace}");
                await SendAsync(PacketType.Message, PacketSubType.Connect, null);
            }
        }

        #endregion

        public void Dispose()
        {
            _socket?.Dispose();
            _cts?.Dispose();
            _packetChannel.Writer.TryComplete();
        }
    }
}