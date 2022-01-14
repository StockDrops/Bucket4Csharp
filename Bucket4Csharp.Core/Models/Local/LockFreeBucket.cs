using Bucket4Csharp.Core.Extensions;
using Bucket4Csharp.Core.Interfaces;
using Bucket4Csharp.Core.Models.Buckets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models.Local
{
    public abstract class LockFreeBucketContendedTimeMeter : SchedulingBucket, IBucketListener
    {

        protected readonly ITimeMeter timeMeter;
        internal ITimeMeter TimeMeter => timeMeter;

        public LockFreeBucketContendedTimeMeter(ITimeMeter timeMeter)
        {

            this.timeMeter = timeMeter;
        }
    }
    public abstract class LockFreeBucket_FinalFields_CacheLinePadding : LockFreeBucketContendedTimeMeter
    {

        long p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16;

        public LockFreeBucket_FinalFields_CacheLinePadding(ITimeMeter timeMeter) : base(timeMeter) { }
        
    }


    public class LockFreeBucket : LockFreeBucket_FinalFields_CacheLinePadding, ILocalBucket
    {
        StateWithConfiguration currentState;
        StateWithConfiguration previousState;
        public LockFreeBucket(BucketConfiguration configuration, MathType mathType, ITimeMeter timeMeter) : this(CreateStateWithConfiguration(configuration, mathType, timeMeter), timeMeter)
        {
            
        }
        private LockFreeBucket(StateWithConfiguration state, ITimeMeter timeMeter) : base(timeMeter)
        {
           
            this.currentState = state;
            this.previousState = state;
        }
        protected override long ConsumeAsMuchAsPossibleImpl(long limit)
        {
            StateWithConfiguration previousState = null!;
            Interlocked.Exchange(ref previousState, currentState);
            StateWithConfiguration newState = previousState.Copy();
            long currentTimeNanos = timeMeter.Nanoseconds;

            while (true)
            {
                newState.RefillAllBandwidth(currentTimeNanos);
                long availableToConsume = newState.GetAvailableTokens();
                long toConsume = Math.Min(limit, availableToConsume);
                if (toConsume == 0)
                {
                    return 0;
                }
                newState.Consume(toConsume);
                //var oldState = ; //this is value to update, value to update with if equal, comparison value.
                if (Interlocked.CompareExchange(ref currentState, newState, previousState) == previousState) //first one is the expected value, the latter the new state to set.
                {
                    return toConsume;
                }
                else
                {
                    Interlocked.Exchange(ref previousState, currentState);
                    //previousState = stateRef.get();
                    newState.CopyStateFrom(previousState);
                }
            }
        }
        protected override bool TryConsumeImpl(long tokensToConsume)
        {
            StateWithConfiguration previousState = null!;
            Interlocked.Exchange(ref previousState, currentState);
            StateWithConfiguration newState = previousState.Copy();
            long currentTimeNanos = timeMeter.Nanoseconds;

            while (true)
            {
                newState.RefillAllBandwidth(currentTimeNanos);
                long availableToConsume = newState.GetAvailableTokens();
                if (tokensToConsume > availableToConsume)
                {
                    return false;
                }
                newState.Consume(tokensToConsume);
                if (Interlocked.CompareExchange(ref currentState, newState, previousState) == previousState)
                {
                    return true;
                }
                else
                {
                    Interlocked.Exchange(ref previousState, currentState);
                    newState.CopyStateFrom(previousState);
                }
            }
        }
        protected override ConsumptionProbe TryConsumeAndReturnRemainingTokensImpl(long tokensToConsume)
        {
            StateWithConfiguration previousState = null!;
            Interlocked.Exchange(ref previousState, currentState);
            StateWithConfiguration newState = previousState.Copy();
            long currentTimeNanos = timeMeter.Nanoseconds;

            while (true)
            {
                newState.RefillAllBandwidth(currentTimeNanos);
                long availableToConsume = newState.GetAvailableTokens();
                if (tokensToConsume > availableToConsume)
                {
                    long nanosToWaitForRefill = newState.DelayNanosAfterWillBePossibleToConsume(tokensToConsume, currentTimeNanos);
                    long nanosToWaitForReset = newState.CalculateFullRefillingTime(currentTimeNanos);
                    return ConsumptionProbe.Rejected(availableToConsume, nanosToWaitForRefill, nanosToWaitForReset);
                }
                newState.Consume(tokensToConsume);
                if (Interlocked.CompareExchange(ref currentState, newState, previousState) == previousState)
                {
                    long remainingTokens = availableToConsume - tokensToConsume;
                    long nanosToWaitForReset = newState.CalculateFullRefillingTime(currentTimeNanos);
                    return ConsumptionProbe.Consumed(remainingTokens, nanosToWaitForReset);
                }
                else
                {
                    Interlocked.Exchange(ref previousState, currentState);
                    newState.CopyStateFrom(previousState);
                }
            }
        }

        protected override EstimationProbe EstimateAbilityToConsumeImpl(long tokensToEstimate)
        {
            StateWithConfiguration previousState = null!;
            Interlocked.Exchange(ref previousState, currentState);
            StateWithConfiguration newState = previousState.Copy();
            long currentTimeNanos = timeMeter.Nanoseconds;

            newState.RefillAllBandwidth(currentTimeNanos);
            long availableToConsume = newState.GetAvailableTokens();
            if (tokensToEstimate > availableToConsume)
            {
                long nanosToWaitForRefill = newState.DelayNanosAfterWillBePossibleToConsume(tokensToEstimate, currentTimeNanos);
                return EstimationProbe.NotConsumable(availableToConsume, nanosToWaitForRefill);
            }
            else
            {
                return EstimationProbe.Consumable(availableToConsume);
            }
        }
        protected override long ReserveAndCalculateTimeToSleepImpl(
            long tokensToConsume, long waitIfBusyLimit, TimeUnit timeUnitUsed = TimeUnit.Nanoseconds) //let's adapt this to use milliseconds.
        {
            StateWithConfiguration previousState = null!;
            Interlocked.Exchange(ref previousState, currentState);
            StateWithConfiguration newState = previousState.Copy();
            long currentTimeNanos = timeMeter.Nanoseconds;
            //long currentTimeMilliseconds = timeMeter.Milliseconds;
            long waitIfBusyNanosLimit;
            if(waitIfBusyLimit == INFINITY_DURATION)
            {
                waitIfBusyNanosLimit = INFINITY_DURATION;
            }
            else
            {
                //convert.
                switch (timeUnitUsed)
                {
                    case TimeUnit.Nanoseconds:
                        waitIfBusyNanosLimit = waitIfBusyLimit;
                        break;
                    case TimeUnit.Milliseconds:
                        waitIfBusyNanosLimit = 1_000_000L * waitIfBusyLimit;
                        break;
                    case TimeUnit.Microseconds:
                        waitIfBusyNanosLimit = 1000L * waitIfBusyLimit;
                        break;
                    default:
                        throw new InvalidOperationException("Unknown time unit.");
                }
            }
            
            while (true)
            {
                newState.RefillAllBandwidth(currentTimeNanos);
                long nanosToCloseDeficit = newState.DelayNanosAfterWillBePossibleToConsume(tokensToConsume, currentTimeNanos);
                if (nanosToCloseDeficit == 0)
                {
                    newState.Consume(tokensToConsume);
                    if (Interlocked.CompareExchange(ref currentState, newState, previousState) == previousState)
                    {
                        return 0L;
                    }
                    Interlocked.Exchange(ref previousState, currentState);
                    newState.CopyStateFrom(previousState);
                    continue;
                }

                if (nanosToCloseDeficit == long.MaxValue || nanosToCloseDeficit > waitIfBusyNanosLimit)
                {
                    return long.MaxValue;
                }

                newState.Consume(tokensToConsume);
                if (Interlocked.CompareExchange(ref currentState, newState, previousState) == previousState)
                {
                    return nanosToCloseDeficit.ConvertTo(TimeUnit.Nanoseconds, timeUnitUsed);
                }
                Interlocked.Exchange(ref previousState, currentState);
                newState.CopyStateFrom(previousState);
            }
        }
        protected override void AddTokensImpl(long tokensToAdd)
        {
            StateWithConfiguration previousState = null!;
            Interlocked.Exchange(ref previousState, currentState);
            StateWithConfiguration newState = previousState.Copy();
            long currentTimeNanos = timeMeter.Nanoseconds;

            while (true)
            {
                newState.RefillAllBandwidth(currentTimeNanos);
                newState.state.AddTokens(newState.configuration.Bandwidths, tokensToAdd);
                if (Interlocked.CompareExchange(ref currentState, newState, previousState) == previousState)
                {
                    return;
                }
                else
                {
                    Interlocked.Exchange(ref previousState, currentState);
                    newState.CopyStateFrom(previousState);
                }
            }
        }


        protected override void ForceAddTokensImpl(long tokensToAdd)
        {
            StateWithConfiguration previousState = null!;
            Interlocked.Exchange(ref previousState, currentState);
            StateWithConfiguration newState = previousState.Copy();
            long currentTimeNanos = timeMeter.Nanoseconds;

            while (true)
            {
                newState.RefillAllBandwidth(currentTimeNanos);
                newState.state.ForceAddTokens(newState.configuration.Bandwidths, tokensToAdd);
                if (Interlocked.CompareExchange(ref currentState, newState, previousState) == previousState)
                {
                    return;
                }
                else
                {
                    Interlocked.Exchange(ref previousState, currentState);
                    newState.CopyStateFrom(previousState);
                }
            }
        }
        protected override void ReplaceConfigurationImpl(BucketConfiguration newConfiguration, TokensInheritanceStrategy tokensInheritanceStrategy)
        {
            StateWithConfiguration previousState = null!;
            Interlocked.Exchange(ref previousState, currentState);
            StateWithConfiguration newState = previousState.Copy();
            long currentTimeNanos = timeMeter.Nanoseconds;

            while (true)
            {
                newState.RefillAllBandwidth(currentTimeNanos);
                newState.configuration = newConfiguration;
                newState.state = newState.state.ReplaceConfiguration(previousState.configuration, newConfiguration, tokensInheritanceStrategy, currentTimeNanos);
                if (Interlocked.CompareExchange(ref currentState, newState, previousState) == previousState)
                {
                    return;
                }
                else
                {
                    Interlocked.Exchange(ref previousState, currentState);
                    newState.CopyStateFrom(previousState);
                }
            }
        }

        
        protected override long ConsumeIgnoringRateLimitsImpl(long tokensToConsume)
        {
            StateWithConfiguration previousState = null!;
            Interlocked.Exchange(ref previousState, currentState);
            StateWithConfiguration newState = previousState.Copy();
            long currentTimeNanos = timeMeter.Nanoseconds;

            while (true)
            {
                newState.RefillAllBandwidth(currentTimeNanos);
                long nanosToCloseDeficit = newState.DelayNanosAfterWillBePossibleToConsume(tokensToConsume, currentTimeNanos);

                if (nanosToCloseDeficit == INFINITY_DURATION)
                {
                    return nanosToCloseDeficit;
                }
                newState.Consume(tokensToConsume);
                if (Interlocked.CompareExchange(ref currentState, newState, previousState) == previousState)
                {
                    return nanosToCloseDeficit;
                }
                else
                {
                    Interlocked.Exchange(ref previousState, currentState);
                    newState.CopyStateFrom(previousState);
                }
            }
        }
        //skipping https://github.com/vladimir-bukhtoyarov/bucket4j/blob/85a0148788223bc968fe4faa72f733b68dbf129f/bucket4j-core/src/main/java/io/github/bucket4j/local/LockFreeBucket.java#L292
        //from l. 292 to l.467 -> we are not porting the verbose items yet.

        public override long AvailableTokens
        {
            get
            {
                long currentTimeNanos = timeMeter.Nanoseconds;

                StateWithConfiguration c = null!;
                StateWithConfiguration snapshot = Interlocked.Exchange(ref c, currentState).Copy();
                snapshot.RefillAllBandwidth(currentTimeNanos);
                return snapshot.GetAvailableTokens();
            }
            
        }


        public BucketConfiguration Configuration
        {
            get
            {
                StateWithConfiguration c = null!;
                Interlocked.Exchange(ref c, currentState);
                return c.configuration;
            }
        }

        ITimeMeter ILocalBucket.TimeMeter => timeMeter;


        private static StateWithConfiguration CreateStateWithConfiguration(BucketConfiguration configuration, MathType mathType, ITimeMeter timeMeter)
        {
            IBucketState initialState = IBucketState.CreateInitialState(configuration, mathType, timeMeter.Nanoseconds);
            return new StateWithConfiguration(configuration, initialState);
        }
        
        
        public override string ToString()
        {
            StateWithConfiguration local = null!;
            Interlocked.Exchange(ref local, currentState);
            return "LockFreeBucket{" +
                "state=" + local.ToString() +
                ", configuration=" + Configuration +
                '}';
        }

    }
}
