using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Socket.Io.Csharp.Core.Test.Extensions
{
    internal static class TaskExtensions
    {
        internal static async Task WaitForAsync(this Task task, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout);
            var result = await Task.WhenAny(task, timeoutTask);
            Assert.Equal(result, task);
        }

        internal static async Task<Task<T>> WaitForAsync<T>(this Task<T> task, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout);
            var result = await Task.WhenAny(task, timeoutTask);
            Assert.Equal(result, task);
            await task;
            return task;
        }
    }
}
