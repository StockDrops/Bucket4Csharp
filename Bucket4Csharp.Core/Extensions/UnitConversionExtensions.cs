using Bucket4Csharp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Extensions
{
    public static class UnitConversionExtensions
    {
        public static long ConvertTo(this long value, TimeUnit originTimeUnit, TimeUnit destinationTimeUnit)
        {
            switch (originTimeUnit)
            {
                case TimeUnit.Nanoseconds:
                    return value.ConvertFromNanoSeconds(destinationTimeUnit);
                default:
                    throw new NotImplementedException("Conversion from this unit is not implemented yet.");
            }
        }
        private static long ConvertFromNanoSeconds(this long value, TimeUnit destinationTimeUnit)
        {
            switch (destinationTimeUnit)
            {
                case TimeUnit.Nanoseconds:
                    return value;
                case TimeUnit.Microseconds:
                    return value / 1000L;
                case TimeUnit.Milliseconds:
                    return value / 1_000_000L;
                default:
                    throw new InvalidOperationException("Unknown time unit.");
            }
        }
    }
}
