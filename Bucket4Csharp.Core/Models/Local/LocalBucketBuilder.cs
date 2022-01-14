using Bucket4Csharp.Core.Exceptions;
using Bucket4Csharp.Core.Extensions;
using Bucket4Csharp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models.Local
{
    /// <summary>
    /// This builder creates in-memory buckets <see cref="LockFreeBucket"/>
    /// </summary>
    public class LocalBucketBuilder
    {
        private readonly ConfigurationBuilder configurationBuilder;
        public LocalBucketBuilder()
        {
            configurationBuilder = new ConfigurationBuilder();
        }
        public LocalBucketBuilder AddLimit(Bandwidth bandwidth)
        {
            configurationBuilder.AddLimit(bandwidth);
            return this;
        }
        private ITimeMeter timeMeter = ITimeMeter.SystemTimeMeter;
        private SynchronizationStrategy synchronizationStrategy = SynchronizationStrategy.LockFree;
        private MathType mathType = MathType.Integer64Bits;
        /// <summary>
        /// The maximum resolution in c# is one tick = 100 ns: <a href="https://manski.net/2014/07/high-resolution-clock-in-csharp/#:~:text=One%20tick%20is%20100%20nanoseconds,be%20up%20to%200.0001%20milliseconds">source</a>
        /// </summary>
        /// <returns></returns>
        public LocalBucketBuilder WithHundredNanosecondPrecision()
        {
            this.timeMeter = ITimeMeter.StopWatchTimeMeter;
            return this;
        }
        /// <summary>
        /// Proxy method for <see cref="WithHundredNanosecondPrecision"/> for easy compatibility with java's original code.
        /// This is because in java you can get a higher precision than in c# not that it would matter to be honest.
        /// 1 ns or 100 ns you wouldn't be able to tell... having a physics background is crazy how most devs have 0 clue about orders of magnitudes, and significant digits. Rant finished.
        /// </summary>
        /// <returns></returns>
        public LocalBucketBuilder WithNanosecondPrecision() => WithHundredNanosecondPrecision();
        public LocalBucketBuilder WithMillisecondPrecision()
        {
            this.timeMeter = ITimeMeter.SystemTimeMeter;
            return this;
        }
        public LocalBucketBuilder WithCustomTimePrecision(ITimeMeter customTimeMeter)
        {
            if (customTimeMeter == null)
            {
                throw BucketExceptions.NullTimeMeter();
            }
            this.timeMeter = customTimeMeter;
            return this;
        }
        public LocalBucketBuilder WithSynchronizationStrategy(SynchronizationStrategy synchronizationStrategy)
        {
            this.synchronizationStrategy = synchronizationStrategy;
            return this;
        }
        public LocalBucketBuilder WithMath(MathType mathType)
        {
            this.mathType = Objects.RequireNotNull(mathType);
            return this;
        }
        public ILocalBucket Build()
        {
            BucketConfiguration configuration = BuildConfiguration();
            switch (synchronizationStrategy)
            {
                case SynchronizationStrategy.LockFree: return new LockFreeBucket(configuration, mathType, timeMeter);
                    //we haven't ported the stuff yet.
                //case SynchronizationStrategy.Synchronized: return new SynchronizedBucket(configuration, mathType, timeMeter);
                //case SynchronizationStrategy.None: return new SynchronizedBucket(configuration, mathType, timeMeter, FakeLock.INSTANCE);
                default: throw new InvalidOperationException(); //https://stackoverflow.com/questions/2108544/java-lang-illegalstateexception-in-net
            }
        }
        private BucketConfiguration BuildConfiguration()
        {
            BucketConfiguration configuration = configurationBuilder.Build();
            foreach (Bandwidth bandwidth in configuration.Bandwidths)
            {
                if (bandwidth.IsIntervallyAligned && !timeMeter.IsWallClockBased)
                {
                    throw BucketExceptions.IntervallyAlignedRefillCompatibleOnlyWithWallClock();
                }
            }
            return configuration;
        }

    }
}
