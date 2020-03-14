using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Socket.Io.Client.Core.Reactive
{
    internal static class SocketIo
    {
        public const string DefaultNamespace = "/";

        internal static class Event
        {
            internal static IDictionary<SocketIoEvent, string> Name { get; } =
                Enum.GetValues(typeof(SocketIoEvent)).OfType<SocketIoEvent>().ToDictionary(e => e, e => e.ToString());


            private static string ToUnderscore(string enumName)
            {
                if (enumName.Length < 2) return enumName.ToLowerInvariant();

                var sb = new StringBuilder(enumName.Length + 3);
                sb.Append(char.ToLowerInvariant(enumName[0]));
                for (int i = 1; i < enumName.Length; i++)
                {
                    if (char.IsUpper(enumName[i]))
                    {
                        sb.Append("_");
                        sb.Append(char.ToLowerInvariant(enumName[i]));
                    }
                    else
                    {
                        sb.Append(enumName[i]);
                    }
                }

                return sb.ToString();
            }
        }
    }
}