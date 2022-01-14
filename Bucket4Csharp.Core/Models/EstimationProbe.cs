using Bucket4Csharp.Core.Interfaces.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models
{
    public class EstimationProbe : ComparableByContent<EstimationProbe>
    {
        private readonly bool canBeConsumed;
        private readonly long remainingTokens;
        private readonly long nanosToWaitForRefill;
        // TODO: distributed properties. https://github.com/vladimir-bukhtoyarov/bucket4j/blob/master/bucket4j-core/src/main/java/io/github/bucket4j/distributed/serialization/SerializationHandle.java
        // public static final SerializationHandle<EstimationProbe> 
        /// <summary>
        /// Creates a consumable estimation probe. Java equivalent in EstimationProbe.canBeConsumed.
        /// Name change was required to avoid the property clashing with this static method.
        /// </summary>
        /// <param name="remainingTokens"></param>
        /// <returns></returns>
        public static EstimationProbe Consumable(long remainingTokens)
        {
            return new EstimationProbe(true, remainingTokens, 0);
        }
        /// <summary>
        /// Creates a non consumable estimation probe. Java equivalent in EstimationProbe.canNotBeConsumed.
        /// Name change was required to avoid the property clashing with this static method.
        /// </summary>
        /// <param name="remainingTokens"></param>
        /// <param name="nanosToWaitForRefill"></param>
        /// <returns></returns>
        public static EstimationProbe NotConsumable(long remainingTokens, long nanosToWaitForRefill)
        {
            return new EstimationProbe(false, remainingTokens, nanosToWaitForRefill);
        }
        private EstimationProbe(bool canBeConsumed, long remainingTokens, long nanosToWaitForRefill)
        {
            this.canBeConsumed = canBeConsumed;
            this.remainingTokens = Math.Max(0, remainingTokens);
            this.nanosToWaitForRefill = nanosToWaitForRefill;
        }
        /// <summary>
        /// Flag describes result of consumption operation.
        /// </summary>
        public bool CanBeConsumed => canBeConsumed;
        /// <summary>
        /// Return the tokens remaining in the bucket
        /// </summary>
        public long RemainingTokens => remainingTokens;
        /// <summary>
        /// Returns zero if <see cref="CanBeConsumed"/> returns true, else time in nanos which need to wait until requested amount of tokens will be refilled
        /// </summary>
        public long NanosToWaitForRefill => nanosToWaitForRefill;

        public override string ToString()
        {
            return "ConsumptionResult{" +
                "isConsumed=" + canBeConsumed +
                ", remainingTokens=" + remainingTokens +
                ", nanosToWaitForRefill=" + nanosToWaitForRefill +
                '}';
        }

        public override bool EqualsByContent(EstimationProbe? other)
        {
            if(other == null) { return false; }
            return canBeConsumed == other.canBeConsumed &&
                remainingTokens == other.remainingTokens &&
                nanosToWaitForRefill == other.nanosToWaitForRefill;
        }
    }
}
