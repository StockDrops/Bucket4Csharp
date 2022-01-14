using Bucket4Csharp.Core.Models;

namespace Bucket4Csharp.Core.Interfaces
{
    public class BucketState64BitsInteger : IBucketState
    {
        private static readonly int BANDWIDTH_SIZE = 3;

        readonly long[] stateData;

        BucketState64BitsInteger(long[] stateData)
        {
            this.stateData = stateData;
        }
        public BucketState64BitsInteger(BucketConfiguration configuration, long currentTimeNanos)
        {
            Bandwidth[] bandwidths = configuration.Bandwidths;

            this.stateData = new long[bandwidths.Length * 3];
            for (int i = 0; i < bandwidths.Length; i++)
            {
                SetCurrentSize(i, CalculateInitialTokens(bandwidths[i], currentTimeNanos));
                SetLastRefillTimeNanos(i, CalculateLastRefillTimeNanos(bandwidths[i], currentTimeNanos));
            }
        }
        public IBucketState Copy()
        {
            return new BucketState64BitsInteger((long[])stateData.Clone());
        }
        public IBucketState ReplaceConfiguration(BucketConfiguration previousConfiguration, BucketConfiguration newConfiguration,
                                            TokensInheritanceStrategy tokensInheritanceStrategy, long currentTimeNanos)
        {
            if (tokensInheritanceStrategy == TokensInheritanceStrategy.Reset)
            {
                return new BucketState64BitsInteger(newConfiguration, currentTimeNanos);
            }

            bool nullIdComparisonCanBeApplied = CountOfBandwidthsWithNullIdentifiers(previousConfiguration) < 2
                    && CountOfBandwidthsWithNullIdentifiers(newConfiguration) < 2;

            Bandwidth[] previousBandwidths = previousConfiguration.Bandwidths;
            Bandwidth[] newBandwidths = newConfiguration.Bandwidths;

            BucketState64BitsInteger newState = new BucketState64BitsInteger(new long[newBandwidths.Length * 3]);
            for (int newBandwidthIndex = 0; newBandwidthIndex < newBandwidths.Length; newBandwidthIndex++)
            {
                Bandwidth newBandwidth = newBandwidths[newBandwidthIndex];
                Bandwidth? previousBandwidth = null;
                int previousBandwidthIndex = -1;
                if (newBandwidth.Id != null || nullIdComparisonCanBeApplied)
                {
                    for (int j = 0; j < previousBandwidths.Length; j++)
                    {
                        if (newBandwidth.Id == previousBandwidths[j].Id)
                        {
                            previousBandwidth = previousBandwidths[j];
                            previousBandwidthIndex = j;
                            break;
                        }
                    }
                }
                if (previousBandwidth == null)
                {
                    newState.SetCurrentSize(newBandwidthIndex, CalculateInitialTokens(newBandwidth, currentTimeNanos));
                    newState.SetLastRefillTimeNanos(newBandwidthIndex, CalculateLastRefillTimeNanos(newBandwidth, currentTimeNanos));
                    continue;
                }

                switch (tokensInheritanceStrategy)
                {
                    case TokensInheritanceStrategy.AsIs:
                        ReplaceBandwidthAsIs(newState, newBandwidthIndex, newBandwidth, previousBandwidthIndex, previousBandwidth, currentTimeNanos);
                        break;
                    case TokensInheritanceStrategy.Proportionally:
                        ReplaceBandwidthProportional(newState, newBandwidthIndex, newBandwidth, previousBandwidthIndex, previousBandwidth, currentTimeNanos);
                        break;
                    case TokensInheritanceStrategy.Additive:
                        ReplaceBandwidthAdditive(newState, newBandwidthIndex, newBandwidth, previousBandwidthIndex, previousBandwidth, currentTimeNanos);
                        break;
                    default: throw new InvalidOperationException("Should never reach there");
                }
            }
            return newState;
        }

