using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Socket.Io.Client.Core.EventArguments;
using Socket.Io.Client.Core.Model;
using Socket.Io.Client.Core.Extensions;

namespace Socket.Io.Client.Core.Processor
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
            return _emitter.EmitAsync(SocketIoEvent.Pong, new PongEventArgs(packet));
        }
    }
}