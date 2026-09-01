namespace Mebabl.Platform.Application.Features.Storage.Buckets.CreateBucket;

public sealed record CreateBucketResponse(
    Guid Id,
    string Name,
    string Code);