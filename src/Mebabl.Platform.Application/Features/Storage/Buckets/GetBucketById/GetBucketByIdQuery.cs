using MediatR;

namespace Mebabl.Platform.Application.Features.Storage.Buckets.GetBucketById;

public sealed record GetBucketByIdQuery(
    Guid Id
) : IRequest<GetBucketByIdResponse>;