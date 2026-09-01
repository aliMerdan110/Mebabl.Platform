using MediatR;

namespace Mebabl.Platform.Application.Features.Storage.Buckets.UpdateBucket;

public sealed record UpdateBucketCommand(
    Guid Id,
    string Name,
    string Description,
    bool IsPublic,
    bool IsActive
) : IRequest;