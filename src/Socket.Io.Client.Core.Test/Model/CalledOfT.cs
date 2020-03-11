using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Socket.Io.Client.Core.Test.Model
{
    internal class Called<T> : CalledBase
    {
        public Called(Func<T, ValueTask> callback)
        {
            Callback = callback;
        }

        public Func<T, ValueTask> Callback { get; }
    }
}
