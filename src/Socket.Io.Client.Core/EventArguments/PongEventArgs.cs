using Socket.Io.Client.Core.Model;

namespace Socket.Io.Client.Core.EventArguments
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