using Bucket4Csharp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models
{
    public class StopWatchTimeMeter : ITimeMeter
    {
        public long Nanoseconds => 1000000L * Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;
        public long Microseconds => 1000L * Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;
        public long Milliseconds => Stopwatch.GetTimestamp() / TimeSpan.TicksPerMillisecond;

        public bool IsWallClockBased => false;
    }
}
