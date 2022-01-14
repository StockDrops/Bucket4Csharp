using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Models
{
    
    public enum MathType
    {
        /// <summary>
        /// The default math precision that uses integer arithmetic with 64 bits numbers.
        /// </summary>
        Integer64Bits,
        Integer32Bits,
        //I am not porting experimental features.
    }
}
