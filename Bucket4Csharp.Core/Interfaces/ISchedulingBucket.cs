using Bucket4Csharp.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Interfaces
{
    /// <summary>
    /// Provides the scheduling API for <see cref="IBucket"/>
    /// </summary>
    public interface ISchedulingBucket
    {
        /// <summary>
        /// In contrast with the Java library, our scheduling bucket uses milliseconds instead of nanoseconds for time periods.
        /// This is because the async state machines in c# will be limited to milliseconds accuracies because of they way they are built.
        /// 
        /// 
        /// </summary>
        /// <param name="numTokens"></param>
        /// <param name="maxWaitMillis"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> TryConsumeAsync(long numTokens, long maxWaitMillis, CancellationToken cancellationToken);
        /// <summary>
        /// Overload equivalent of <see cref="TryConsumeAsync(long, long, CancellationToken)"/>
        /// </summary>
        /// <param name="numTokens">The number of tokens to consume from the bucket.</param>
        /// <param name="maxWait">Limit of time which thread can wait.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> TryConsumeAsync(long numTokens, TimeSpan maxWait, CancellationToken cancellationToken)
        {
            return TryConsumeAsync(numTokens, (long)maxWait.TotalMilliseconds, cancellationToken);
        }
        /// <summary>
        /// Consumes the specified number of tokens from the bucket.
        /// </summary>
        /// <param name="numTokens"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task ConsumeAsync(long numTokens, CancellationToken cancellationToken);
    }
}
