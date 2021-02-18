using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace Socket.Io.Client.Core.Model.SocketEvent
{
    public class EventMessageEvent : MessageEvent
    {
        public EventMessageEvent(string eventName, IReadOnlyList<JsonElement> data) : base(data)
        {
            EventName = eventName;
        }

        [IgnoreDataMember]
        public string EventName { get; }
    }
}
