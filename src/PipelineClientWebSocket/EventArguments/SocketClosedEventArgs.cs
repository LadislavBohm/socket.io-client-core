using System.Net.WebSockets;

namespace ClientWebSocket.Pipeline.EventArguments
{
    public class SocketClosedEventArgs : System.EventArgs
    {
        public SocketClosedEventArgs(WebSocketCloseStatus? status, string closeDescription)
        {
            Status = status;
            CloseDescription = closeDescription;
        }

        public WebSocketCloseStatus? Status { get; }
        public string CloseDescription { get; }
    }
}