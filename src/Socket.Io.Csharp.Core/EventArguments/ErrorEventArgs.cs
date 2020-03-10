using System;

namespace Socket.Io.Csharp.Core.EventArguments
{
    public class ErrorEventArgs : System.EventArgs
    {
        public ErrorEventArgs(string description)
        {
            Description = description;
        }

        public ErrorEventArgs(Exception ex)
        {
            Exception = ex ?? throw new ArgumentNullException(nameof(ex));
            Description = ex.Message;
        }
        
        public Exception Exception { get; }

        public string Description { get; }
    }
}