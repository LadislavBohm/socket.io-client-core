using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace Socket.Io.Client.Core.Model.SocketEvent
{
    public class DisconnectEvent
    {
        public DisconnectEvent(WebSocketCloseStatus? closeStatus, string closeStatusDescription)
        {
            CloseStatus = closeStatus;
            CloseStatusDescription = closeStatusDescription;
        }

        public DisconnectEvent(Exception exception)
        {
            Exception = exception;
        }

        public DisconnectEvent(WebSocketCloseStatus? closeStatus, string closeStatusDescription, Exception exception)
        {
            CloseStatus = closeStatus;
            CloseStatusDescription = closeStatusDescription;
            Exception = exception;
        }

        public WebSocketCloseStatus? CloseStatus { get; }

        public string CloseStatusDescription { get; }

        public Exception Exception { get; }
    }
}
