# Bucket4Csharp

A port of the Bucket4J library https://github.com/vladimir-bukhtoyarov/bucket4j to C#. Trying to port the functionality and algorithms while keeping the syntax more C#'py (instead of GetX use a property called X).

# Usage

The basic usage is done with the IBucket interface which has a static CreateBuilder method.

You can call this method to create a bucket builder which will create a LockFreeBucket instance:

```csharp
var bucketBuilder = IBucket.CreateBuilder(); //create the builder.
var newBucket = bucketBuilder.AddLimit(Bandwidth.Simple(100, TimeSpan.FromHours(1))).Build(); //100 requests every hour max.
```

You can then use the bucket as simple as:

```csharp
var result = newBucket.TryConsume(1).
```
If there's an available token it will return true. If the rate limit is reached it will return false.

You can also cast it to an ISchedulingBucket if you want to wait until there's an available token using the async/await, and TryConsumeAsync.
