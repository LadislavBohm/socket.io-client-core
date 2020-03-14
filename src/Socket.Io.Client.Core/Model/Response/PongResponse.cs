using System;
using System.Collections.Generic;
using System.Text;

namespace Socket.Io.Client.Core.Model.Response
{
    public class PongResponse
    {
        public PongResponse(string data)
        {
            Data = data;
        }

        public string Data { get; }

        public override string ToString()
        {
            return $"{nameof(Data)}: {Data}";
        }
    }
}
