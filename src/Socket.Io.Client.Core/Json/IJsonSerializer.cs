using System;

namespace Socket.Io.Client.Core.Json
{
    public interface IJsonSerializer
    {
        string Serialize<T>(T data);

        T Deserialize<T>(string json);
    }
}
