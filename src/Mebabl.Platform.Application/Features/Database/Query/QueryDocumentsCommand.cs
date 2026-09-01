using MediatR;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

namespace Mebabl.Platform.Application.Features.Database.Query;

public sealed record QueryDocumentsCommand(
    Guid CollectionId,
    IReadOnlyCollection<QueryFilter> Filters,
    IReadOnlyCollection<QuerySort> Sorts,
    int Offset,
    int Limit,
    string? Search = null,
    IReadOnlyCollection<string>? Select = null
) : IRequest<IReadOnlyList<QueryDocumentsResponse>>;