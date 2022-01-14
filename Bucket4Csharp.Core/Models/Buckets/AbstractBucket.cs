using Bucket4Csharp.Core.Exceptions;
using Bucket4Csharp.Core.Extensions;
using Bucket4Csharp.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models
{
    public abstract class AbstractBucket : IBucket, IBucketListener //TODO: implement BlockingBucket, and SchedulingBucket.
    {
        protected static long INFINITY_DURATION = long.MaxValue;
        protected static long UNLIMITED_AMOUNT = long.MaxValue;
        protected ILogger<AbstractBucket> logger;

        public abstract long AvailableTokens { get; }

        public event EventHandler<OnTokensEventArgs>? Consumed;
        public event EventHandler<OnTokensEventArgs>? Rejected;
        public event EventHandler<OnWaitEventArgs>? Parked;
        public event EventHandler<OnInterruptedEventArgs>? Interrupted;
        public event EventHandler<OnWaitEventArgs>? Delayed;

        protected abstract long ConsumeAsMuchAsPossibleImpl(long limit);

        protected abstract bool TryConsumeImpl(long tokensToConsume);

        protected abstract ConsumptionProbe TryConsumeAndReturnRemainingTokensImpl(long tokensToConsume);

        protected abstract EstimationProbe EstimateAbilityToConsumeImpl(long numTokens);

        protected abstract long ReserveAndCalculateTimeToSleepImpl(long tokensToConsume, long waitIfBusyNanos);

        protected abstract void AddTokensImpl(long tokensToAdd);

        protected abstract void ForceAddTokensImpl(long tokensToAdd);

        protected abstract void ReplaceConfigurationImpl(BucketConfiguration newConfiguration, TokensInheritanceStrategy tokensInheritanceStrategy);

        protected abstract long ConsumeIgnoringRateLimitsImpl(long tokensToConsume);

        //protected abstract VerboseResult<Long> ConsumeAsMuchAsPossibleVerboseImpl(long limit);

        //protected abstract VerboseResult<bool> TryConsumeVerboseImpl(long tokensToConsume);

        //protected abstract VerboseResult<ConsumptionProbe> TryConsumeAndReturnRemainingTokensVerboseImpl(long tokensToConsume);

        //protected abstract VerboseResult<EstimationProbe> EstimateAbilityToConsumeVerboseImpl(long numTokens);

        //protected abstract VerboseResult<long> AvailableTokensVerboseImpl { get; }

        //protected abstract VerboseResult<null> AddTokensVerboseImpl(long tokensToAdd);

        //protected abstract VerboseResult<Nothing> ForceAddTokensVerboseImpl(long tokensToAdd);

        //protected abstract VerboseResult<Nothing> ReplaceConfigurationVerboseImpl(BucketConfiguration newConfiguration, TokensInheritanceStrategy tokensInheritanceStrategy);

        //protected abstract VerboseResult<Long> ConsumeIgnoringRateLimitsVerboseImpl(long tokensToConsume);

        // replaced all of this with events.
        //public AbstractBucket(IBucketListener listener)
        //{
        //    if (listener == null)
        //    {
        //        throw BucketExceptions.NullListener();
        //    }
        //    this.listener = listener;
        //}
        //private readonly IBucketListener listener;
        //protected IBucketListener Listener => listener;

        //Verbose buckets not ported yet. Skipping.
        // https://github.com/vladimir-bukhtoyarov/bucket4j/blob/85a0148788223bc968fe4faa72f733b68dbf129f/bucket4j-core/src/main/java/io/github/bucket4j/AbstractBucket.java#L176
        // this will NOT be ported. Use the C# "as" keyword.



        public void AddTokens(long tokensToAdd)
        {
            tokensToAdd.CheckTokensToAdd();
            AddTokensImpl(tokensToAdd);
        }

        public bool TryConsume(long tokensToConsume)
        {
            tokensToConsume.CheckTokensToConsume();

            if (TryConsumeImpl(tokensToConsume))
            {
                OnConsumed(new OnTokensEventArgs(tokensToConsume));
                return true;
            }
            else
            {
                OnRejected(new OnTokensEventArgs(tokensToConsume));
                return false;
            }
        }

        public long ConsumeIgnoringRateLimits(long numTokens)
        {
            throw new NotImplementedException();
        }

        public EstimationProbe EstimateAbilityToConsume(long numTokens)
        {
            throw new NotImplementedException();
        }

        public void ForceAddTokens(long tokensToAdd)
        {
            throw new NotImplementedException();
        }

        public void ReplaceConfiguration(BucketConfiguration newConfiguration, TokensInheritanceStrategy tokensInheritanceStrategy)
        {
            throw new NotImplementedException();
        }

        public IBucket ToListenable(IBucketListener listener)
        {
            throw new NotImplementedException();
        }

        

        public ConsumptionProbe TryConsumeAndReturnRemaining(long numTokens)
        {
            throw new NotImplementedException();
        }

        public long TryConsumeAsMuchAsPossible()
        {
            throw new NotImplementedException();
        }

        public long TryConsumeAsMuchAsPossible(long limit)
        {
            throw new NotImplementedException();
        }
        
        protected virtual void OnConsumed(OnTokensEventArgs e)
        {
            
            Consumed?.Invoke(this, e);
        }
        protected virtual void OnRejected(OnTokensEventArgs e)
        {
            Rejected?.Invoke(this, e);
        }
        protected virtual void OnParked(OnWaitEventArgs e)
        {
            Parked?.Invoke(this, e);
        }
        protected virtual void OnInterrupted(OnInterruptedEventArgs e)
        {
            Interrupted?.Invoke(this, e);
        }
        protected virtual void OnDelayed(OnWaitEventArgs e)
        {
            Delayed?.Invoke(this, e);
        }
    }
}
