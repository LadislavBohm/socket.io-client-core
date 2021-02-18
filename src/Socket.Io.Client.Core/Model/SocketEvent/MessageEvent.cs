using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Socket.Io.Client.Core.Model.SocketEvent
{
    public abstract class MessageEvent
    {
        internal MessageEvent(IReadOnlyList<JsonElement> data)
        {
            Data = data;
        }

        public IReadOnlyList<JsonElement> Data { get; }
    }
}
