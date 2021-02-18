using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Socket.Io.Client.Core
{
    internal static class SocketIo
    {
        public const string DefaultNamespace = "/";
        public const string DefaultPath = "/socket.io";
        public const int DefaultPingTimeout = 5000;
        public const int DefaultPingInterval = 25000;
    }
}