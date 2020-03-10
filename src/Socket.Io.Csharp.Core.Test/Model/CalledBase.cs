using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Socket.Io.Csharp.Core.Test.Model
{
    internal abstract class CalledBase
    {
        private int _calledTimes = 0;

        public int CalledTimes => _calledTimes;

        public void Increment() => Interlocked.Increment(ref _calledTimes);

        public void AssertOnce() => Assert.Equal(1, CalledTimes);

        public void AssertNever() => Assert.Equal(0, CalledTimes);

        public void AssertExactly(int exactly) => Assert.Equal(exactly, CalledTimes);

        public void AssertAtLeast(int atLeast) => Assert.True(CalledTimes >= atLeast);

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
                Assert.True(CalledTimes <= exactly);
                elapsed = await WaitAndIncrementAsync(elapsed, 5);
            }

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
                elapsed = await WaitAndIncrementAsync(elapsed, 5);
            }

            AssertAtLeast(atLeast);
        }

        private static async Task<TimeSpan> WaitAndIncrementAsync(TimeSpan elapsed, int wait)
        {
            await Task.Delay(wait);
            return elapsed.Add(TimeSpan.FromMilliseconds(wait));
        }
    }
}
