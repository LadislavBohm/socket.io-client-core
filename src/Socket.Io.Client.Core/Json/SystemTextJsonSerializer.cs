using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Socket.Io.Client.Core.Json
{
    public class SystemTextJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _options;

        public SystemTextJsonSerializer(JsonSerializerOptions options = null)
        {
            _options = options;
        }

        public string Serialize<T>(T data) => JsonSerializer.Serialize(data, _options);

        public T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _options);
    }
}