        private void ReplaceBandwidthAsIs(BucketState64BitsInteger newState, int newBandwidthIndex, Bandwidth newBandwidth,
                                      int previousBandwidthIndex, Bandwidth previousBandwidth, long currentTimeNanos)
        {
            long lastRefillTimeNanos = GetLastRefillTimeNanos(previousBandwidthIndex);
            newState.SetLastRefillTimeNanos(newBandwidthIndex, lastRefillTimeNanos);

            long currentSize = GetCurrentSize(previousBandwidthIndex);
            if (currentSize >= newBandwidth.capacity)
            {
                newState.SetCurrentSize(newBandwidthIndex, newBandwidth.Capacity);
                return;
            }
            if (newBandwidth.IsGreedy && previousBandwidth.IsGreedy)
            {
                long newSize = Math.Min(newBandwidth.Capacity, currentSize);
                newState.SetCurrentSize(newBandwidthIndex, newSize);

                long roundingError = GetRoundingError(previousBandwidthIndex);
                double roundingScale = (double)newBandwidth.refillPeriodNanos / (double)previousBandwidth.refillPeriodNanos;
                long newRoundingError = (long)roundingScale * roundingError;
                if (newRoundingError >= newBandwidth.refillPeriodNanos)
                {
                    newRoundingError = newBandwidth.refillPeriodNanos - 1;
                }
                newState.SetRoundingError(newBandwidthIndex, newRoundingError);
                return;
            }
            else
            {
                long newSize = Math.Min(newBandwidth.capacity, currentSize);
                newState.SetCurrentSize(newBandwidthIndex, newSize);
            }
        }
        private void ReplaceBandwidthProportional(BucketState64BitsInteger newState, int newBandwidthIndex, Bandwidth newBandwidth, int previousBandwidthIndex, Bandwidth previousBandwidth, long currentTimeNanos)
        {
            newState.SetLastRefillTimeNanos(newBandwidthIndex, GetLastRefillTimeNanos(previousBandwidthIndex));
            long currentSize = GetCurrentSize(previousBandwidthIndex);
            if (currentSize >= previousBandwidth.capacity)
            {
                // can come here if forceAddTokens has been used
                newState.SetCurrentSize(newBandwidthIndex, newBandwidth.capacity);
                return;
            }

            long roundingError = GetRoundingError(previousBandwidthIndex);
            double realRoundedError = (double)roundingError / (double)previousBandwidth.refillPeriodNanos;
            double scale = (double)newBandwidth.capacity / (double)previousBandwidth.capacity;
            double realNewSize = ((double)currentSize + realRoundedError) * scale;
            long newSize = (long)realNewSize;

            if (newSize >= newBandwidth.capacity)
            {
                newState.SetCurrentSize(newBandwidthIndex, newBandwidth.capacity);
                return;
            }
            if (newSize == long.MinValue)
            {
                newState.SetCurrentSize(newBandwidthIndex, long.MinValue);
                return;
            }

            double restOfDivision = realNewSize - newSize;
            if (restOfDivision > 1.0d || restOfDivision < -1.0d)
            {
                restOfDivision = realNewSize % 1;
            }
            if (restOfDivision == 0.0d)
            {
                newState.SetCurrentSize(newBandwidthIndex, newSize);
                return;
            }

            if (realNewSize < 0)
            {
                newSize--;
                restOfDivision = restOfDivision + 1;
            }
            newState.SetCurrentSize(newBandwidthIndex, newSize);
            if (newBandwidth.IsGreedy)
            {
                long newRoundingError = (long)(restOfDivision * newBandwidth.refillPeriodNanos);
                newState.SetRoundingError(newBandwidthIndex, newRoundingError);
            }
        }

