using System.Buffers;
using System.IO.Pipelines;

namespace ClientWebSocket.Pipeline
{
    public interface IFrameSeparator
    {
        void WriteEndOfFrame(PipeWriter writer);   
        bool TryReadFrame(ref ReadOnlySequence<byte> input, out ReadOnlySequence<byte> payload);
    }
}