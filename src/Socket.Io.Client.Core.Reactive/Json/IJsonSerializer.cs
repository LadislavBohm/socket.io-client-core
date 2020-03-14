using System;

namespace Socket.Io.Client.Core.Reactive.Json
{
    public interface IJsonSerializer
    {
        string Serialize<T>(T data);

        T Deserialize<T>(string json);
    }
}
