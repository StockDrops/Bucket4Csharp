using Bucket4Csharp.Core.Extensions;
using Bucket4Csharp.Core.Interfaces.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models
{
    public class BucketConfiguration : ComparableByContent<BucketConfiguration>
    {
        private readonly Bandwidth[] bandwidths;
        public BucketConfiguration(IList<Bandwidth> bandwidths)
        {
            Objects.RequireNotNullArray(bandwidths);
            if(bandwidths.Count == 0)
                throw new ArgumentException(nameof(bandwidths), "Cannot be empty.");

        }
        public override bool EqualsByContent(BucketConfiguration? other)
        {
            throw new NotImplementedException();
        }
    }
}
