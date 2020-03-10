using System;
using System.IO.Pipelines;
using System.Text;

namespace ClientWebSocket.Pipeline
{
    public class PipelineWebSocketOptions
    {
        public PipelineWebSocketOptions() : this(1024,
            1024,
            PipeOptions.Default,
            PipeOptions.Default,
            TimeSpan.FromSeconds(10),
            new CharSeparatorFrameSeparator('\n'),
            Encoding.UTF8)
        {
        }

        public PipelineWebSocketOptions(int receiveBufferSize,
                                        int sendBufferSize,
                                        PipeOptions inputPipeOptions,
                                        PipeOptions outputPipeOptions,
                                        TimeSpan connectTimeout,
                                        IFrameSeparator frameSeparator,
                                        Encoding defaultEncoding)
        {
            ReceiveBufferSize = receiveBufferSize;
            SendBufferSize = sendBufferSize;
            InputPipeOptions = inputPipeOptions ?? throw new ArgumentNullException(nameof(inputPipeOptions));
            OutputPipeOptions = outputPipeOptions ?? throw new ArgumentNullException(nameof(outputPipeOptions));
            ConnectTimeout = connectTimeout;
            FrameSeparator = frameSeparator ?? throw new ArgumentNullException(nameof(frameSeparator));
            DefaultEncoding = defaultEncoding;
        }

        public int ReceiveBufferSize { get; }
        public int SendBufferSize { get; }
        public PipeOptions InputPipeOptions { get; }
        public PipeOptions OutputPipeOptions { get; }
        public TimeSpan ConnectTimeout { get; }
        public IFrameSeparator FrameSeparator { get; }
        public Encoding DefaultEncoding { get; }
    }
}