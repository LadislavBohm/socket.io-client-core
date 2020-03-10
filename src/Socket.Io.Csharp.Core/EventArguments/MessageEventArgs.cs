using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Socket.Io.Csharp.Core.EventArguments
{
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(IReadOnlyList<string> data)
        {
            Data = data;
        }

        public IReadOnlyList<string> Data { get; }

        public new static MessageEventArgs Empty { get; } = new MessageEventArgs(new List<string>());
    }
}
