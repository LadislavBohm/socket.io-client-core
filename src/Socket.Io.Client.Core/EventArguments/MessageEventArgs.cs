using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Socket.Io.Client.Core.EventArguments
{
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(IReadOnlyList<string> data)
        {
            Data = data;
        }

        public IReadOnlyList<string> Data { get; }

        [JsonIgnore]
        public string FirstData => Data?.FirstOrDefault();

        [JsonIgnore]
        public new static MessageEventArgs Empty { get; } = new MessageEventArgs(new List<string>());
    }
}
