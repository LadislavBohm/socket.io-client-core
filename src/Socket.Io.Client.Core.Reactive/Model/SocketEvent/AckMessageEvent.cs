using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Socket.Io.Client.Core.Reactive.Model.SocketEvent
{
    public class AckMessageEvent : MessageEvent
    {
        public AckMessageEvent(int ack, IReadOnlyList<string> data) : base(data)
        {
            Ack = ack;
        }

        [IgnoreDataMember]
        internal int Ack { get; }
    }
}
