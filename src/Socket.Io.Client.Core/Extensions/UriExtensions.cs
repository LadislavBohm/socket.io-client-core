using System;
using System.Collections.Generic;
using System.Text;

namespace Socket.Io.Client.Core.Extensions
{
    internal static class UriExtensions
    {
        internal static Uri ToSocketIoWebSocketUri(this Uri httpUri, string eio = "3", string path = "")
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

            if (!string.IsNullOrEmpty(httpUri.Query))
            {
                builder.Append("&");
                builder.Append(httpUri.Query.TrimStart('?'));
            }
            
            return new Uri(builder.ToString());
        }
    }
}
