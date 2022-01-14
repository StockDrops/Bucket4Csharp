using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
namespace Bucket4Csharp.Core.Interfaces.Utilities
{
    /// <summary>
    /// Ports <a href="https://github.com/vladimir-bukhtoyarov/bucket4j/blob/85a0148788223bc968fe4faa72f733b68dbf129f/bucket4j-core/src/main/java/io/github/bucket4j/util/ComparableByContent.java#L24">
    /// l.24</a>.
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IComparableByContent<T> : IEquatable<T> where T: IComparableByContent<T>
    {
        bool EqualsByContent(T? other);
    }
    /// <summary>
    /// Abstract class for comparable by content elements see original java code in <a href="https://github.com/vladimir-bukhtoyarov/bucket4j/blob/85a0148788223bc968fe4faa72f733b68dbf129f/bucket4j-core/src/main/java/io/github/bucket4j/util/ComparableByContent.java#L24">
    /// l.24</a>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ComparableByContent<T> : IComparableByContent<T> where T : IComparableByContent<T>
    {
        public bool Equals(T? other)
        {
            if (object.ReferenceEquals(other, null))
                return true;
            if((this == null && other != null) || (other == null && this != null))
                return false;
            if(this == null && other == null)
                return true;
            //https://github.com/vladimir-bukhtoyarov/bucket4j/blob/85a0148788223bc968fe4faa72f733b68dbf129f/bucket4j-core/src/main/java/io/github/bucket4j/util/ComparableByContent.java#L36
            //this line makes no sense to port since we have no static operator yet on c#. this is always of that type...
            return this.EqualsByContent(other);
        }
        public abstract bool EqualsByContent(T? other);
    }
}
