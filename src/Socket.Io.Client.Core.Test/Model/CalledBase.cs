using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Socket.Io.Client.Core.Test.Model
{
    internal abstract class CalledBase
    {
        private readonly ConcurrentQueue<Exception> _exceptions = new ConcurrentQueue<Exception>();
        private int _calledTimes;
        
        public int CalledTimes => _calledTimes;

        public void AddException(Exception ex) => _exceptions.Enqueue(ex);

        public void Increment() => Interlocked.Increment(ref _calledTimes);

        public void AssertOnce() => Assert.Equal(1, CalledTimes);

        public void AssertNever() => Assert.Equal(0, CalledTimes);

        public void AssertExactly(int exactly) => Assert.Equal(exactly, CalledTimes);

        public void AssertAtLeast(int atLeast) => Assert.True(CalledTimes >= atLeast, $"Expected called at least: {atLeast} actual: {CalledTimes}");

        private void AssertNoException() => Assert.Empty(_exceptions);

        public async Task AssertOnceAsync(TimeSpan timeout)
        {
            await Task.Delay(timeout);
            AssertOnce();
        }

        public async Task AssertNeverAsync(TimeSpan timeout)
        {
            var elapsed = TimeSpan.Zero;
            while (elapsed < timeout)
            {
                AssertNoException();
                AssertNever();
                elapsed = await WaitAndIncrementAsync(elapsed, 5);
            }
        }

        public Task AssertExactlyOnceAsync(TimeSpan timeout) => AssertExactlyAsync(1, timeout);

        public async Task AssertExactlyAsync(int exactly, TimeSpan timeout)
        {
            var elapsed = TimeSpan.Zero;
            while (elapsed < timeout)
            {
                if (CalledTimes == exactly) break;
                AssertNoException();
                Assert.True(CalledTimes <= exactly);
                elapsed = await WaitAndIncrementAsync(elapsed, 5);
            }

            AssertNoException();
            AssertExactly(exactly);
        }

        public Task AssertAtLeastOnceAsync(TimeSpan timeout) => AssertAtLeastAsync(1, timeout);

        public async Task AssertAtLeastAsync(int atLeast, TimeSpan timeout)
        {
            var elapsed = TimeSpan.Zero;
            while (elapsed < timeout)
            {
                if (CalledTimes >= atLeast)
                    break;
                AssertNoException();
                elapsed = await WaitAndIncrementAsync(elapsed, 5);
            }

            AssertNoException();
            AssertAtLeast(atLeast);
        }

        private static async Task<TimeSpan> WaitAndIncrementAsync(TimeSpan elapsed, int wait)
        {
            await Task.Delay(wait);
            return elapsed.Add(TimeSpan.FromMilliseconds(wait));
        }
    }
}
