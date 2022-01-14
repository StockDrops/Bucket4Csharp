using Bucket4Csharp.Core.Interfaces;
using Bucket4Csharp.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Bucket4Csharp.Core.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task SchedulingBucketExample()
        {
            var limit = Bandwidth.Simple(1, TimeSpan.FromSeconds(30));
            var bucket = IBucket.CreateBuilder()
                            .AddLimit(limit)
                            .Build() as ISchedulingBucket;
            if(bucket != null)
            {
                var count = 0;
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(240));
                while (count < 10)
                {
                    await bucket.ConsumeAsync(1, cancellationTokenSource.Token);
                    Debug.WriteLine($"{DateTime.Now:T}");
                    count++;
                }
            }
            

        }
        [TestMethod]
        public async Task ThrottlingExample()
        {
            var bucket = CreateNewBucket();
            var count = 0;
            while(count < 500)
            {
                if (bucket.TryConsume(1))
                {
                    Debug.WriteLine("Allowed");
                }
                else
                {

                    Debug.WriteLine("NotAllowed");
                    Debug.WriteLine($"{DateTime.Now:T}");
                }

                count++;
                await Task.Delay(500);
            }
        }
        [TestMethod]
        public async Task MultipleBandwidth()
        {
            var bucket = IBucket.CreateBuilder()
                        // allows 1000 tokens per 1 minute
                        .AddLimit(Bandwidth.Simple(30, TimeSpan.FromMinutes(1)))
                        // but not often then 1 tokens per 1 second
                        .AddLimit(Bandwidth.Simple(1, TimeSpan.FromSeconds(1)))
                        .Build() as ISchedulingBucket;
            var cancellationToken = new CancellationTokenSource().Token;
            while (true)
            {
                if(await bucket.TryConsumeAsync(1, 10000, cancellationToken))
                {
                    Debug.WriteLine("Allowed");
                    Debug.WriteLine($"{DateTime.Now:T}");
                }
                else
                {
                    Debug.WriteLine("Not Allowed");
                    Debug.WriteLine($"{DateTime.Now:T}");
                }
            }
        }
        private IBucket CreateNewBucket()
        {
            long overdraft = 5;
            Refill refill = Refill.Greedy(10, TimeSpan.FromSeconds(10));
            Bandwidth limit = Bandwidth.Classic(overdraft, refill);
            return IBucket.CreateBuilder().AddLimit(limit).Build();
        }
    }
}