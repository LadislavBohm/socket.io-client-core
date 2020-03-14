using System;

namespace Socket.Io.Client.Core.Reactive
{
    public partial class SocketIoClient
    {
        private void ThrowIfStarted()
        {
            if (_socket != null)
                throw new InvalidOperationException("Socket is already running, stop it first.");
        }

        private void ThrowIfNotRunning()
        {
            //if (_socket == null || !_socket.IsRunning && State != ReadyState.Opening)
            //    throw new InvalidOperationException("Socket is not running.");
        }

        private void ThrowIfNotStarted()
        {
            if (_socket == null || !_socket.IsStarted)
                throw new InvalidOperationException("Socket is not running, start it first.");
        }
    }
}
