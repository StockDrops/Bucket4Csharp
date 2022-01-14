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
        Task<bool> TryConsumeAsync(long numTokens, long maxWaitNanos, CancellationToken cancellationToken);
        /// <summary>
        /// Overload equivalent of <see cref="TryConsumeAsync(long, long, CancellationToken)"/>
        /// </summary>
        /// <param name="numTokens">The number of tokens to consume from the bucket.</param>
        /// <param name="maxWait">Limit of time which thread can wait.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> TryConsumeAsync(long numTokens, TimeSpan maxWait, CancellationToken cancellationToken)
        {
            return TryConsumeAsync(numTokens, maxWait.Nanoseconds(), cancellationToken);
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
