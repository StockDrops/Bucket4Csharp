using Bucket4Csharp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models
{
    public class ParkingBlockingStrategy : IBlockingStrategy
    {
        public void Park(long nanosToPark)
        {
            long endNanos = ITimeMeter.StopWatchTimeMeter.Nanoseconds + nanosToPark;
            long remainingParkNanos = nanosToPark;
            //while (true)
            //{
            //    LockSupport.parkNanos(remainingParkNanos);
            //    long currentTimeNanos = System.nanoTime();
            //    remainingParkNanos = endNanos - currentTimeNanos;
            //    if (Thread.interrupted())
            //    {
            //        throw new InterruptedException();
            //    }
            //    if (remainingParkNanos <= 0)
            //    {
            //        return;
            //    }
            //}
        }
    }
}
