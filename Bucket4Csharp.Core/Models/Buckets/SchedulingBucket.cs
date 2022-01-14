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
        public virtual async Task<bool> TryConsumeAsync(long numTokens, long maxWaitMilliseconds, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }
            maxWaitMilliseconds.CheckMaxWaitTime();
            numTokens.CheckTokensToConsume();
            try
            {
                //transform this to milliseconds.
                long milliSecondsToSleep = ReserveAndCalculateTimeToSleepImpl(numTokens, maxWaitMilliseconds, TimeUnit.Milliseconds);
                if (milliSecondsToSleep == INFINITY_DURATION)
                {
                    OnRejected(new OnTokensEventArgs(numTokens));
                    return false;
                }
                if (milliSecondsToSleep == 0L)
                {
                    OnConsumed(new OnTokensEventArgs(numTokens));
                    return true;
                }
                OnConsumed(new OnTokensEventArgs(numTokens));
                OnDelayed(new OnWaitEventArgs(milliSecondsToSleep, TimeUnit.Milliseconds));
                await Task.Delay((int)milliSecondsToSleep, cancellationToken).ConfigureAwait(false);// ns are 10^-9, ms are 10^-3
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "");
                throw new InvalidOperationException("Error occurred while attempting to consume.", ex);
            }
        }

        public virtual async Task ConsumeAsync(long numTokens, CancellationToken cancellationToken)
        {
            numTokens.CheckTokensToConsume();

            try
            {
                long milliSecondsToSleep = ReserveAndCalculateTimeToSleepImpl(numTokens, INFINITY_DURATION, TimeUnit.Milliseconds);
                if (milliSecondsToSleep == INFINITY_DURATION)
                {
                    throw BucketExceptions.ReservationOverflow();
                }
                if (milliSecondsToSleep == 0L)
                {
                    OnConsumed(new OnTokensEventArgs(numTokens));
                    return;
                }
                OnConsumed(new OnTokensEventArgs(numTokens));
                OnDelayed(new OnWaitEventArgs(milliSecondsToSleep, TimeUnit.Milliseconds));
                await Task.Delay((int)milliSecondsToSleep, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "");
                throw new InvalidOperationException("Error occurred while attempting to consume.", ex);
            }
        }
    }
}
