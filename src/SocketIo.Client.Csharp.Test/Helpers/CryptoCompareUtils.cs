using System;
using System.Collections.Generic;

namespace CryptoCompare.Streamer.Utils
{
    internal static class CryptoCompareUtils
    {
        private static string ConvertToMegaBytes(long bytes) => $"{Math.Round((double)bytes / 1024 / 1024, 2)} MB";

        public static DateTime ConvertToDateTime(long timestamp)
        {
            var date = new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            return date.AddMilliseconds(timestamp * 1000);
        }

        public static Uri CreateWssUri(Uri socketIoUri, string path = null, IDictionary<string, string> parameters = null)
        {
            return socketIoUri.HttpToSocketIoWs("3", path, parameters);
        }
    }
}