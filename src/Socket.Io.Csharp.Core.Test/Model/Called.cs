using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Socket.Io.Csharp.Core.Test.Model
{
    internal class Called
    {
        private int _times = 0;

        public int Times => _times;

        public void Increment() => Interlocked.Increment(ref _times);

        public void AssertOnce() => Assert.Equal(1, Times);

        public void AssertNever() => Assert.Equal(0, Times);

        public void AssertExactly(int exactly) => Assert.Equal(exactly, Times);

        public void AssertAtLeast(int atLeast) => Assert.True(Times >= atLeast);

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
                if (Times == exactly) break;
                Assert.True(Times <= exactly);
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
                if (Times >= atLeast)
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
