using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Socket.Io.Client.Core.Reactive.Json
{
    public class Utf8JsonSerializer : IJsonSerializer
    {
        private readonly IJsonFormatterResolver _formatterResolver;

        public Utf8JsonSerializer(IJsonFormatterResolver formatterResolver)
        {
            _formatterResolver = formatterResolver;
        }

        public string Serialize<T>(T data) => JsonSerializer.ToJsonString(data, _formatterResolver);

        public T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _formatterResolver);
    }
}
