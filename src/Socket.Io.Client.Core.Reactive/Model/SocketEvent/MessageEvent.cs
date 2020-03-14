using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Socket.Io.Client.Core.Reactive.Model.SocketEvent
{
    public class MessageEvent
    {
        public MessageEvent(int? ack, IReadOnlyList<string> data)
        {
            Ack = ack;
            Data = data;
        }

        public IReadOnlyList<string> Data { get; }

        [IgnoreDataMember]
        public string FirstData => Data?.FirstOrDefault();
        
        [IgnoreDataMember]
        internal int? Ack { get; }

        [IgnoreDataMember]
        public static MessageEvent Empty { get; } = new MessageEvent(null, new List<string>());
    }
}
