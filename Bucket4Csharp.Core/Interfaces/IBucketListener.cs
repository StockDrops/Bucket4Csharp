using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Interfaces
{
    /// <summary>
    /// Interface for listening to bucket related events. The typical use-cases of this interface are logging and monitoring.
    /// The bucket can be decorated by the listener via
    ///  {
    ///      @link Bucket#toListenable(BucketListener)} method.
    ///<para>Question: How many listeners is need to create in case of application uses many buckets?</para>
    ///<b>Answer:</b>  it depends:
    ///<ul>
    ///    <li>If you want to have aggregated statistics for all buckets then create single listener per application and reuse this listener for all buckets.</li>
    ///    <li>If you want to measure statistics independently per each bucket then use listener per bucket model.</li>
    ///</ul>
    ///<para>Question: where is methods of listener are invoking in case of distributed usage?</para>
    ///<b>Answer:</b> listener always invoked on client side, it is means that each client JVM will have own totally independent  for same bucket.
    ///<para>Question: Why does bucket invoke the listener on client side instead of server side in case of distributed scenario?
    ///What I need to do if I need in aggregated stat across the whole cluster?</para>
    ///<b>Answer:</b> Because of planned expansion to non-JVM back-ends such as Redis, MySQL, PostgreSQL.
    ///It is not possible to serialize and invoke listener on this non-java back-ends, so it was decided to invoke listener on client side,
    ///in order to avoid inconsistency between different back-ends in the future.
    ///You can do post-aggregation of monitoring statistics via features built-into your monitoring database or via mediator(like StatsD) between your application and monitoring database.
    /// <see cref="SimpleBucketListener"/>
    /// </summary>
    public interface IBucketListener //TODO: can we use events here?
    {
        /// <summary>
        /// This method is called whenever {@code tokens} is consumed.
        /// </summary>
        /// <param name="tokens">tokens amount of tokens consumed</param>
        void OnConsumed(long tokens);
        /// <summary>
        /// This method is called whenever consumption request for {@code tokens} is rejected.
        /// </summary>
        /// <param name="tokens">amount of tokens rejected</param>
        void OnRejected(long tokens);
        /// <summary>
        /// This method is called each time when thread was parked for wait of tokens refill
        /// in result of interaction with {@link BlockingBucket}
        /// </summary>
        /// <param name="nanos">nanos amount of nanoseconds for which thread was parked</param>
        void OnParked(long nanos);
        ///<summary>
        ///This method is called each time when thread was interrupted during the wait of tokens refill
        ///in result of interaction with <see cref="BlockingBucket"/>
        ///<paramref name="e">exception</paramref>
        ///</summary>    
        void OnInterrupted(ThreadInterruptedException e);
        ///<summary>
        ///This method is called each time when delayed task was submit to {@link java.util.concurrent.ScheduledExecutorService}
        ///because of wait for tokens refill in result of interaction with { @link SchedulingBucket }
        ///<paramref name="nanos"/>
        ///</summary>
        void OnDelayed(long nanos);
    }
       
}

