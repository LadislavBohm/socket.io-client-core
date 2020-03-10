using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnumsNET;

namespace Socket.Io.Csharp.Core
{
    internal static class SocketIo
    {
        internal static class Event
        {
            internal static IDictionary<SocketIoEvent, string> Name { get; } =
                Enums.GetMembers<SocketIoEvent>().ToDictionary(x => x.Value, x => ToUnderscore(x.Name));


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