using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Socket.Io.Client.Core.Model.SocketEvent
{
    public abstract class MessageEvent
    {
        internal MessageEvent(IReadOnlyList<string> data)
        {
            Data = data;
        }

        public IReadOnlyList<string> Data { get; }

        [IgnoreDataMember]
        public string FirstData => Data?.FirstOrDefault();
    }
}
