using System.Buffers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ClientWebSocket.Pipeline.Helpers
{
    internal static class WebSocketExtensions
    {
        public static ValueTask SendAsync(this WebSocket webSocket,
                                          ReadOnlySequence<byte> buffer,
                                          WebSocketMessageType webSocketMessageType,
                                          CancellationToken cancellationToken = default)
        {
            if (buffer.IsSingleSegment)
            {
                return webSocket.SendAsync(buffer.First, webSocketMessageType, endOfMessage: true, cancellationToken);
            }

            return SendMultiSegmentAsync(webSocket, buffer, webSocketMessageType, cancellationToken);
        }
        
        private static async ValueTask SendMultiSegmentAsync(WebSocket webSocket, ReadOnlySequence<byte> buffer, WebSocketMessageType webSocketMessageType, CancellationToken cancellationToken = default)
        {
            var position = buffer.Start;
            // Get a segment before the loop so we can be one segment behind while writing
            // This allows us to do a non-zero byte write for the endOfMessage = true send
            buffer.TryGet(ref position, out var prevSegment);
            while (buffer.TryGet(ref position, out var segment))
            {
                await webSocket.SendAsync(prevSegment, webSocketMessageType, endOfMessage: false, cancellationToken);
                prevSegment = segment;
            }

            await webSocket.SendAsync(prevSegment, webSocketMessageType, endOfMessage: true, cancellationToken);
        }
    }
}