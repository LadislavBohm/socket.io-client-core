using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientWebSocket.Pipeline.Helpers;

namespace ClientWebSocket.Pipeline
{
    public partial class PipelineWebSocket : IDisposable
    {
        private SemaphoreSlim _singleWriter;
        private System.Net.WebSockets.ClientWebSocket _webSocket;
        private IDuplexPipe _transport;
        private IDuplexPipe _application;
        private Task _socketProcessing;
        private volatile bool _aborted;
        
        public PipelineWebSocket(): this(new PipelineWebSocketOptions()) {}
        
        public PipelineWebSocket(PipelineWebSocketOptions options)
        {
            Options = options;
        }
        
        public PipelineWebSocketOptions Options { get; }

        #region Public Methods

        public async Task StartAsync(Uri url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            try
            {
                var pipePair = DuplexPipe.CreateConnectionPair(Options.InputPipeOptions, Options.OutputPipeOptions);
                _transport = pipePair.Transport;
                _application = pipePair.Application;

                var connectTokenSource = new CancellationTokenSource(Options.ConnectTimeout);
                _singleWriter = new SemaphoreSlim(1);
                _webSocket = new System.Net.WebSockets.ClientWebSocket();
                await _webSocket.ConnectAsync(url, connectTokenSource.Token);
                RaiseOnConnected();

#pragma warning disable 4014
                StartReadingFromPipelineAsync(CancellationToken.None)
                    .ContinueWith(t => GC.KeepAlive(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
                _socketProcessing = StartSocketProcessingAsync();
#pragma warning restore 4014
            }
            catch (Exception ex)
            {
                Cleanup(ex);
                throw;
            }
        }

        public async Task StopAsync(CancellationToken token = default)
        {
            Exception exception = null;
            try
            {
                _transport.Output.Complete();
                _transport.Input.Complete();

                // Cancel any pending reads from the application, this should start the entire shutdown process
                _application.Input.CancelPendingRead();
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
                await _socketProcessing;
            }
            catch (Exception ex) { exception = ex; }
            finally
            {
                Cleanup(exception);
            }
        }

        public ValueTask SendAsync(string data, Encoding encoding = null) =>
            WriteAsync((encoding ?? Options.DefaultEncoding).GetBytes(data));

        public ValueTask SendAsync(ReadOnlyMemory<byte> message) => WriteAsync(message);
        
        public void Dispose()
        {
            _webSocket?.Dispose();
        }

        #endregion

        #region Pipeline Operations
        
        private async Task StartReadingFromPipelineAsync(CancellationToken token = default)
        {
            var reader = _transport.Input ?? throw new ObjectDisposedException(ToString());
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!reader.TryRead(out var readResult))
                        readResult = await reader.ReadAsync(token);
                    if (readResult.IsCanceled) break;

                    var buffer = readResult.Buffer;
                    while (Options.FrameSeparator.TryReadFrame(ref buffer, out ReadOnlySequence<byte> payload))
                    {
                        await RaiseOnMessageAsync(payload.Lease());
                    }
                    
                    reader.AdvanceTo(buffer.Start, buffer.End);
                    if (readResult.IsCompleted) break;
                }
                try { reader.Complete(); } catch { /* ignore */ }
            }
            catch (Exception ex)
            {
                RaiseOnError(ex);
                try { reader.Complete(ex); } catch { /* ignore */ }
            }
        }

        private ValueTask WriteAsync(ReadOnlyMemory<byte> payload)
        {
            async ValueTask AwaitFlushAndRelease(ValueTask<FlushResult> flush)
            {
                try { await flush; }
                finally { _singleWriter.Release(); }
            }
            
            // try to get the conch; if not, switch to async
            if (!_singleWriter.Wait(0)) return WriteAsyncSlowPath(payload);
            bool release = true;
            try
            {
                var writer = _transport?.Output ?? throw new ObjectDisposedException(ToString());
                var write = writer.WriteAsync(payload);
                if (write.IsCompletedSuccessfully) return default;
                release = false;
                return AwaitFlushAndRelease(write);
            }
            finally
            {
                if (release) _singleWriter.Release();
            }
        }
        
