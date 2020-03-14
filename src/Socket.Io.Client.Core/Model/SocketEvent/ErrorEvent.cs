using System;
using System.Collections.Generic;
using System.Text;

namespace Socket.Io.Client.Core.Model.SocketEvent
{
    public class ErrorEvent
    {
        public ErrorEvent(string description)
        {
            Description = description;
        }

        public ErrorEvent(Exception ex)
        {
            Exception = ex ?? throw new ArgumentNullException(nameof(ex));
            Description = ex.Message;
        }

        public ErrorEvent(Exception exception, string description)
        {
            Exception = exception;
            Description = description;
        }

        public Exception Exception { get; }

        public string Description { get; }
    }
}
