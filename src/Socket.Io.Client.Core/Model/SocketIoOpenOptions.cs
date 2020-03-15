using System;
using System.Collections.Generic;
using System.Text;

namespace Socket.Io.Client.Core.Model
{
    public class SocketIoOpenOptions
    {
        public SocketIoOpenOptions(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Provide optional path parameter to use instead of <see cref="Socket.Io.Client.Core.SocketIo.DefaultPath"/>.
        /// </summary>
        public string Path { get; }
    }
}
