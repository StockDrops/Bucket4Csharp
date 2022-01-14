using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models
{
    /// <summary>
    /// Specifies the rules for inheritance of available tokens when <see cref="Bucket4Csharp.Core.Interfaces.IBucket.ReplaceConfiguration(BucketConfiguration, TokensInheritanceStrategy)"/> happens.
    /// </summary>
    public enum TokensInheritanceStrategy
    {
        /// <summary>
        /// Makes to copy available tokens proportional to bandwidth capacity by following formula:
        /// <code>
        /// newAvailableTokens = availableTokensBeforeReplacement(newBandwidthCapacity / capacityBeforeReplacement)
        /// </code>
        ///<para>
        ///Let's describe few examples.
        ///</para>
        ///<para> <b>Example 1:</b> imagine bandwidth that was created by <c> Bandwidth.classic(100, Refill.gready(10, Duration.ofMinutes(1)))</c>.
        ///At the moment of config replacement it was 40 available tokens.After replacing this bandwidth by following <c> Bandwidth.Classic(200, Refill.Greedy(10, TimeSpan.FromMinutes(1)))</c>
        ///40 available tokens will be multiplied by 2(200/100), and after replacement we will have 80 available tokens.
        ///</para>
        ///<para> <b>Example 2:</b> imagine bandwidth that was created by <c> Bandwidth.classic(100, Refill.gready(10, Duration.ofMinutes(1)))}</c>.
        ///At the moment of config replacement it was 40 available tokens.After replacing this bandwidth by following <c> Bandwidth.Classic(20, Refill.Greedy(10, TimeSpan.FromMinutes(1)))</c>
        ///40 available tokens will be multiplied by 0.2(20/100), and after replacement we will have 8 available tokens.
        ///</para>
        /// </summary>
        Proportionally = 0,
        /// <summary>
        /// Instructs to copy available tokens as is, but with one exclusion: if available tokens is greater than new capacity,
        /// available tokens will be decreased to new capacity.
        /// <para>
        /// Let's describe few examples.
        /// </para>
        /// <para>
        /// <b>Example 1:</b> imagine bandwidth that was created by <c>Bandwidth.classic(100, Refill.gready(10, Duration.ofMinutes(1)))</c>.
        /// At the moment of config replacement it was 40 available tokens. After replacing this bandwidth by following <c>Bandwidth.Classic(200, Refill.Greedy(10, TimeSpan.FromMinutes(1)))</c>
        /// 40 available tokens will be just copied, and after replacement we will have 40 available tokens.
        /// </para>
        /// <para>
        /// <b>Example 2:</b> imagine bandwidth that was created by <c>Bandwidth.classic(100, Refill.gready(10, Duration.ofMinutes(1)))</c>.
        ///  At the moment of config replacement it was 40 available tokens. After replacing this bandwidth by following <c>Bandwidth.Classic(20, Refill.Greedy(10, TimeSpan.FromMinutes(1)))</c>
        ///  40 available tokens can not be copied as is, because it is greater then new capacity, so available tokens will be reduced to 20.
        /// </para>
        /// </summary>
        AsIs = 1,
        /// <summary>
        /// Use this mode when you just want to forget about the previous bucket state.
        /// <c>IBucket.ReplaceConfiguration(newConfiguration, TokensInheritanceStrategy.Reset)</c> just erases the previous state.
        /// Using this strategy equals to destroying the bucket and creating it again with the new configuration.
        /// </summary>
        Reset = 2,
        /// <summary>
        /// Instructs to copy available tokens as is, but with one exclusion: if new bandwidth capacity is greater than old capacity, available tokens will be increased by the difference between the old and the new configuration.
        /// <br/>
        /// The formula is <code>newAvailableTokens = Math.min(availableTokensBeforeReplacement, newBandwidthCapacity) + Math.max(0, newBandwidthCapacity - capacityBeforeReplacement)</code>
        /// 
        /// <para>Let's describe a few examples:</para>
        /// <para>
        ///     <b>Example 1:</b> imagine a bandwidth object that was created by <c>Bandwidth.Classic(100, Refill.Greedy(10, TimeSpan.FromMinutes(1)))</c>.
        ///     At the moment of configuration replacement, there were 40 available tokens.
        ///     After replacing this bandwidth using the following <c>Bandwidth.Classic(200, Refill.Greedy(10, TimeSpan.FromMinutes(1)))</c>, the 40 available tokens will be copied and added to the difference between the old and new configuration,
        ///     and after replacement, there will be 140 available tokens.
        /// </para>
        /// <para>
        ///     <b>Example 2:</b> imagine bandwidth that was created by <c>Bandwidth.Classic(100, Refill.Greedy(10, TimeSpan.FromMinutes(1)))</c>.
        ///     At the moment of config replacement there were 40 available tokens.
        ///     After replacing this bandwidth using the following <c>Bandwidth.Classic(20, Refill.Greedy(10, TimeSpan.FromMinutes(1))))</c>,
        ///     after replacement, there will be 20 available tokens.
        /// </para>
        /// <para>
        ///     <b>Example 3:</b> imagine a bandwidth object that was created by <c>Bandwidth.Classic(100, Refill.Greedy(10, TimeSpan.FromMinutes(1)))</c>
        ///     At the moment of config replacement there were 10 available tokens.
        ///     After replacing this bandwidth using the following code <c>Bandwidth.Classic(20, Refill.Greedy(10, TimeSpan.FromMinutes(1))))</c>,
        ///     after replacement, there will be 10 available tokens.   
        /// </para>
        /// </summary>
        Additive = 3,
    }
}
