using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Interfaces
{
    /// <summary>
    /// Specifies the way to block current thread to amount of time required to refill missed number of tokens in the bucket.
    /// There is default implementation
    /// </summary>
    public interface IBlockingStrategy
    {
        void Park(long nanosToPark);
    }
}
