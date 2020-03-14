using System;
using Socket.Io.Client.Core.Reactive.Model.SocketIo;

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
            if (_socket == null || !_socket.IsRunning && State != ReadyState.Opening)
                throw new InvalidOperationException("Socket is not running.");
        }

        private void ThrowIfNotStarted()
        {
            if (_socket == null || !_socket.IsStarted)
                throw new InvalidOperationException("Socket is not running, start it first.");
        }

        private void ThrowIfInvalidEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentException("Event name must not be null or empty.");
        }
    }
}
