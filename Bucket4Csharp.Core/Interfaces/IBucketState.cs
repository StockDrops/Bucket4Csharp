using Bucket4Csharp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Interfaces
{
    public interface IBucketState
    {

        IBucketState Copy();

        IBucketState ReplaceConfiguration(BucketConfiguration previousConfiguration, BucketConfiguration newConfiguration,
                                         TokensInheritanceStrategy tokensInheritanceStrategy, long currentTimeNanos);

        void CopyStateFrom(IBucketState sourceState);

        long GetAvailableTokens(Bandwidth[] bandwidths);

        void Consume(Bandwidth[] bandwidths, long toConsume);

        long CalculateDelayNanosAfterWillBePossibleToConsume(Bandwidth[] bandwidths, long tokensToConsume, long currentTimeNanos);

        long CalculateFullRefillingTime(Bandwidth[] bandwidths, long currentTimeNanos);

        void RefillAllBandwidth(Bandwidth[] limits, long currentTimeNanos);

        void AddTokens(Bandwidth[] bandwidths, long tokensToAdd);

        void ForceAddTokens(Bandwidth[] bandwidths, long tokensToAdd);

        long GetCurrentSize(int bandwidth);

        long GetRoundingError(int bandwidth);

        MathType MathType { get; }

        static IBucketState CreateInitialState(BucketConfiguration configuration, MathType mathType, long currentTimeNanos)
        {
            switch (mathType)
            {
                case MathType.Integer64Bits: return new BucketState64BitsInteger(configuration, currentTimeNanos);
                case MathType.Integer32Bits: return new BucketState32BitsInteger(configuration, currentTimeNanos);
                default: throw new InvalidOperationException("Unsupported mathType:" + mathType);
            }
        }
    }
}
