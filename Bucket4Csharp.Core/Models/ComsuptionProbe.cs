using Bucket4Csharp.Core.Interfaces;
using Bucket4Csharp.Core.Interfaces.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models;

//original code: https://github.com/vladimir-bukhtoyarov/bucket4j/blob/master/bucket4j-core/src/main/java/io/github/bucket4j/ConsumptionProbe.java
//https://stackoverflow.com/questions/1327544/what-is-the-equivalent-of-javas-final-in-c 
/// <summary>
/// Describes tokens consumed, tokens remaining, time required for token regeneration to occur, and
/// the current bucket configuration after consumption.
/// <br/>
/// <see cref="IBucket.TryConsumeAndReturnRemaining(long)"/>
/// <see cref=""/>
/// </summary>
public class ComsuptionProbe : ComparableByContent<ComsuptionProbe>
{
    private readonly bool consumed;
    private readonly long remainingTokens;
    private readonly long nanosToWaitForRefill;
    private readonly long nanosToWaitForReset;

    // TODO: distributed properties. https://github.com/vladimir-bukhtoyarov/bucket4j/blob/master/bucket4j-core/src/main/java/io/github/bucket4j/distributed/serialization/SerializationHandle.java
    // public sealed static SerializationHandle<ConsumptionProbe> SERIALIZATION_HANDLE = new SerializationHandle<ConsumptionProbe>() { }

    public static ComsuptionProbe Consumed(long remainingTokens, long nanosToWaitForReset)
    {
        return new ComsuptionProbe(true, remainingTokens, 0, nanosToWaitForReset);
    }

    public static ComsuptionProbe Rejected(long remainingTokens, long nanosToWaitForRefill, long nanosToWaitForReset)
    {
        return new ComsuptionProbe(false, remainingTokens, nanosToWaitForRefill, nanosToWaitForReset);
    }

    private ComsuptionProbe(bool consumed, long remainingTokens, long nanosToWaitForRefill, long nanosToWaitForReset)
    {
        this.consumed = consumed;
        this.remainingTokens = Math.Max(remainingTokens, 0);
        this.nanosToWaitForRefill = nanosToWaitForRefill;
        this.nanosToWaitForReset = nanosToWaitForReset;
    }
    /// <summary>
    /// Flag describes result of consumption operation.
    /// True if tokens were consumed.
    /// </summary>
    public bool IsConsumed => consumed;
    /// <summary>
    /// Gets the number of tokens remaining in the bucket.
    /// </summary>
    public long RemainingTokens => remainingTokens;
    /// <summary>
    /// Returns zero if <see cref="IsConsumed"/> returns true, else time in nanos which need to wait until requested amount of tokens will be refilled
    /// </summary>
    public long NanosToWaitForRefill => nanosToWaitForRefill;
    /// <summary>
    /// Time in nanoseconds which needs to be awaited until the bucket will be fully refilled to its maximum
    /// </summary>
    public long NanosToWaitForReset => nanosToWaitForReset;

    ///<inheritdoc/>
    public override string ToString()
    {
        return "ConsumptionProbe{" +
                "consumed=" + consumed +
                ", remainingTokens=" + remainingTokens +
                ", nanosToWaitForRefill=" + nanosToWaitForRefill +
                ", nanosToWaitForReset=" + nanosToWaitForReset +
                '}';
    }

    ///<inheritdoc/>
    public override bool EqualsByContent(ComsuptionProbe? other)
    {
        if (other == null) { return false; }
        return consumed == other.consumed &&
                remainingTokens == other.remainingTokens &&
                nanosToWaitForRefill == other.nanosToWaitForRefill &&
                nanosToWaitForReset == other.nanosToWaitForReset;
    }
}
