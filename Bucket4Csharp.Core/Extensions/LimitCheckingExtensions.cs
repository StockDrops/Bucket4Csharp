using Bucket4Csharp.Core.Exceptions;
using Bucket4Csharp.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Extensions
{
    public static class LimitCheckingExtensions
    {
        public const long INFINITY_DURATION = long.MaxValue;
        public const long UNLIMITED_AMOUNT = long.MaxValue;

        public static void CheckTokensToAdd(this long tokensToAdd)
        {
            if (tokensToAdd <= 0)
            {
                throw new ArgumentException("tokensToAdd should be >= 0");
            }
        }

        public static void CheckTokensToConsume(this long tokensToConsume)
        {
            if (tokensToConsume <= 0)
            {
                throw BucketExceptions.NonPositiveTokensToConsume(tokensToConsume);
            }
        }

        public static void CheckMaxWaitTime(this long maxWaitTimeNanos)
        {
            if (maxWaitTimeNanos <= 0)
            {
                throw BucketExceptions.NonPositiveNanosToWait(maxWaitTimeNanos);
            }
        }

        //scheduler executor service doesn't exist in c#. We are using async/await which avoids this.
        //public static void CheckScheduler(ScheduledExecutorService scheduler)
        //{
        //    if (scheduler == null)
        //    {
        //        throw BucketExceptions.nullScheduler();
        //    }
        //}

        public static void CheckConfiguration(this BucketConfiguration? newConfiguration)
        {
            if (newConfiguration == null)
            {
                throw BucketExceptions.NullConfiguration();
            }
        }
        //makes no sense in c#. Enums are never null.
        //public static void checkMigrationMode(TokensInheritanceStrategy tokensInheritanceStrategy)
        //{
        //    if (tokensInheritanceStrategy == null)
        //    {
        //        throw BucketExceptions.nullTokensInheritanceStrategy();
        //    }
        //}
    }
}
