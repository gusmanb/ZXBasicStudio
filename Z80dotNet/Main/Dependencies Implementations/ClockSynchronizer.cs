using System.Diagnostics;
using System.Threading;

namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Default implementation of <see cref="IClockSynchronizer"/>.
    /// </summary>
    public class ClockSynchronizer : IClockSynchronizer
    {
        private const int MinMicrosecondsToWait = 10*1000;

        public decimal EffectiveClockFrequencyInMHz { get; set; }

        private Stopwatch stopWatch = new Stopwatch();

        private decimal accummulatedMicroseconds;

        public void Start()
        {
            stopWatch.Reset();
            stopWatch.Start();
        }

        public void Stop()
        {
            stopWatch.Stop();
        }

        public void TryWait(int periodLengthInCycles)
        {
            accummulatedMicroseconds += (periodLengthInCycles / EffectiveClockFrequencyInMHz);

            var microsecondsPending = (accummulatedMicroseconds - stopWatch.ElapsedMilliseconds);

            if(microsecondsPending >= MinMicrosecondsToWait) 
            {
                Thread.Sleep((int)(microsecondsPending / 1000));
                accummulatedMicroseconds = 0;
                stopWatch.Reset();
            }
        }
    }
}
