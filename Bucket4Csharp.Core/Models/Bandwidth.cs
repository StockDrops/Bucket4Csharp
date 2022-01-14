using Bucket4Csharp.Core.Exceptions;
using Bucket4Csharp.Core.Interfaces.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models
{
    public class Bandwidth : ComparableByContent<Bandwidth>
    {
        public static readonly string? UNDEFINED_ID;

        //see https://stackoverflow.com/questions/16164902/what-is-the-default-access-modifier-in-java
        //no modifier in java = internal (well package-private in java terms).
        internal readonly long capacity;
        internal readonly long initialTokens;
        internal readonly long refillPeriodNanos;
        internal readonly long refillTokens;
        internal readonly bool refillIntervally;
        internal readonly long timeOfFirstRefillMillis;
        internal readonly bool useAdaptiveInitialTokens;
        internal readonly string? id;

        private Bandwidth(long capacity,
            long refillPeriodNanos,
            long refillTokens,
            long initialTokens,
            bool refillIntervally,
            long timeOfFirstRefillMillis,
            bool useAdaptiveInitialTokens,
            string? id)
        {
            this.capacity = capacity;
            this.initialTokens = initialTokens;
            this.refillPeriodNanos = refillPeriodNanos;
            this.refillTokens = refillTokens;
            this.refillIntervally = refillIntervally;
            this.timeOfFirstRefillMillis = timeOfFirstRefillMillis;
            this.useAdaptiveInitialTokens = useAdaptiveInitialTokens;
            this.id = id;
        }
        /// <summary>
        /// Specifies simple limitation <tt>capacity</tt> tokens per <tt>period</tt> time window.
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static Bandwidth Simple(long capacity, TimeSpan period)
        {
            Refill refill = Refill.Greedy(capacity, period);
            return Classic(capacity, refill);
        }
        /// <summary>
        /// Specifies limitation in conventional interpretation of token-bucket algorithm.
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="refill"></param>
        /// <returns></returns>
        public static Bandwidth Classic(long capacity, Refill refill)
        {
            if (capacity <= 0)
            {
                throw BucketExceptions.NonPositiveCapacity(capacity);
            }
            if (refill == null)
            {
                throw BucketExceptions.NullBandwidthRefill();
            }
            return new Bandwidth(capacity, refill.PeriodNanos, refill.Tokens, capacity, refill.RefillIntervally,
                    refill.TimeOfFirstRefilMillis, refill.UseAdaptiveInitialTokens, UNDEFINED_ID);
        }
        public Bandwidth WithInitialTokens(long initialTokens)
        {
            if (initialTokens < 0)
            {
                throw BucketExceptions.NonPositiveInitialTokens(initialTokens);
            }
            if (IsIntervallyAligned && UseAdaptiveInitialTokens)
            {
                throw BucketExceptions.IntervallyAlignedRefillWithAdaptiveInitialTokensIncompatipleWithManualSpecifiedInitialTokens();
            }
            return new Bandwidth(capacity, refillPeriodNanos, refillTokens, initialTokens, refillIntervally,
                    timeOfFirstRefillMillis, useAdaptiveInitialTokens, UNDEFINED_ID);
        }
        /// <summary>
        /// By default new created bandwidth has no ID.
        /// This method allows to specify unique identifier of bandwidth that can be used for bandwidth comparision during configuration replacement {@link Bucket#replaceConfiguration(BucketConfiguration, TokensInheritanceStrategy)}
        /// </summary>
        /// <param name="id">This method allows to specify unique identifier of bandwidth that can be used for bandwidth comparision during configuration replacement {@link Bucket#replaceConfiguration(BucketConfiguration, TokensInheritanceStrategy)}</param>
        /// <returns>the copy of this bandwidth with new value ofof initial tokens.</returns>
        public Bandwidth WithId(string id)
        {
            return new Bandwidth(capacity, refillPeriodNanos, refillTokens, initialTokens, refillIntervally,
                    timeOfFirstRefillMillis, useAdaptiveInitialTokens, id);
        }
        public bool IsIntervallyAligned => timeOfFirstRefillMillis != Refill.UNSPECIFIED_TIME_OF_FIRST_REFILL;

        public long TimeOfFirstRefillMillis => timeOfFirstRefillMillis;
        public long Capacity => capacity;
        public long InitialTokens => initialTokens;
        public long RefillPeriodNanos => refillPeriodNanos;
        public long RefillTokens => refillTokens;
        public bool UseAdaptiveInitialTokens => useAdaptiveInitialTokens;
        public bool RefillIntervally => refillIntervally;
        /// <summary>
        /// Original property in Java's version is "isGready" but that's an obvious typo. Gready is not an adjective in english.
        /// </summary>
        public bool IsGreedy => !refillIntervally;
        public string? Id => id;
        //serialization properties are not ported yet!.
        //https://github.com/vladimir-bukhtoyarov/bucket4j/blob/85a0148788223bc968fe4faa72f733b68dbf129f/bucket4j-core/src/main/java/io/github/bucket4j/Bandwidth.java#L197
        public bool HasId => id != null;
        public override bool Equals(object? o)
        {
            if (this == o) { return true; }
            if (o == null || GetType() != o.GetType()) { return false; }

            Bandwidth? bandwidth = o as Bandwidth;
            if(bandwidth == null) { return false; }

            if (capacity != bandwidth.capacity) { return false; }
            if (initialTokens != bandwidth.initialTokens) { return false; }
            if (refillPeriodNanos != bandwidth.refillPeriodNanos) { return false; }
            if (refillTokens != bandwidth.refillTokens) { return false; }
            if (refillIntervally != bandwidth.refillIntervally) { return false; }
            if (timeOfFirstRefillMillis != bandwidth.timeOfFirstRefillMillis) { return false; }
            return useAdaptiveInitialTokens == bandwidth.useAdaptiveInitialTokens;
        }
        public override int GetHashCode()
        {
            ulong unsignedCapacity = (ulong)capacity;
            ulong unsignedInitialTokens = (ulong)initialTokens;
            ulong unsignedRefillPeriodNanos = (ulong)refillPeriodNanos;
            ulong unsignedRefillTokens = (ulong)refillTokens;
            ulong unsignedTimeOfFirstRefillMillis = (ulong)timeOfFirstRefillMillis;

            int result = (int)(unsignedCapacity ^ (unsignedCapacity >> 32));
            result = 31 * result + (int)(unsignedInitialTokens ^ (unsignedInitialTokens >> 32));
            result = 31 * result + (int)(unsignedRefillPeriodNanos ^ (unsignedRefillPeriodNanos >> 32));
            result = 31 * result + (int)(unsignedRefillTokens ^ (unsignedRefillTokens >> 32));
            result = 31 * result + (refillIntervally ? 1 : 0);
            result = 31 * result + (int)(unsignedTimeOfFirstRefillMillis ^ (unsignedTimeOfFirstRefillMillis >> 32));
            result = 31 * result + (useAdaptiveInitialTokens ? 1 : 0);
            return result;
        }
        public override string ToString()
        {
            var sb = new StringBuilder("Bandwidth{");
            sb.Append("capacity=").Append(capacity);
            sb.Append(", initialTokens=").Append(initialTokens);
            sb.Append(", refillPeriodNanos=").Append(refillPeriodNanos);
            sb.Append(", refillTokens=").Append(refillTokens);
            sb.Append(", refillIntervally=").Append(refillIntervally);
            sb.Append(", timeOfFirstRefillMillis=").Append(timeOfFirstRefillMillis);
            sb.Append(", useAdaptiveInitialTokens=").Append(useAdaptiveInitialTokens);
            sb.Append('}');
            return sb.ToString();
        }
        public override bool EqualsByContent(Bandwidth? other)
        {
            if(other == null) { return false; }
            return capacity == other.capacity &&
                initialTokens == other.initialTokens &&
                refillPeriodNanos == other.refillPeriodNanos &&
                refillTokens == other.refillTokens &&
                refillIntervally == other.refillIntervally &&
                timeOfFirstRefillMillis == other.timeOfFirstRefillMillis &&
                useAdaptiveInitialTokens == other.useAdaptiveInitialTokens;
        }
    }
}
