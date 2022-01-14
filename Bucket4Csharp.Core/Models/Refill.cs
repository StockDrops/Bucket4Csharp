using Bucket4Csharp.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models
{
    /// <summary>
    /// Specifies the speed of tokens regeneration.
    /// </summary>
    public class Refill
    {
        static internal readonly long UNSPECIFIED_TIME_OF_FIRST_REFILL = long.MinValue;

        readonly long periodNanos;
        readonly long tokens;
        readonly bool refillIntervally;
        readonly long timeOfFirstRefilMillis;
        readonly bool useAdaptiveInitialTokens;

        internal long PeriodNanos => periodNanos;
        internal long Tokens => tokens;
        internal bool RefillIntervally => refillIntervally;
        internal long TimeOfFirstRefilMillis => timeOfFirstRefilMillis;
        internal bool UseAdaptiveInitialTokens => useAdaptiveInitialTokens;


        private Refill(long tokens,
            TimeSpan period, bool refillIntervally,
            long timeOfFirstRefillMillis,
            bool useAdaptiveInitialTokens)
        {
            //timespan cannot be null.
            if (tokens <= 0)
            {
                throw BucketExceptions.NonPositivePeriodTokens(tokens);
            }
            this.periodNanos = period.Nanoseconds();
            if (periodNanos <= 0)
            {
                throw BucketExceptions.NonPositivePeriod(periodNanos);
            }
            if (tokens > periodNanos)
            {
                throw BucketExceptions.TooHighRefillRate(periodNanos, tokens);
            }

            this.tokens = tokens;
            this.refillIntervally = refillIntervally;
            this.timeOfFirstRefilMillis = timeOfFirstRefillMillis;
            this.useAdaptiveInitialTokens = useAdaptiveInitialTokens;
        }
        [Obsolete]
        public static Refill Of(long tokens, TimeSpan period)
        {
            return Greedy(tokens, period);
        }

        [Obsolete]
        public static Refill Smooth(long tokens, TimeSpan period)
        {
            return Greedy(tokens, period);
        }
        /// <summary>
        /// Creates the {@link Refill} that does refill of tokens in greedy manner,
        /// it will try to add the tokens to bucket as soon as possible.
        /// For example refill "10 tokens per 1 second" will add 1 token per each 100 millisecond,
        /// in other words refill will not wait 1 second to regenerate whole bunch of 10 tokens.
        /// <para>
        /// The three refills bellow do refill of tokens with same speed:
        /// </para>
        /// <pre>
        /// <code>Refill.Greedy(600, TimeSpan.FromMinutes(1));</code>
        /// <code>Refill.Greedy(10, TimeSpan.FromSeconds(1));</code>
        /// <code>Refill.Greedy(1, TimeSpan.FromMilliseconds(100));</code>
        /// </pre>
        /// If greediness is undesired then you can specify the fixed interval refill via <see cref="Intervally(long, TimeSpan)"/>
        /// </summary>
        /// <param name="tokens">amount of tokens</param>
        /// <param name="period">the period within tokens will be fully regenerated</param>
        /// <returns>the <see cref="Refill"/> that does the refill of tokens in manner specified.</returns>
        public static Refill Greedy(long tokens, TimeSpan period)
        {
            return new Refill(tokens, period, false, UNSPECIFIED_TIME_OF_FIRST_REFILL, false);
        }
        /// <summary>
        /// Creates the <see cref="Refill"/> that does refill of tokens in intervally manner.
        /// "Intervally" in opposite to "greedy"  will wait until whole period will be elapsed before regenerating the tokens.
        /// </summary>
        /// <param name="tokens">amount of tokens</param>
        /// <param name="period">period the period within tokens will be fully regenerated</param>
        /// <returns></returns>
        public static Refill Intervally(long tokens, TimeSpan period)
        {
            return new Refill(tokens, period, true, UNSPECIFIED_TIME_OF_FIRST_REFILL, false);
        }
        /// <summary>
        /// <a href="https://github.com/vladimir-bukhtoyarov/bucket4j/blob/85a0148788223bc968fe4faa72f733b68dbf129f/bucket4j-core/src/main/java/io/github/bucket4j/Refill.java#L112">original documentation.</a>
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="period"></param>
        /// <param name="timeOfFirstRefill"></param>
        /// <param name="useAdaptiveInitialTokens"></param>
        /// <returns></returns>
        public static Refill IntervallyAligned(long tokens, TimeSpan period, DateTime timeOfFirstRefill, bool useAdaptiveInitialTokens)
        {
            long timeOfFirstRefillMillis = (long)Math.Round(timeOfFirstRefill.ToUniversalTime().Subtract(
                                        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                        ).TotalMilliseconds);
            if (timeOfFirstRefillMillis < 0)
            {
                throw BucketExceptions.NonPositiveTimeOfFirstRefill(timeOfFirstRefill);
            }
            return new Refill(tokens, period, true, timeOfFirstRefillMillis, useAdaptiveInitialTokens);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("Refill{");
            sb.Append("periodNanos=").Append(periodNanos);
            sb.Append(", tokens=").Append(tokens);
            sb.Append(", refillIntervally=").Append(refillIntervally);
            sb.Append(", timeOfFirstRefillMillis=").Append(TimeOfFirstRefilMillis);
            sb.Append(", useAdaptiveInitialTokens=").Append(useAdaptiveInitialTokens);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
