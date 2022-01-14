using Bucket4Csharp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Interfaces
{
    /// <summary>
    /// An abstraction over time measurement.
    /// <see cref=""/>
    /// <see cref=""/>
    /// </summary>
    public interface ITimeMeter
    {
        /// <summary>
        /// Returns current time in nanosecond precision, but not necessarily nanosecond resolution.
        /// </summary>
        long Nanoseconds { get; }
        /// <summary>
        /// Returns {@code true} if implementation of clock behaves the similar way as {@link System#currentTimeMillis()}
        /// in other words if implementation can be used as wall clock.
        /// </summary>
        bool IsWallClockBased { get; }
        /// <summary>
        /// Provides regular precision around 1ms using <see cref="SystemTimeMeter"/> which uses <see cref="DateTime"/> behind the scenes.
        /// </summary>
        static ITimeMeter SystemTimeMeter => new SystemTimeMeter();
        /// <summary>
        /// Provides higher precision using <see cref="StopWatchTimeMeter"/> which uses a <see cref="System.Diagnostics.Stopwatch"/> behind the scenes.
        /// </summary>
        static ITimeMeter StopWatchTimeMeter => new StopWatchTimeMeter();
    }
}
