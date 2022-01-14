using Bucket4Csharp.Core.Exceptions;
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
            this.bandwidths = new Bandwidth[bandwidths.Count];
            for(int i = 0; i < bandwidths.Count; i++)
            {
                this.bandwidths[i] = Objects.RequireNotNull(bandwidths[i]);
            }
            for(int i = 0; i < this.bandwidths.Length; i++)
            {
                if(this.bandwidths[i].Id != Bandwidth.UNDEFINED_ID)
                {
                    for (int j = i + 1; j < this.bandwidths.Length; j++)
                    {
                        if (this.bandwidths[i].Id == this.bandwidths[j].Id)
                        {
                            throw BucketExceptions.FoundTwoBandwidthsWithSameId(i, j, this.bandwidths[i].Id);
                        }
                    }
                }
            }
        }
        public static ConfigurationBuilder CreateBuilder()
        {
            return new ConfigurationBuilder();
        }
        public Bandwidth[] Bandwidths => this.bandwidths;
        public override bool Equals(object? o)
        {
            if (this == o) { return true; }
            if (o == null || GetType() != o.GetType()) { return false; }

            BucketConfiguration that = (BucketConfiguration)o;

            return Enumerable.SequenceEqual(bandwidths, that.bandwidths);
        }
        public override int GetHashCode()
        {
            return bandwidths.GetHashCode();
        }
        public override string ToString()
        {
            return "BucketConfiguration{" +
                "bandwidths=" + string.Join(',',bandwidths.Select(x => x.ToString())) +
                '}';
        }
        // TODO: https://github.com/vladimir-bukhtoyarov/bucket4j/blob/85a0148788223bc968fe4faa72f733b68dbf129f/bucket4j-core/src/main/java/io/github/bucket4j/BucketConfiguration.java#L91
        // skipped until we actually develop the distributed into the code.
        // for now port is limited to local.

        public override bool EqualsByContent(BucketConfiguration? other)
        {
            if(other == null) { return false; }
            if (bandwidths.Length != other.bandwidths.Length)
            {
                return false;
            }
            for (int i = 0; i < other.Bandwidths.Length; i++)
            {
                Bandwidth bandwidth1 = bandwidths[i];
                Bandwidth bandwidth2 = other.bandwidths[i];
                if (!bandwidth1.EqualsByContent(bandwidth2))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
