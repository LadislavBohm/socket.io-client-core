using System.Threading.Tasks;
using Socket.Io.Client.Core.Model.SocketIo;

namespace Socket.Io.Client.Core.Processing
{
    internal interface IPacketProcessor
    {
        void Process(Packet packet);
    }
}