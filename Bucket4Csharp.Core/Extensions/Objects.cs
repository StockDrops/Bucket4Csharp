using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Extensions
{
    /// <summary>
    /// <a href="https://stackoverflow.com/questions/754330/is-there-a-way-to-require-that-an-argument-provided-to-a-method-is-not-null">see stackoverflow.</a>
    /// </summary>
    public static class Objects
    {
        public static T RequireNotNull<T>(T arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }
            return arg;
        }
        public static object[] RequireNotNullArray(params object[] args)
        {
            return RequireNotNullArray<object>(args);
        }
        public static T[] RequireNotNullArray<T>(params T[] args)
        {
            Objects.RequireNotNull(args);
            for (int i = 0; i < args.Length; i++)
            {
                T arg = args[i];
                if (arg == null)
                {
                    throw new ArgumentNullException($"null entry at position:{i}");
                }
            }
            return args;
        }
    }
}
