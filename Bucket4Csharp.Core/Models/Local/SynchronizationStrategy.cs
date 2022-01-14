using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models.Local
{
    public enum SynchronizationStrategy
    {
        /// <summary>
        /// Lock-free algorithm based on CAS(compare and swap) of immutable objects.
        /// <para><b>Advantages:</b> This strategy is tolerant to high contention usage scenario, threads do not block each other.</para>
        /// <br/><b>Disadvantages:</b> The sequence "read-clone-update-save" needs to allocate one object per each invocation of consumption method.
        /// <br/><b>Usage recommendations:</b> when you are not sure what kind of strategy is better for you.
        /// </summary>
        LockFree,
        /// <summary>
        /// Blocking strategy based on
        /// <para>Advantages: Never allocates memory.</para>
        /// <para>Disadvantages: Thread which acquired the lock(and superseded from CPU by OS scheduler) can block another threads for significant time.</para>
        /// <para>Usage recommendations: when your primary goal is avoiding of memory allocation and you do not care about contention.</para>
        /// </summary>
        Synchronized,
        /// <summary>
        /// This is fake strategy which does not perform synchronization at all.
        /// It is usable when there are no multithreading access to same bucket,
        /// or synchronization already provided by third-party library or yourself.
        /// <para>Advantages: Never allocates memory and never acquires any locks, in other words you pay nothing for synchronization.</para>
        /// <para>Disadvantages: If your code or third-party library code has errors then bucket state could become corrupted.</para>
        /// <para>Usage recommendations: <b>IF AND ONLY IF</b> you have guarantees that bucket will be never used from multiple threads,
        /// for example in cases where your third-party library prevents concurrent access and provide guarantees of visibility,
        /// or when you are so senior that can manage all synchronization by yourself.
        /// </para>
        /// </summary>
        None
    }
}
