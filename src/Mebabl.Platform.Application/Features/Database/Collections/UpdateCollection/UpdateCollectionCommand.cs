using MediatR;
using Mebabl.Platform.Application.Features.Database.Collections.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Collections.UpdateCollection;

/// <summary>
/// Updates collection metadata within the project resolved from the
/// authenticated application context. ProjectId is deliberately not accepted
/// from the request to prevent callers from selecting another project's scope.
/// </summary>
public sealed record UpdateCollectionCommand(
    Guid CollectionId,
    string Name,
    string Description)
    : IRequest<CollectionResponse>;
