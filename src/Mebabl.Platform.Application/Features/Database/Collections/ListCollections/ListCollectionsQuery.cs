using MediatR;
using Mebabl.Platform.Application.Features.Database.Collections.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Collections.ListCollections;

public sealed record ListCollectionsQuery
    : IRequest<IReadOnlyList<CollectionResponse>>;