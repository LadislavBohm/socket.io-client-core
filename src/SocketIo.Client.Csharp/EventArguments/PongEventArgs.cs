using Socket.Io.Csharp.Core.Model;

namespace Socket.Io.Csharp.Core.EventArguments
{
    public class PongEventArgs : System.EventArgs
    {
        public PongEventArgs(Packet pong)
        {
            Pong = pong;
        }
       
        public Packet Pong { get; }
    }
}