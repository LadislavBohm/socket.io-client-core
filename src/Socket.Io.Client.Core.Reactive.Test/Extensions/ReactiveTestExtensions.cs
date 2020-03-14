using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Socket.Io.Client.Core.Reactive.Test.Model;

namespace Socket.Io.Client.Core.Reactive.Test.Extensions
{
    internal static class ReactiveTestExtensions
    {
        internal static Called<T> SubscribeCalled<T>(this IObservable<T> observable, Action<T> action = null)
        {
            return new Called<T>(observable, action);
        }
    }
}
