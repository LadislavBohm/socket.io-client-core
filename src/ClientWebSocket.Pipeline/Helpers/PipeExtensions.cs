using System;
using System.IO.Pipelines;

namespace ClientWebSocket.Pipeline.Helpers
{
    internal static class PipeExtensions
    {
        internal static void Close(this IDuplexPipe pipe, Exception ex = null)
        {
            if (pipe != null)
            {
                // burn the pipe to the ground
                try { pipe.Input.Complete(ex); } catch { }
                try { pipe.Input.CancelPendingRead(); } catch { }
                try { pipe.Output.Complete(ex); } catch { }
                try { pipe.Output.CancelPendingFlush(); } catch { }
                if (pipe is IDisposable d) try { d.Dispose(); } catch { }
            }
        }
    }
}