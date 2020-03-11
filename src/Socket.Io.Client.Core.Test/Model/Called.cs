using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Socket.Io.Client.Core.Test.Model
{
    internal class Called : CalledBase
    {
        public Called(Func<ValueTask> callback)
        {
            Callback = callback;
        }

        public Func<ValueTask> Callback { get; }
    }
}
