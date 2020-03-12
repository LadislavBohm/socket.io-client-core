using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Socket.Io.Client.Core.EventArguments;
using Socket.Io.Client.Core.Model;
using Socket.Io.Client.Core.Extensions;

namespace Socket.Io.Client.Core.Processor
{
    internal class ErrorPacketProcessor : IPacketProcessor
    {
        private readonly IEventEmitter _emitter;
        private readonly ILogger _logger;

        internal ErrorPacketProcessor(IEventEmitter emitter, ILogger logger)
        {
            _emitter = emitter;
            _logger = logger;
        }
        
        public ValueTask ProcessAsync(Packet packet)
        {
            _logger.LogError($"Received error packet. Data: {packet.Data}");
            return _emitter.EmitAsync(SocketIoEvent.Error, new ErrorEventArgs(packet.Data));
        }
    }
}