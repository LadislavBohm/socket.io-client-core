using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Socket.Io.Client.Core.Model.SocketEvent;
using Socket.Io.Client.Core.Model.SocketIo;

namespace Socket.Io.Client.Core.Processing
{
    internal class ErrorPacketProcessor : IPacketProcessor
    {
        private readonly ISocketIoClient _socket;
        private readonly ILogger _logger;

        internal ErrorPacketProcessor(ISocketIoClient socket, ILogger logger)
        {
            _socket = socket;
            _logger = logger;
        }
        
        public void Process(Packet packet)
        {
            _logger.LogError($"Received error packet. Data: {packet.Data}");
            _socket.Events.ErrorSubject.OnNext(new ErrorEvent(packet.Data));
        }
    }
}