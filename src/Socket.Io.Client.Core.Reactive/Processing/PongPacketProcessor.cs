using Microsoft.Extensions.Logging;
using Socket.Io.Client.Core.Reactive.Model.Response;
using Socket.Io.Client.Core.Reactive.Model.SocketIo;

namespace Socket.Io.Client.Core.Reactive.Processing
{
    internal class PongPacketProcessor : IPacketProcessor
    {
        private readonly ISocketIoClient _client;
        private readonly ILogger _logger;

        internal PongPacketProcessor(ISocketIoClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }
        
        public void Process(Packet packet)
        {
            _logger.LogDebug(string.IsNullOrEmpty(packet.Data)
                ? "Received PONG packet"
                : $"Received PONG packet with data: {packet.Data}");
            _client.Events.PongSubject.OnNext(new PongResponse(packet.Data));
        }
    }
}