using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Threading.Tasks;
using ClientWebSocket.Pipeline.EventArguments;
using ClientWebSocket.Pipeline.Helpers;

namespace ClientWebSocket.Pipeline
{
    public partial class PipelineWebSocket
    {
        public event EventHandler<System.EventArgs> OnConnected;
        public event EventHandler<SocketClosedEventArgs> OnClosed;
        public event AsyncEventHandler<IMemoryOwner<byte>> OnMessage;
        public event EventHandler<SocketErrorEventArgs> OnError; 
        
        private void RaiseOnConnected() => OnConnected?.Invoke(this, System.EventArgs.Empty);

        private void RaiseOnClosed(WebSocketCloseStatus? status, string closeDescription) => OnClosed?.Invoke(this, new SocketClosedEventArgs(status, closeDescription));

        private void RaiseOnError(Exception exception) => OnError?.Invoke(this, new SocketErrorEventArgs(exception));

        private ValueTask RaiseOnMessage(object sender, IMemoryOwner<byte> data) => OnMessage.InvokeAsync(sender, data);
    }
}