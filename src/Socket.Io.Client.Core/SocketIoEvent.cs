namespace Socket.Io.Client.Core
{
    public enum SocketIoEvent
    {
        Open,
        Connect,
        Close,
        Message,
        Error,
        UpgradeError,
        Handshake,
        Upgrading,
        Upgrade,
        Packet,
        Heartbeat,
        Data,
        Ping,
        Pong,
        ProbeError,
        ProbeSuccess
    }
}