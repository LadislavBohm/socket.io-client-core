using System;
using System.Collections.Generic;
using System.Text;

namespace Socket.Io.Client.Core.Extensions
{
    internal static class UriExtensions
    {
        internal static Uri ToSocketIoWebSocketUri(this Uri httpUri, string eio = "3", string path = "", IDictionary<string, string> queryParameters = null)
        {
            var builder = new StringBuilder();
            builder.Append(httpUri.Scheme == "https" || httpUri.Scheme == "wss" ? "wss://" : "ws://");
            builder.Append(httpUri.Host);

            if (!httpUri.IsDefaultPort)
            {
                builder.Append(":").Append(httpUri.Port);
            }

            builder
                .Append(string.IsNullOrWhiteSpace(path) ? "/socket.io" : path)
                .Append("/?EIO=")
                .Append(eio)
                .Append("&transport=websocket");

            if (queryParameters != null)
            {
                foreach (var item in queryParameters)
                {
                    builder
                        .Append("&")
                        .Append(item.Key)
                        .Append("=")
                        .Append(item.Value);
                }
            }
            return new Uri(builder.ToString());
        }
    }
}
