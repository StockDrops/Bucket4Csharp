using Bucket4Csharp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models.Local
{
    public interface ILocalBucket : IBucket
    {
        BucketConfiguration Configuration { get; }
        ITimeMeter TimeMeter { get; }
    }
}
