using MediatR;

namespace Mebabl.Platform.Application.Features.Storage.Buckets.CreateBucket;

public sealed record CreateBucketCommand(
    string Name,
    string Code,
    string Description,
    bool IsPublic
) : IRequest<CreateBucketResponse>;