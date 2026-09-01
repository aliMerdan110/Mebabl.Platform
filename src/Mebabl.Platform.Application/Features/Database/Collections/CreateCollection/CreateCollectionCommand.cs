using MediatR;
using Mebabl.Platform.Application.Features.Database.Collections.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Collections.CreateCollection;

public sealed record CreateCollectionCommand(
    string Name,
    string Description)
    : IRequest<CollectionResponse>;