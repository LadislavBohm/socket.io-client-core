using System.Threading.Tasks;
using Socket.Io.Client.Core.Model;

namespace Socket.Io.Client.Core.Processor
{
    internal interface IPacketProcessor
    {
        ValueTask ProcessAsync(Packet packet);
    }
}