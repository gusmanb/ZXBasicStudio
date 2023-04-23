using NUnit.Framework;
using Ploeh.AutoFixture;
using System.Diagnostics;

namespace Konamiman.Z80dotNet.Tests
{
    [Explicit]
    public class ClockSynchronizationHelperTests
    {
        private const int MinMicrosecondsToWait = 10*1000;

        private ClockSynchronizer Sut { get; set; }
        private Fixture Fixture { get; set; }

        [SetUp]
        [Repeat(1000)]
        public void Setup()
        {
            Sut = new ClockSynchronizer();
            Sut.EffectiveClockFrequencyInMHz = 1;
            Fixture = new Fixture();
        }

        [Test]
        public void TryWait_works_with_repeated_short_intervals()
        {
            Sut.Start();

            var sw = new Stopwatch();
            sw.Start();

            var totalCyclesToWait = 50000;

            for(int i = 0; i<(totalCyclesToWait / 5); i++)
                Sut.TryWait(5);

            sw.Stop();

            var expected = 50;
            var actual = sw.ElapsedMilliseconds;
            Assert.IsTrue(actual >= expected-5 && actual <= expected+5, "Actual value: " + actual);
        }

        [Test]
        public void TryWait_works_with_one_long_interval()
        {
            Sut.Start();

            var sw = new Stopwatch();
            sw.Start();

            var totalCyclesToWait = 50000;

            Sut.TryWait(totalCyclesToWait);

            sw.Stop();

            var expected = 50;
            var actual = sw.ElapsedMilliseconds;
            Assert.IsTrue(actual >= expected-5 && actual <= expected+5, "Actual value: " + actual);
        }

        [Test]
        public void TryWait_works_repeatedly()
        {
            Sut.Start();

            var sw = new Stopwatch();
            sw.Start();

            var totalCyclesToWait = 50000;

            Sut.TryWait(totalCyclesToWait);

            var expected = 50;
            var actual = sw.ElapsedMilliseconds;
            Assert.IsTrue(actual >= expected-5 && actual <= expected+5, "Actual value 1: " + actual);

            sw.Stop();

            actual = sw.ElapsedMilliseconds;
            Assert.IsTrue(actual >= expected-5 && actual <= expected+5, "Actual value 2: " + actual);
        }
    }
}
