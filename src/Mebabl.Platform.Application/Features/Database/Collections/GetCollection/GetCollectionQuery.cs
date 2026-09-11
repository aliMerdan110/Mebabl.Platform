using MediatR;
using Mebabl.Platform.Application.Features.Database.Collections.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Collections.GetCollection;

public sealed record GetCollectionQuery(
    Guid CollectionId)
    : IRequest<CollectionResponse>;