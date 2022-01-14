using Bucket4Csharp.Core.Models;

namespace Bucket4Csharp.Core.Interfaces
{
    public class BucketState32BitsInteger : IBucketState
    {
        public BucketState32BitsInteger(BucketConfiguration configuration, long currentTimeNanos)
        {
            throw new NotImplementedException();
        }
        public MathType MathType => throw new NotImplementedException();

        public void AddTokens(Bandwidth[] bandwidths, long tokensToAdd)
        {
            throw new NotImplementedException();
        }

        public long CalculateDelayNanosAfterWillBePossibleToConsume(Bandwidth[] bandwidths, long tokensToConsume, long currentTimeNanos)
        {
            throw new NotImplementedException();
        }

        public long CalculateFullRefillingTime(Bandwidth[] bandwidths, long currentTimeNanos)
        {
            throw new NotImplementedException();
        }

        public void Consume(Bandwidth[] bandwidths, long toConsume)
        {
            throw new NotImplementedException();
        }

        public IBucketState Copy()
        {
            throw new NotImplementedException();
        }

        public void CopyStateFrom(IBucketState sourceState)
        {
            throw new NotImplementedException();
        }

        public void ForceAddTokens(Bandwidth[] bandwidths, long tokensToAdd)
        {
            throw new NotImplementedException();
        }

        public long GetAvailableTokens(Bandwidth[] bandwidths)
        {
            throw new NotImplementedException();
        }

        public long GetCurrentSize(int bandwidth)
        {
            throw new NotImplementedException();
        }

        public long GetRoundingError(int bandwidth)
        {
            throw new NotImplementedException();
        }

        public void RefillAllBandwidth(Bandwidth[] limits, long currentTimeNanos)
        {
            throw new NotImplementedException();
        }

        public IBucketState ReplaceConfiguration(BucketConfiguration previousConfiguration, BucketConfiguration newConfiguration, TokensInheritanceStrategy tokensInheritanceStrategy, long currentTimeNanos)
        {
            throw new NotImplementedException();
        }
    }
}