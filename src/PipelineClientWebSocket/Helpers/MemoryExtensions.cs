using System;
using System.Text;

namespace ClientWebSocket.Pipeline.Helpers
{
    public static class MemoryExtensions
    {
        public static string Decode(this ReadOnlyMemory<byte> bytes, Encoding encoding = null)
        {
            return (encoding ?? Encoding.UTF8).GetString(bytes.Span);
        }
        
        public static string Decode(this Memory<byte> bytes, Encoding encoding = null)
        {
            return (encoding ?? Encoding.UTF8).GetString(bytes.Span);
        }
    }
}