using Bucket4Csharp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models
{
    public class SystemTimeMeter : ITimeMeter
    {
        public long Nanoseconds => 1000L * CurrentTimeMicroseconds();
        public long Microseconds => CurrentTimeMicroseconds();
        public long Milliseconds => CurrentTimeMilliseconds();

        public bool IsWallClockBased => true;


        private static readonly DateTime Jan1st1970 = new DateTime
                         (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long CurrentTimeMilliseconds()
        {
            return (DateTime.UtcNow - Jan1st1970).Ticks / TimeSpan.TicksPerMillisecond;
        }
        private static long CurrentTimeMicroseconds()
        {
            return 1000L * (DateTime.UtcNow - Jan1st1970).Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}
