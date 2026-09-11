using MediatR;

namespace Mebabl.Platform.Application.Features.Database.Collections.DeleteCollection;

public sealed record DeleteCollectionCommand(
    Guid CollectionId)
    : IRequest;