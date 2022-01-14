using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Exceptions
{
    public static class BucketExceptions
    {
        public static ArgumentException NonPositiveCapacity(long capacity) => new ArgumentException($"{capacity} is wrong value for capacity, because capacity should be positive");
        public static ArgumentException NonPositiveInitialTokens(long initialTokens) => new ArgumentException($"{initialTokens} is wrong value for initial capacity, because initial tokens count should be positive");
        public static ArgumentException NullBandwidth() => new ArgumentException("Bandwidth can not be null");
        public static ArgumentException NullBandwidthRefill() => new ArgumentException("Bandwidth refill can not be null");
        public static ArgumentException NullTimeMeter() => new ArgumentException("Time meter can not be null");
        public static ArgumentException NullSynchronizationStrategy() => new ArgumentException("Synchronization strategy can not be null");
        public static ArgumentException NullListener() => new ArgumentException("Listener can not be null");
        public static ArgumentException NullRefillPeriod() => new ArgumentException("Refill period can not be null");
        public static ArgumentException NullFixedRefillInterval() => new ArgumentException("Fixed refill interval can not be null");
        public static ArgumentException NullScheduler() => new ArgumentException("Scheduler can not be null");
        public static ArgumentException NullConfiguration() => new ArgumentException("Configuration can not be null");
        /// <summary>
        /// Should probably be eliminated or replaced. Futures are not really a thing in c# at least modern c#.
        /// </summary>
        /// <returns></returns>
        public static ArgumentException NullConfigurationFuture() => new ArgumentException("Configuration future can not be null");

        public static ArgumentException NullConfigurationSupplier() => new ArgumentException("Configuration supplier can not be null");


        public static ArgumentException NonPositivePeriod(long period) 
            => new ArgumentException($"{period} is wrong value for period of bandwidth, because period should be positive");

        public static ArgumentException NonPositiveLimitToSync(long unsynchronizedPeriod) 
            => new ArgumentException($"{unsynchronizedPeriod} is wrong value for limit to sync, because period should be positive");

        public static ArgumentException NonPositiveFixedRefillInterval(TimeSpan fixedRefillInterval)
            => new ArgumentException($"{fixedRefillInterval} is wrong value for fixed refill interval, because period should be positive");
        public static ArgumentException NonPositivePeriodTokens(long tokens)
            => new ArgumentException($"{tokens} is wrong value for period tokens, because tokens should be positive");

        // TODO add test
        public static ArgumentException NonPositiveTokensForDelayParameters(long maxUnsynchronizedTokens)
            => new ArgumentException($"{maxUnsynchronizedTokens} is wrong value for maxUnsynchronizedTokens, because tokens should be positive.");

        // TODO add test
        public static ArgumentException NullMaxTimeoutBetweenSynchronizationForDelayParameters()
            => new ArgumentException("maxTimeoutBetweenSynchronization can not be null.");
        // TODO add test
        public static ArgumentException NonPositiveMaxTimeoutBetweenSynchronizationForDelayParameters(TimeSpan maxTimeoutBetweenSynchronization)
            => new ArgumentException($"maxTimeoutBetweenSynchronization = {maxTimeoutBetweenSynchronization}, maxTimeoutBetweenSynchronization can not be negative."); //TODO: is this an issue in c#?


        // TODO add test
        public static ArgumentException WrongValueOfMinSamplesForPredictionParameters(int minSamples)
            => new ArgumentException($"minSamples = {minSamples}, minSamples must be >= 2");
        // TODO add test
        public static ArgumentException MaxSamplesForPredictionParametersCanNotBeLessThanMinSamples(int minSamples, int maxSamples)
            => new ArgumentException($"minSamples = {minSamples}, maxSamples = {maxSamples}, maxSamples must be >= minSamples");

        // TODO add test
        public static ArgumentException NonPositiveSampleMaxAgeForPredictionParameters(long maxUnsynchronizedTimeoutNanos)
            => new ArgumentException($"maxUnsynchronizedTimeoutNanos = {maxUnsynchronizedTimeoutNanos}, maxUnsynchronizedTimeoutNanos must be positive");

        public static ArgumentException RestrictionsNotSpecified()
            => new ArgumentException("At least one limited bandwidth should be specified");

        public static ArgumentException TooHighRefillRate(long periodNanos, long tokens)
        {
            double actualRate = tokens / (double)periodNanos;
            string pattern = $"{0} token/nanosecond is not permitted refill rate" +
                    ", because highest supported rate is 1 token/nanosecond";
            string msg = string.Format(pattern, actualRate);
            return new ArgumentException(msg);
        }
        //might never happen in c#.
        public static ArgumentException NonPositiveTimeOfFirstRefill(DateTime timeOfFirstRefill)
        {
            string pattern = "{0} is wrong value for timeOfFirstRefill, because timeOfFirstRefill should be a positive date";
            string msg = string.Format(pattern, timeOfFirstRefill);
            return new ArgumentException(msg);
        }

        public static ArgumentException IntervallyAlignedRefillWithAdaptiveInitialTokensIncompatipleWithManualSpecifiedInitialTokens()
        {
            string msg = "Intervally aligned Refill With adaptive initial tokens incompatiple with maanual specified initial tokens";
            return new ArgumentException(msg);
        }

        public static ArgumentException IntervallyAlignedRefillCompatibleOnlyWithWallClock()
        {
            string msg = "intervally aligned refill is compatible only with wall-clock style TimeMeter";
            return new ArgumentException(msg);
        }

        public static ArgumentException FoundTwoBandwidthsWithSameId(int firstIndex, int secondIndex, String id)
        {
            string pattern = "All identifiers must unique. Id: {0}, first index: {1}, second index: {2}";
            string msg = string.Format(pattern, id, firstIndex, secondIndex);
            return new ArgumentException(msg);
        }

        // ------------------- end of construction time exceptions --------------------------------

        // ------------------- usage time exceptions  ---------------------------------------------
        public static ArgumentException NonPositiveNanosToWait(long waitIfBusyNanos)
        {
            string pattern = "Waiting value should be positive, {0} is wrong waiting period";
            string msg = string.Format(pattern, waitIfBusyNanos);
            return new ArgumentException(msg);
        }

        public static ArgumentException NonPositiveTokensToConsume(long tokens)
        {
            string pattern = "Unable to consume {0} tokens, due to number of tokens to consume should be positive";
            string msg = string.Format(pattern, tokens);
            return new ArgumentException(msg);
        }

        public static ArgumentException NonPositiveTokensLimitToSync(long tokens)
        {
            string pattern = "Sync threshold tokens should be positive, {0} is wrong waiting period";
            string msg = string.Format(pattern, tokens);
            return new ArgumentException(msg);
        }

        public static ArgumentException ReservationOverflow()
        {
            string msg = "Existed hardware is unable to service the reservation of so many tokens";
            return new ArgumentException(msg);
        }

        public static ArgumentException NullTokensInheritanceStrategy()
        {
            string msg = "Tokens migration mode must not be null";
            return new ArgumentException(msg);
        }

        public static BucketExecutionException ExecutionException(Exception cause)
        {
            return new BucketExecutionException(cause);
        }

        public static InvalidOperationException AsyncModeIsNotSupported()
        {
            string msg = "Asynchronous mode is not supported";
            return new InvalidOperationException(msg);
        }

        public class BucketExecutionException : Exception
        {
            public BucketExecutionException()
            {
            }

            public BucketExecutionException(string? message) : base(message)
            {
            }
            public BucketExecutionException(Exception? innerException) : base("A runtime error happened.", innerException) { }

            public BucketExecutionException(string? message, Exception? innerException) : base(message, innerException)
            {
            }

            protected BucketExecutionException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }
}
