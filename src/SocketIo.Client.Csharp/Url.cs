using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Socket.Io.Csharp.Core
{
    public static class Url
    {
        private static readonly string PatternHttp = "^http|ws$";
        private static readonly string PatternHttps = "^(http|ws)s$";

        public static Uri Parse(Uri uri)
        {
            var protocol = uri.Scheme;
            if (!Regex.IsMatch(uri.Scheme, "^https?|wss?$"))
            {
                protocol = "https";
            }

            int port = uri.Port;
            if (port == -1)
            {
                if (Regex.IsMatch(protocol, PatternHttp))
                {
                    port = 80;
                }
                else if (Regex.IsMatch(protocol, PatternHttps))
                {
                    port = 443;
                }
            }

            String path = uri.AbsolutePath;
            if (string.IsNullOrEmpty(path))
            {
                path = "/";
            }

            var userInfo = uri.UserInfo;
            var query = uri.Query;
            var fragment = uri.Fragment;
            var builder = new StringBuilder()
                          .Append(protocol)
                          .Append("://")
                          .Append(userInfo + "@")
                          .Append(uri.Host)
                          .Append(port != -1 ? ":" + port : "")
                          .Append(path)
                          .Append("?" + query)
                          .Append("#" + fragment);

            return new Uri(builder.ToString());
        }

        public static String ExtractId(String url) => ExtractId(new Uri(url));

        public static String ExtractId(Uri url)
        {
            String protocol = url.Scheme;
            int port = url.Port;
            if (port == -1)
            {
                if (Regex.IsMatch(protocol, PatternHttp))
                    port = 80;
                else if (Regex.IsMatch(protocol, PatternHttps)) port = 443;
            }

            return protocol + "://" + url.Host + ":" + port;
        }
    }
}