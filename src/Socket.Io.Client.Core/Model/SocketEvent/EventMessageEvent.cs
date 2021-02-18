using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace Socket.Io.Client.Core.Model.SocketEvent
{
    public class EventMessageEvent : MessageEvent
    {
        private readonly Action<object> _callback;
        
        public EventMessageEvent(string eventName, bool supportsAcknowledgement, Action<object> callback, IReadOnlyList<JsonElement> data) : base(data)
        {
            _callback = callback;
            EventName = eventName;
            SupportsAcknowledgement = supportsAcknowledgement;
        }

        public string EventName { get; }
        public bool SupportsAcknowledgement { get; }

        /// <summary>
        /// Invokes callback on server to let it know this message has been received/processed.
        /// Always check <see cref="SupportsAcknowledgement"/> property before calling!
        /// </summary>
        /// <param name="data">Data to be passed into server callback</param>
        public void Acknowledge<T>(T data) => _callback(data);

        /// <summary>
        /// Invokes callback on server to let it know this message has been received/processed.
        /// Always check <see cref="SupportsAcknowledgement"/> property before calling!
        /// </summary>
        public void Acknowledge() => _callback(default);
    }
}