        private async ValueTask WriteAsyncSlowPath(ReadOnlyMemory<byte> payload)
        {
            await _singleWriter.WaitAsync();
            try
            {
                var writer = _transport?.Output ?? throw new ObjectDisposedException(ToString());
                await writer.WriteAsync(payload);
            }
            finally
            {
                _singleWriter.Release();
            }
        }
        
        #endregion

        #region ClientWebSocket Operations

        private async Task StartSocketProcessingAsync()
        {
            using (_webSocket)
            {
                var receiving = StartSocketReceiving(_webSocket);
                var sending = StartSocketSending(_webSocket);

                var trigger = await Task.WhenAny(receiving, sending);
                
                //we should get here once socket is being closed basically
                if (trigger == receiving)
                {
                    _application.Input.CancelPendingRead();

                    using var delayCts = new CancellationTokenSource();
                    var resultTask = await Task.WhenAny(sending, Task.Delay(TimeSpan.FromSeconds(1), delayCts.Token));

                    if (resultTask != sending)
                    {
                        _aborted = true;
                        _webSocket.Abort();
                    }
                    else
                    {
                        delayCts.Cancel();
                    }
                }
                else
                {
                    _aborted = true;
                    _webSocket.Abort();
                    _application.Output.CancelPendingFlush();
                }
            }
        }
        
        private async Task StartSocketReceiving(WebSocket socket)
        {
            try
            {
                while (true)
                {
                    ValueWebSocketReceiveResult receiveResult;
                    do
                    {
                        var memory = _application.Output.GetMemory(Options.ReceiveBufferSize);
                        receiveResult = await socket.ReceiveAsync(memory, CancellationToken.None);
                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            try
                            {
                                if (_webSocket.CloseStatus != WebSocketCloseStatus.NormalClosure)
                                {
                                    throw new InvalidOperationException($"Websocket closed with error: {_webSocket.CloseStatus}.");
                                }

                                return;
                            }
                            finally
                            {
                                RaiseOnClosed(_webSocket.CloseStatus, _webSocket.CloseStatusDescription);
                            }
                        }
                        
                        _application.Output.Advance(receiveResult.Count);
                    } while (!receiveResult.EndOfMessage);
                    
                    Options.FrameSeparator.WriteEndOfFrame(_application.Output);
                    var flushResult = await _application.Output.FlushAsync();
                    if (flushResult.IsCanceled || flushResult.IsCompleted) break;
                }
            }
            catch (Exception ex)
            {
                RaiseOnError(ex);
                if (!_aborted)
                {
                    _application.Output.Complete(ex);
                }
            }
            finally
            {
                _application.Output.Complete();
            }
        }

        private async Task StartSocketSending(WebSocket socket)
        {
            Exception error = null;

            try
            {
                while (true)
                {
                    var result = await _application.Input.ReadAsync();
                    var buffer = result.Buffer;

                    try
                    {
                        if (result.IsCanceled) break;
                        if (!buffer.IsEmpty)
                        {
                            try
                            {
                                if (WebSocketCanSend(socket))
                                    await socket.SendAsync(buffer, WebSocketMessageType.Text);
                                else
                                    break;
                            }
                            catch (Exception ex)
                            {
                                RaiseOnError(ex);
                                break;
                            }
                        }
                        else if (result.IsCompleted) break;
                    }
                    finally
                    {
                        _application.Input.AdvanceTo(buffer.End);
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseOnError(ex);
                error = ex;
            }
            finally
            {
                if (WebSocketCanSend(socket))
                {
                    await CloseWebSocketAsync(socket, error);
                }

                _application.Input.Complete();
            }
        }

        private static bool WebSocketCanSend(WebSocket ws)
        {
            return !(ws.State == WebSocketState.Aborted ||
                     ws.State == WebSocketState.Closed ||
                     ws.State == WebSocketState.CloseSent);
        }
        
        private async Task CloseWebSocketAsync(WebSocket socket, Exception error)
        {
            try
            {
                // We're done sending, send the close frame to the client if the websocket is still open
                await socket.CloseOutputAsync(
                    error != null
                        ? WebSocketCloseStatus.InternalServerError
                        : WebSocketCloseStatus.NormalClosure,
                    string.Empty,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                RaiseOnError(ex);
            }
        }

        #endregion

        private void Cleanup(Exception ex = null)
        {
            _application.Close(ex);
            _transport.Close(ex);
            _singleWriter?.Dispose();
            _webSocket.Abort();
        }
    }
}