        private void ReplaceBandwidthAdditive(BucketState64BitsInteger newState, int newBandwidthIndex, Bandwidth newBandwidth,
                                              int previousBandwidthIndex, Bandwidth previousBandwidth, long currentTimeNanos)
        {
            if (newBandwidth.capacity <= previousBandwidth.capacity)
            {
                ReplaceBandwidthAsIs(newState, newBandwidthIndex, newBandwidth, previousBandwidthIndex, previousBandwidth, currentTimeNanos);
                return;
            }
            long lastRefillTimeNanos = GetLastRefillTimeNanos(previousBandwidthIndex);
            newState.SetLastRefillTimeNanos(newBandwidthIndex, lastRefillTimeNanos);

            long currentSize = GetCurrentSize(previousBandwidthIndex);
            if (currentSize >= previousBandwidth.capacity)
            {
                newState.SetCurrentSize(newBandwidthIndex, newBandwidth.capacity);
                return;
            }

            long newSize = currentSize + (newBandwidth.capacity - previousBandwidth.capacity);
            newState.SetCurrentSize(newBandwidthIndex, newSize);

            if (newSize < newBandwidth.capacity && newBandwidth.IsGreedy && previousBandwidth.IsGreedy)
            {
                long roundingError = GetRoundingError(previousBandwidthIndex);
                double roundingScale = (double)newBandwidth.refillPeriodNanos / (double)previousBandwidth.refillPeriodNanos;
                long newRoundingError = (long)roundingScale * roundingError;
                if (newRoundingError >= newBandwidth.refillPeriodNanos)
                {
                    newRoundingError = newBandwidth.refillPeriodNanos - 1;
                }
                newState.SetRoundingError(newBandwidthIndex, newRoundingError);
            }
        }

        private int CountOfBandwidthsWithNullIdentifiers(BucketConfiguration configuration)
        {
            Bandwidth[] bandwidths = configuration.Bandwidths;
            int count = 0;
            for (int i = 0; i < bandwidths.Length; i++)
            {
                if (bandwidths[i].Id == null)
                {
                    count++;
                }
            }
            return count;
        }
        public void CopyStateFrom(IBucketState sourceState)
        {
            BucketState64BitsInteger sourceState64BitsInteger = (BucketState64BitsInteger)sourceState;
            Array.Copy(sourceState64BitsInteger.stateData, 0, stateData, 0, stateData.Length);
        }
        public long GetAvailableTokens(Bandwidth[] bandwidths)
        {
            long availableTokens = GetCurrentSize(0);
            for (int i = 1; i < bandwidths.Length; i++)
            {
                availableTokens = Math.Min(availableTokens, GetCurrentSize(i));
            }
            return availableTokens;
        }
        public void Consume(Bandwidth[] bandwidths, long toConsume)
        {
            for (int i = 0; i < bandwidths.Length; i++)
            {
                Consume(i, toConsume);
            }
        }

