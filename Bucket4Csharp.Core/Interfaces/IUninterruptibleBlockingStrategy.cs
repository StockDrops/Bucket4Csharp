using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Interfaces
{
    public interface IUninterruptibleBlockingStrategy
    {
        void ParkUninterruptibly(long nanosToPark);
    }
}
