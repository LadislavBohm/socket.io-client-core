using System;
using System.Collections.Generic;
using System.Text;

namespace Socket.Io.Client.Core.Reactive.Model.Response
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

        public string Sid { get; }
        public IList<string> Upgrades { get; }
        public long PingInterval { get; }
        public long PingTimeout { get; }

        public override string ToString()
        {
            return $"{nameof(Sid)}: {Sid}, {nameof(PingInterval)}: {PingInterval}, {nameof(PingTimeout)}: {PingTimeout}";
        }
    }
}
