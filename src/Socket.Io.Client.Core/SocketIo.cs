using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Socket.Io.Client.Core
{
    internal static class SocketIo
    {
        public const string DefaultNamespace = "/";

        internal static class Event
        {
            internal static IDictionary<SocketIoEvent, string> Name { get; } =
                Enum.GetValues(typeof(SocketIoEvent)).OfType<SocketIoEvent>().ToDictionary(e => e, e => e.ToString());
        }
    }
}