using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace Socket.Io.Client.Core.Model.SocketEvent
{
    public class AckMessageEvent : MessageEvent
    {
        public AckMessageEvent(int ack, IReadOnlyList<JsonElement> data) : base(data)
        {
            Ack = ack;
        }

        internal int Ack { get; }
    }
}
