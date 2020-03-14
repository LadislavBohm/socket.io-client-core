namespace Socket.Io.Client.Core.Reactive.Model.SocketIo
{
    /// <summary>
    /// Socket-io type
    /// </summary>
    public enum SocketIoType
    {
        Connect = 0,
        Disconnect = 1,
        Event = 2,
        Ack = 3,
        Error = 4,
        BinaryEvent = 5,
        BinaryAck = 6
    }
}