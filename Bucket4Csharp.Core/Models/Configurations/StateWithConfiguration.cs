using Bucket4Csharp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models
{
    public enum TimeUnit
    {
        Milliseconds,
        Microseconds,
        Nanoseconds
    }
    internal class StateWithConfiguration
    {

        internal BucketConfiguration configuration;
        internal IBucketState state;

        internal StateWithConfiguration(BucketConfiguration configuration, IBucketState state)
        {
            this.configuration = configuration;
            this.state = state;
        }

        internal StateWithConfiguration Copy()
        {
            return new StateWithConfiguration(configuration, state.Copy());
        }

        internal void CopyStateFrom(StateWithConfiguration other)
        {
            configuration = other.configuration;
            state.CopyStateFrom(other.state);
        }

        internal void RefillAllBandwidth(long currentTime, TimeUnit timeUnitUsed = TimeUnit.Nanoseconds)
        {
            switch (timeUnitUsed)
            {
                case TimeUnit.Milliseconds:
                    break;
                case TimeUnit.Microseconds:
                    break;
                case TimeUnit.Nanoseconds:
                    state.RefillAllBandwidth(configuration.Bandwidths, currentTime);
                    break;
            }
            
        }

        internal long GetAvailableTokens()
        {
            return state.GetAvailableTokens(configuration.Bandwidths);
        }

        internal void Consume(long tokensToConsume)
        {
            state.Consume(configuration.Bandwidths, tokensToConsume);
        }

        internal long CalculateFullRefillingTime(long currentTimeNanos)
        {
            return state.CalculateFullRefillingTime(configuration.Bandwidths, currentTimeNanos);
        }

        internal long DelayNanosAfterWillBePossibleToConsume(long tokensToConsume, long currentTimeNanos)
        {
            return state.CalculateDelayNanosAfterWillBePossibleToConsume(configuration.Bandwidths, tokensToConsume, currentTimeNanos);
        }

    }
}
