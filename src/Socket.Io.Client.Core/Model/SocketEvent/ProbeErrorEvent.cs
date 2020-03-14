using System;
using System.Collections.Generic;
using System.Text;
using Socket.Io.Client.Core.Model.SocketIo;

namespace Socket.Io.Client.Core.Model.SocketEvent
{
    public class ProbeErrorEvent
    {
        public ProbeErrorEvent(string pingData, string pongData)
        {
            PingData = pingData;
            PongData = pongData;
        }

        public string PingData { get; }

        public string PongData { get; }
    }
}
