using MediatR;
using Mebabl.Platform.Application.Features.Database.Collections.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Collections.UpdateCollection;

public sealed record UpdateCollectionCommand(
    Guid CollectionId,
    string Name,
    string Description)
    : IRequest<CollectionResponse>;