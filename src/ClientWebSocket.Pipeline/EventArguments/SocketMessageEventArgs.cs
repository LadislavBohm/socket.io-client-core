using System.Buffers;

namespace ClientWebSocket.Pipeline.EventArguments
{
    public class SocketMessageEventArgs : System.EventArgs
    {
        public SocketMessageEventArgs(IMemoryOwner<byte> data)
        {
            Data = data;
        }

        public IMemoryOwner<byte> Data { get; }
    }
}