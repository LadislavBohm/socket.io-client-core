using System.Threading.Tasks;
using Socket.Io.Client.Core.Reactive.Model.SocketIo;

namespace Socket.Io.Client.Core.Reactive.Processing
{
    internal interface IPacketProcessor
    {
        void Process(Packet packet);
    }
}