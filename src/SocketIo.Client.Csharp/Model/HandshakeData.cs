using System.Collections;
using System.Collections.Generic;

namespace Socket.Io.Csharp.Core.Model
{
    public class HandshakeData
    {
        public HandshakeData() {}
        
        public HandshakeData(string sid, IList<string> upgrades, long pingInterval, long pingTimeout)
        {
            Sid = sid;
            Upgrades = upgrades;
            PingInterval = pingInterval;
            PingTimeout = pingTimeout;
        }
        
        public string Sid { get; set; }
        public IList<string> Upgrades { get; set; }
        public long PingInterval { get; set; }
        public long PingTimeout { get; set; }

        public override string ToString()
        {
            return $"{nameof(Sid)}: {Sid}, {nameof(PingInterval)}: {PingInterval}, {nameof(PingTimeout)}: {PingTimeout}";
        }
    }
}