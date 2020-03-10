using System.Threading.Tasks;
using Socket.Io.Csharp.Core.Model;

namespace Socket.Io.Csharp.Core.Processor
{
    internal interface IPacketProcessor
    {
        ValueTask ProcessAsync(Packet packet);
    }
}