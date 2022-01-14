using Bucket4Csharp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Interfaces
{
    /// <summary>
    /// Performs rate limiting using algorithm based on top of ideas of <a href="https://en.wikipedia.org/wiki/Token_bucket">Token Bucket</a>.
    /// </summary>
    public interface IBucket
    {
        /// <summary>
        /// Tries to consume a specified number of tokens from this bucket.
        /// </summary>
        /// <param name="numTokens">The number of tokens to consume from the bucket, must be a positive number.</param>
        /// <returns><c>true</c> if the tokens were consumed, <c>false</c> otherwise.</returns>
        bool TryConsume(long numTokens);
        /// <summary>
        /// Consumes tokens from bucket ignoring all limits.
        /// In result of this operation amount of tokens in the bucket could became negative.
        /// There are two possible reasons to use this method:
        /// <list>
        /// <item>An operation with high priority should be executed independently of rate limits, but it should take effect to subsequent operation with bucket</item>
        /// <item>You want to apply custom blocking strategy instead of default which applied on <c>AsScheduler().Consume(tokens)</c></item>
        /// </list>
        /// </summary>
        /// <param name="numTokens">tokens amount of tokens that should be consumed from bucket.</param>
        /// <returns>the amount of rate limit violation in nanoseconds calculated in following way:
        /// zero if rate limit was not violated. For example bucket had 5 tokens before invocation of {@code consumeIgnoringRateLimits(2)},
        /// after invocation there are 3 tokens remain in the bucket, since limits were not violated zero  returned as result.
        ///  Positive value which describes the amount of rate limit violation in nanoseconds.
        /// For example bucket with limit 10 tokens per 1 second, currently has the 2 tokens available, last refill happen 100 milliseconds ago, and { @code consumeIgnoringRateLimits(6)}
        /// called.
        /// 300_000_000 will be returned as result and available tokens in the bucket will became 3, and any variation of { @code tryConsume...}
        /// will not be successful for 400 milliseconds(time required to refill amount of available tokens until 1).
        /// 
        /// </returns>
        long ConsumeIgnoringRateLimits(long numTokens);
        /// <summary>
        /// Tries to consume a specified number of tokens from this bucket.
        /// </summary>
        /// <param name="numTokens">The number of tokens to consume from the bucket, must be a positive number.</param>
        /// <returns><see cref="ConsumptionProbe"/> which describes both result of consumption and tokens remaining in the bucket after consumption.</returns>
        ComsuptionProbe TryConsumeAndReturnRemaining(long numTokens);
        /// <summary>
        /// Estimates ability to consume a specified number of tokens.
        /// </summary>
        /// <param name="numTokens">Estimates ability to consume a specified number of tokens.</param>
        /// <returns><see cref="EstimationProbe"/> which describes the ability to consume.</returns>
        EstimationProbe EstimateAbilityToConsume(long numTokens);
        /// <summary>
        /// Tries to consume as much tokens from this bucket as available at the moment of invocation.
        /// </summary>
        /// <returns>number of tokens which has been consumed, or zero if was consumed nothing.</returns>
        long TryConsumeAsMuchAsPossible();
        /// <summary>
        /// Tries to consume as much tokens from bucket as available in the bucket at the moment of invocation,
        /// but tokens which should be consumed is limited by limit.
        /// </summary>
        /// <param name="limit"> Maximum number of tokens to consume, should be positive.</param>
        /// <returns>Number of tokens which has been consumed, or zero if was consumed nothing.</returns>
        long TryConsumeAsMuchAsPossible(long limit);
        /// <summary>
        /// Add <tt>tokensToAdd</tt> to bucket.
        /// Resulted count of tokens are calculated by following formula:
        /// <pre>newTokens = Math.min(capacity, currentTokens + tokensToAdd)</pre>
        /// in other words resulted number of tokens never exceeds capacity independent of <tt>tokensToAdd</tt>.
        /// <para>
        /// Example of usage
        /// </para>
        /// <para>
        /// The "compensating transaction" is one of obvious use case, when any piece of code consumed tokens from bucket, tried to do something and failed, the "addTokens" will be helpful to return tokens back to bucket:
        /// <code>
        /// Bucket wallet;
        /// if(wallet.tryConsume(50)){// get 50 cents from wallet
        /// try {
        ///           buyCocaCola();
        ///       } catch(NoCocaColaException e) {
        ///           // return money to wallet
        /// wallet.addTokens(50);
        ///
        ///}
        ///      };
        ///      
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="tokensToAdd"></param>
        void AddTokens(long tokensToAdd);
        /// <summary>
        /// <para>Add tokensToAdd to bucket. In opposite to {@link #addTokens(long)} usage of this method can lead to overflow bucket capacity.</para>
        /// <para>Example of usage</para>
        /// <para>
        /// The "compensating transaction" is one of obvious use case, when any piece of code consumed tokens from bucket, tried to do something and failed, the "addTokens" will be helpful to return tokens back to bucket:
        /// <code>
        /// Bucket wallet;
        ///
        /// if(wallet.tryConsume(50)
        /// {
        ///     try
        ///     {
        ///         buyCocaCola();
        ///     } catch(NoCocaColaException e){
        ///         
        ///           wallet.addTokens(50);// return money to wallet
        ///     }
        /// }
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="tokensToAdd">number of tokens to add</param>
        void ForceAddTokens(long tokensToAdd);
        void ReplaceConfiguration(BucketConfiguration newConfiguration, TokensInheritanceStrategy tokensInheritanceStrategy);
        /// <summary>
        /// Returns new copy of this bucket instance decorated by {@code listener}.
        /// The created bucket will share same tokens with source bucket and vice versa.
        /// See <see cref="BucketListener"/> in order to understand semantic of listener.
        /// </summary>
        /// <param name="listener"> listener the listener of bucket events.</param>
        /// <returns>new bucket instance decorated by listener</returns>
        IBucket ToListenable(IBucketListener listener);
    }
}
