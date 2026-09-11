using MediatR;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

namespace Mebabl.Platform.Application.Features.Database.Query;

public sealed record QueryDocumentsCommand(
    Guid CollectionId,
    IReadOnlyList<QueryFilter> Filters,
    IReadOnlyList<QuerySort> Sorts,
    int Offset = 0,
    int Limit = 50,
    string? Search = null,
    IReadOnlyList<string>? Select = null)
    : IRequest<IReadOnlyList<QueryDocumentsResponse>>;