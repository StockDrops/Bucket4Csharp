using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Exceptions
{
    /// <summary>
    /// Extension methods for accessing Microseconds and Nanoseconds of a
    /// DateTime object.
    /// https://stackoverflow.com/questions/5358860/is-there-a-high-resolution-microsecond-nanosecond-datetime-object-available-f
    /// </summary>
    public static class TimeSpanExtensionMethods
    {
        /// <summary>
        /// The number of ticks per microsecond.
        /// </summary>
        public const int TicksPerMicrosecond = 10;
        /// <summary>
        /// The number of ticks per Nanosecond.
        /// </summary>
        public const int NanosecondsPerTick = 100;

        /// <summary>
        /// Gets the microsecond fraction of a DateTime.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static long Microseconds(this TimeSpan self)
        {
            return (int)Math.Floor(
               (self.Ticks
               % TimeSpan.TicksPerMillisecond)
               / (double)TicksPerMicrosecond); //TODO: check validity of this.
        }
        /// <summary>
        /// Gets the Nanosecond fraction of a DateTime.  Note that the DateTime
        /// object can only store nanoseconds at resolution of 100 nanoseconds.
        /// </summary>
        /// <param name="self">The DateTime object.</param>
        /// <returns>the number of Nanoseconds.</returns>
        public static long Nanoseconds(this TimeSpan self)
        {
            return (self.Ticks / TimeSpan.TicksPerMillisecond) * 1_000_000L;
        }
        
    }
}
