using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace Socket.Io.Client.Core.Model.SocketEvent
{
    public class DisconnectEvent
    {
        public DisconnectEvent(string reason)
        {
            Reason = reason;
        }

        public DisconnectEvent(Exception exception)
        {
            Exception = exception;
        }

        public DisconnectEvent(WebSocketCloseStatus? closeStatus, string reason)
        {
            CloseStatus = closeStatus;
            Reason = reason;
        }

        public DisconnectEvent(WebSocketCloseStatus? closeStatus, string reason, Exception exception)
        {
            CloseStatus = closeStatus;
            Reason = reason;
            Exception = exception;
        }

        public WebSocketCloseStatus? CloseStatus { get; }

        public string Reason { get; }

        public Exception Exception { get; }
    }
}
