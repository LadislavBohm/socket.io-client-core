namespace Socket.Io.Client.Core.Model.SocketIo
{
    /// <summary>
    /// Engine-io type
    /// </summary>
    public enum EngineIoType
    {
        Open = 0,
        Close = 1,
        Ping = 2,
        Pong = 3,
        Message = 4,
        Upgrade = 5,
        Noop = 6,
        Error = 7
    }
}