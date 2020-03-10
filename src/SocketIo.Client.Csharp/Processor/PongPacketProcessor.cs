using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Socket.Io.Csharp.Core.Extensions;
using Socket.Io.Csharp.Core.EventArguments;
using Socket.Io.Csharp.Core.Model;

namespace Socket.Io.Csharp.Core.Processor
{
    internal class PongPacketProcessor : IPacketProcessor
    {
        private readonly IEventEmitter _emitter;
        private readonly ILogger _logger;

        internal PongPacketProcessor(IEventEmitter emitter, ILogger logger)
        {
            _emitter = emitter;
            _logger = logger;
        }
        
        public ValueTask ProcessAsync(Packet packet)
        {
            _emitter.EmitAsync(SocketIoEvent.Pong, new PongEventArgs(packet));
            return default;
        }
    }
}