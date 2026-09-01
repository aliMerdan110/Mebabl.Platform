using MediatR;

namespace Mebabl.Platform.Application.Features.Storage.Buckets.DeleteBucket;

public sealed record DeleteBucketCommand(
    Guid Id
) : IRequest;