        public long CalculateDelayNanosAfterWillBePossibleToConsume(Bandwidth[] bandwidths, long tokensToConsume, long currentTimeNanos)
        {
            long delayAfterWillBePossibleToConsume = CalculateDelayNanosAfterWillBePossibleToConsume(0, bandwidths[0], tokensToConsume, currentTimeNanos);
            for (int i = 1; i < bandwidths.Length; i++)
            {
                Bandwidth bandwidth = bandwidths[i];
                long delay = CalculateDelayNanosAfterWillBePossibleToConsume(i, bandwidth, tokensToConsume, currentTimeNanos);
                delayAfterWillBePossibleToConsume = Math.Max(delayAfterWillBePossibleToConsume, delay);
                if (delay > delayAfterWillBePossibleToConsume)
                {
                    delayAfterWillBePossibleToConsume = delay;
                }
            }
            return delayAfterWillBePossibleToConsume;
        }
        public void RefillAllBandwidth(Bandwidth[] limits, long currentTimeNanos)
        {
            for (int i = 0; i < limits.Length; i++)
            {
                Refill(i, limits[i], currentTimeNanos);
            }
        }
        public void AddTokens(Bandwidth[] limits, long tokensToAdd)
        {
            for (int i = 0; i < limits.Length; i++)
            {
                AddTokens(i, limits[i], tokensToAdd);
            }
        }
        public void ForceAddTokens(Bandwidth[] limits, long tokensToAdd)
        {
            for (int i = 0; i < limits.Length; i++)
            {
                ForceAddTokens(i, limits[i], tokensToAdd);
            }
        }
        private long CalculateLastRefillTimeNanos(Bandwidth bandwidth, long currentTimeNanos)
        {
            if (!bandwidth.IsIntervallyAligned)
            {
                return currentTimeNanos;
            }
            return bandwidth.timeOfFirstRefillMillis * 1_000_000 - bandwidth.refillPeriodNanos;
        }
        private long CalculateInitialTokens(Bandwidth bandwidth, long currentTimeNanos)
        {
            if (!bandwidth.UseAdaptiveInitialTokens)
            {
                return bandwidth.InitialTokens;
            }

            long timeOfFirstRefillNanos = bandwidth.TimeOfFirstRefillMillis * 1_000_000;
            if (currentTimeNanos >= timeOfFirstRefillNanos)
            {
                return bandwidth.InitialTokens;
            }

            long guaranteedBase = Math.Max(0, bandwidth.capacity - bandwidth.refillTokens);
            long nanosBeforeFirstRefill = timeOfFirstRefillNanos - currentTimeNanos;
            if (MultiplyExactOrReturnMaxValue(nanosBeforeFirstRefill, bandwidth.refillTokens) != long.MaxValue)
            {
                return Math.Min(bandwidth.capacity, guaranteedBase + nanosBeforeFirstRefill * bandwidth.refillTokens / bandwidth.refillPeriodNanos);
            }
            else
            {
                // arithmetic overflow happens.
                // there is no sense to stay in integer arithmetic when having deal with so big numbers
                return Math.Min(bandwidth.capacity, guaranteedBase + (long)((double)nanosBeforeFirstRefill * (double)bandwidth.refillTokens / (double)bandwidth.refillPeriodNanos));
            }
        }
        public long CalculateFullRefillingTime(Bandwidth[] bandwidths, long currentTimeNanos)
        {
            long maxTimeToFullRefillNanos = CalculateFullRefillingTime(0, bandwidths[0], currentTimeNanos);
            for (int i = 1; i < bandwidths.Length; i++)
            {
                maxTimeToFullRefillNanos = Math.Max(maxTimeToFullRefillNanos, CalculateFullRefillingTime(i, bandwidths[i], currentTimeNanos));
            }
            return maxTimeToFullRefillNanos;
        }
        private long CalculateFullRefillingTime(int bandwidthIndex, Bandwidth bandwidth, long currentTimeNanos)
        {
            long availableTokens = GetCurrentSize(bandwidthIndex);
            if (availableTokens >= bandwidth.capacity)
            {
                return 0L;
            }
            long deficit = bandwidth.capacity - availableTokens;

            if (bandwidth.RefillIntervally)
            {
                return CalculateDelayNanosAfterWillBePossibleToConsumeForIntervalBandwidth(bandwidthIndex, bandwidth, deficit, currentTimeNanos);
            }
            else
            {
                return CalculateDelayNanosAfterWillBePossibleToConsumeForGreedyBandwidth(bandwidthIndex, bandwidth, deficit);
            }
        }
        private void AddTokens(int bandwidthIndex, Bandwidth bandwidth, long tokensToAdd)
        {
            long currentSize = GetCurrentSize(bandwidthIndex);
            long newSize = currentSize + tokensToAdd;
            if (newSize >= bandwidth.Capacity)
            {
                ResetBandwidth(bandwidthIndex, bandwidth.Capacity);
            }
            else if (newSize < currentSize)
            {
                // arithmetic overflow happens. This mean that bucket reached Long.MAX_VALUE tokens.
                // just reset bandwidth state
                ResetBandwidth(bandwidthIndex, bandwidth.Capacity);
            }
            else
            {
                SetCurrentSize(bandwidthIndex, newSize);
            }
        }
        private void ForceAddTokens(int bandwidthIndex, Bandwidth bandwidth, long tokensToAdd)
        {
            long currentSize = GetCurrentSize(bandwidthIndex);
            long newSize = currentSize + tokensToAdd;
            if (newSize < currentSize)
            {
                // arithmetic overflow happens. This mean that bucket reached Long.MAX_VALUE tokens.
                // just set MAX_VALUE tokens
                SetCurrentSize(bandwidthIndex, long.MaxValue);
                SetRoundingError(bandwidthIndex, 0);
            }
            else
            {
                SetCurrentSize(bandwidthIndex, newSize);
            }
        }
        private void Refill(int bandwidthIndex, Bandwidth bandwidth, long currentTimeNanos)
        {
            long previousRefillNanos = GetLastRefillTimeNanos(bandwidthIndex);
            if (currentTimeNanos <= previousRefillNanos)
            {
                return;
            }

            if (bandwidth.RefillIntervally)
            {
                long incompleteIntervalCorrection = (currentTimeNanos - previousRefillNanos) % bandwidth.RefillPeriodNanos;
                currentTimeNanos -= incompleteIntervalCorrection;
            }
            if (currentTimeNanos <= previousRefillNanos)
            {
                return;
            }
            else
            {
                SetLastRefillTimeNanos(bandwidthIndex, currentTimeNanos);
            }

            long capacity = bandwidth.Capacity;
            long refillPeriodNanos = bandwidth.RefillPeriodNanos;
            long refillTokens = bandwidth.RefillTokens;
            long currentSize = GetCurrentSize(bandwidthIndex);

            if (currentSize >= capacity)
            {
                // can come here if forceAddTokens has been used
                return;
            }

            long durationSinceLastRefillNanos = currentTimeNanos - previousRefillNanos;
            long newSize = currentSize;

            if (durationSinceLastRefillNanos > refillPeriodNanos)
            {
                long elapsedPeriods = durationSinceLastRefillNanos / refillPeriodNanos;
                long calculatedRefill = elapsedPeriods * refillTokens;
                newSize += calculatedRefill;
                if (newSize > capacity)
                {
                    ResetBandwidth(bandwidthIndex, capacity);
                    return;
                }
                if (newSize < currentSize)
                {
                    // arithmetic overflow happens. This mean that tokens reached Long.MAX_VALUE tokens.
                    // just reset bandwidth state
                    ResetBandwidth(bandwidthIndex, capacity);
                    return;
                }
                durationSinceLastRefillNanos %= refillPeriodNanos;
            }

            long roundingError = GetRoundingError(bandwidthIndex);
            long dividedWithoutError = MultiplyExactOrReturnMaxValue(refillTokens, durationSinceLastRefillNanos);
            long divided = dividedWithoutError + roundingError;
            if (divided < 0 || dividedWithoutError == long.MaxValue)
            {
                // arithmetic overflow happens.
                // there is no sense to stay in integer arithmetic when having deal with so big numbers
                long calculatedRefill = (long)((double)durationSinceLastRefillNanos / (double)refillPeriodNanos * (double)refillTokens);
                newSize += calculatedRefill;
                roundingError = 0;
            }
            else
            {
                long calculatedRefill = divided / refillPeriodNanos;
                if (calculatedRefill == 0)
                {
                    roundingError = divided;
                }
                else
                {
                    newSize += calculatedRefill;
                    roundingError = divided % refillPeriodNanos;
                }
            }

            if (newSize >= capacity)
            {
                ResetBandwidth(bandwidthIndex, capacity);
                return;
            }
            if (newSize < currentSize)
            {
                // arithmetic overflow happens. This mean that bucket reached Long.MAX_VALUE tokens.
                // just reset bandwidth state
                ResetBandwidth(bandwidthIndex, capacity);
                return;
            }
            SetCurrentSize(bandwidthIndex, newSize);
            SetRoundingError(bandwidthIndex, roundingError);
        }
        private void ResetBandwidth(int bandwidthIndex, long capacity)
        {
            SetCurrentSize(bandwidthIndex, capacity);
            SetRoundingError(bandwidthIndex, 0);
        }
        private long CalculateDelayNanosAfterWillBePossibleToConsume(int bandwidthIndex, Bandwidth bandwidth, long tokens, long currentTimeNanos)
        {
            long currentSize = GetCurrentSize(bandwidthIndex);
            if (tokens <= currentSize)
            {
                return 0;
            }
            long deficit = tokens - currentSize;
            if (deficit <= 0)
            {
                // math overflow happen
                return long.MaxValue;
            }

            if (bandwidth.RefillIntervally)
            {
                return CalculateDelayNanosAfterWillBePossibleToConsumeForIntervalBandwidth(bandwidthIndex, bandwidth, deficit, currentTimeNanos);
            }
            else
            {
                return CalculateDelayNanosAfterWillBePossibleToConsumeForGreedyBandwidth(bandwidthIndex, bandwidth, deficit);
            }
        }
        private long CalculateDelayNanosAfterWillBePossibleToConsumeForGreedyBandwidth(int bandwidthIndex, Bandwidth bandwidth, long deficit)
        {
            long refillPeriodNanos = bandwidth.RefillPeriodNanos;
            long refillPeriodTokens = bandwidth.RefillTokens;
            long divided = MultiplyExactOrReturnMaxValue(refillPeriodNanos, deficit);
            if (divided == long.MaxValue)
            {
                // math overflow happen.
                // there is no sense to stay in integer arithmetic when having deal with so big numbers
                return (long)((double)deficit / (double)refillPeriodTokens * (double)refillPeriodNanos);
            }
            else
            {
                long correctionForPartiallyRefilledToken = GetRoundingError(bandwidthIndex);
                divided -= correctionForPartiallyRefilledToken;
                return divided / refillPeriodTokens;
            }
        }
        private long CalculateDelayNanosAfterWillBePossibleToConsumeForIntervalBandwidth(int bandwidthIndex, Bandwidth bandwidth, long deficit, long currentTimeNanos)
        {
            long refillPeriodNanos = bandwidth.RefillPeriodNanos;
            long refillTokens = bandwidth.RefillTokens;
            long previousRefillNanos = GetLastRefillTimeNanos(bandwidthIndex);

            long timeOfNextRefillNanos = previousRefillNanos + refillPeriodNanos;
            long waitForNextRefillNanos = timeOfNextRefillNanos - currentTimeNanos;
            if (deficit <= refillTokens)
            {
                return waitForNextRefillNanos;
            }

            deficit -= refillTokens;
            if (deficit < refillTokens)
            {
                return waitForNextRefillNanos + refillPeriodNanos;
            }

            long deficitPeriods = deficit / refillTokens + (deficit % refillTokens == 0L ? 0 : 1);
            long deficitNanos = MultiplyExactOrReturnMaxValue(deficitPeriods, refillPeriodNanos);
            if (deficitNanos == long.MaxValue)
            {
                // math overflow happen
                return long.MaxValue;
            }
            deficitNanos += waitForNextRefillNanos;
            if (deficitNanos < 0)
            {
                // math overflow happen
                return long.MaxValue;
            }
            return deficitNanos;
        }
        private long GetLastRefillTimeNanos(int bandwidth)
        {
            return stateData[bandwidth * BANDWIDTH_SIZE];
        }

