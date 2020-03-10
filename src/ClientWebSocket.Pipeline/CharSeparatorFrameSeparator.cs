using System;
using System.Buffers;
using System.IO.Pipelines;

namespace ClientWebSocket.Pipeline
{
    public class CharSeparatorFrameSeparator : IFrameSeparator
    {
        private readonly char _separator;

        public CharSeparatorFrameSeparator(char separator)
        {
            _separator = separator;
        }
        
        public void WriteEndOfFrame(PipeWriter writer)
        {
            writer.GetSpan(1).Slice(0, 1).Fill((byte)_separator);
            writer.Advance(1);
        }
        
        public bool TryReadFrame(ref ReadOnlySequence<byte> input, out ReadOnlySequence<byte> payload)
        {
            if (input.IsEmpty || input.Length < 2)
            {
                payload = default;
                return false;
            }

            SequencePosition? position = input.PositionOf((byte)_separator);
            if (position == null)
            {
                payload = default;
                return false;
            }

            payload = input.Slice(0, position.Value);
            input = input.Slice(input.GetPosition(1, position.Value));
            return true;
        }
    }
}