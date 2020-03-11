using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Socket.Io.Client.Core.Test.Extensions
{
    internal static class TaskExtensions
    {
        internal static async Task TimoutAfterAsync(this Task task, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout);
            var result = await Task.WhenAny(task, timeoutTask);
            Assert.Equal(result, task);
        }

        internal static async Task<Task<T>> TimoutAfterAsync<T>(this Task<T> task, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout);
            var result = await Task.WhenAny(task, timeoutTask);
            Assert.Equal(result, task);
            await task;
            return task;
        }
    }
}
