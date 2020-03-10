using System;

namespace ClientWebSocket.Pipeline.EventArguments
{
    public class SocketErrorEventArgs : System.EventArgs
    {
        public SocketErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}