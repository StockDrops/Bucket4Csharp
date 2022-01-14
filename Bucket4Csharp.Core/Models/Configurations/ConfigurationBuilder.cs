using Bucket4Csharp.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models
{
    /// <summary>
    /// The builder for <see cref="BucketConfiguration"/>
    /// </summary>
    public class ConfigurationBuilder
    {
        private readonly IList<Bandwidth> bandwidths;
        public ConfigurationBuilder()
        {
            this.bandwidths = new List<Bandwidth>();
        }

        /// <summary>
        /// Configuration to be used for bucket construction.
        /// </summary>
        /// <returns></returns>
        public BucketConfiguration Build()
        {
            return new BucketConfiguration(this.bandwidths);
        }
        /// <summary>
        /// Adds limited bandwidth for all buckets which will be constructed by this builder instance.
        /// </summary>
        /// <param name="bandwidth">bandwidth limitation</param>
        /// <returns>this builder instance</returns>
        public ConfigurationBuilder AddLimit(Bandwidth bandwidth)
        {
            if (bandwidth == null)
            {
                throw BucketExceptions.NullBandwidth();
            }
            bandwidths.Add(bandwidth);
            return this;
        }
        public override string ToString()
        {
            return "ConfigurationBuilder{" +
                "bandwidths=" + bandwidths +
                '}';
        }
    }
}
