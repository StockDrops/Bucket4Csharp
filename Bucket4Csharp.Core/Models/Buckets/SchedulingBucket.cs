using Bucket4Csharp.Core.Exceptions;
using Bucket4Csharp.Core.Extensions;
using Bucket4Csharp.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models.Buckets
{
    public abstract class SchedulingBucket : AbstractBucket, ISchedulingBucket
    {
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="numTokens"></param>
        /// <param name="maxWaitNanos"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="TaskCanceledException">When the task is cancelled.</exception>
        /// <exception cref="InvalidOperationException">If an internal exception happens.</exception>
        public virtual async Task<bool> TryConsumeAsync(long numTokens, long maxWaitNanos, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
            maxWaitNanos.CheckMaxWaitTime();
            numTokens.CheckTokensToConsume();
            try
            {
                long nanosecondsToSleep = ReserveAndCalculateTimeToSleepImpl(numTokens, maxWaitNanos);
                if (nanosecondsToSleep == INFINITY_DURATION)
                {
                    OnRejected(new OnTokensEventArgs(numTokens));
                    return false;
                }
                if (nanosecondsToSleep == 0L)
                {
                    OnConsumed(new OnTokensEventArgs(numTokens));
                    return true;
                }
                OnConsumed(new OnTokensEventArgs(numTokens));
                OnDelayed(new OnWaitEventArgs(nanosecondsToSleep));
                await Task.Delay((int)(nanosecondsToSleep / 1000000), cancellationToken).ConfigureAwait(false);// ns are 10^-9, ms are 10^-3
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "");
                throw new InvalidOperationException("Error occurred while attempting to consume.", ex);
            }
        }

        public virtual async Task ConsumeAsync(long numTokens, CancellationToken cancellationToken)
        {
            numTokens.CheckTokensToConsume();

            try
            {
                long nanosToSleep = ReserveAndCalculateTimeToSleepImpl(numTokens, INFINITY_DURATION);
                if (nanosToSleep == INFINITY_DURATION)
                {
                    throw BucketExceptions.ReservationOverflow();
                }
                if (nanosToSleep == 0L)
                {
                    OnConsumed(new OnTokensEventArgs(numTokens));
                    return;
                }
                OnConsumed(new OnTokensEventArgs(numTokens));
                OnDelayed(new OnWaitEventArgs(nanosToSleep));
                await Task.Delay((int)(nanosToSleep / 1000000), cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "");
                throw new InvalidOperationException("Error occurred while attempting to consume.", ex);
            }
        }
    }
}