        private void SetLastRefillTimeNanos(int bandwidth, long nanos)
        {
            stateData[bandwidth * BANDWIDTH_SIZE] = nanos;
        }
        public long GetCurrentSize(int bandwidth)
        {
            return stateData[bandwidth * BANDWIDTH_SIZE + 1];
        }
        public long GetRoundingError(int bandwidth)
        {
            return stateData[bandwidth * BANDWIDTH_SIZE + 2];
        }
        public MathType MathType => MathType.Integer64Bits;
        private void SetCurrentSize(int bandwidth, long currentSize)
        {
            stateData[bandwidth * BANDWIDTH_SIZE + 1] = currentSize;
        }
        private void Consume(int bandwidth, long tokens)
        {
            stateData[bandwidth * BANDWIDTH_SIZE + 1] -= tokens;
        }
        private void SetRoundingError(int bandwidth, long roundingError)
        {
            stateData[bandwidth * BANDWIDTH_SIZE + 2] = roundingError;
        }
        public override string ToString()
        {
            return "BucketState{" +
                    "bandwidthStates=" + string.Join(", ", stateData.Select(s => s.ToString())) +
                    '}';
        }
        // just a copy of JDK method Math#multiplyExact,
        // but instead of throwing exception it returns Long.MAX_VALUE in case of overflow
        private static long MultiplyExactOrReturnMaxValue(long x, long y)
        {
            long r = x * y;
            ulong ax = (ulong)Math.Abs(x);
            ulong ay = (ulong)Math.Abs(y);
            if (((ax | ay) >> 31 != 0))
            {
                // Some bits greater than 2^31 that might cause overflow
                // Check the result using the divide operator
                // and check for the special case of Long.MIN_VALUE * -1
                if (((y != 0) && (r / y != x)) || (x == long.MaxValue && y == -1))
                {
                    return long.MaxValue;
                }
            }
            return r;
        }
        public bool EqualsByContent(BucketState64BitsInteger other)
        {
            return Enumerable.SequenceEqual(stateData, other.stateData);
        }

    }
}