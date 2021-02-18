using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Socket.Io.Client.Core.Model.Response
{
    public class HandshakeResponse
    {
        public HandshakeResponse(string sid, IList<string> upgrades, long pingInterval, long pingTimeout)
        {
            Sid = sid;
            Upgrades = upgrades;
            PingInterval = pingInterval;
            PingTimeout = pingTimeout;
        }

        [JsonPropertyName("sid")]
        public string Sid { get; }
        [JsonPropertyName("upgrades")]
        public IList<string> Upgrades { get; }
        [JsonPropertyName("pingInterval")]
        public long PingInterval { get; }
        [JsonPropertyName("pingTimeout")]
        public long PingTimeout { get; }

        public override string ToString()
        {
            return $"{nameof(Sid)}: {Sid}, {nameof(PingInterval)}: {PingInterval}, {nameof(PingTimeout)}: {PingTimeout}";
        }
    }
}
