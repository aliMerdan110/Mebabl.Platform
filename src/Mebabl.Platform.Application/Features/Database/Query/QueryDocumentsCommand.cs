using MediatR;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

namespace Mebabl.Platform.Application.Features.Database.Query;

/// <summary>
/// Executes a query against a collection in the authenticated project.
/// Project, application, account, and user scope are always resolved from
/// the validated access token and are never accepted from the request body.
/// </summary>
public sealed record QueryDocumentsCommand(
    Guid CollectionId,
    IReadOnlyList<QueryFilter>? Filters = null,
    IReadOnlyList<QuerySort>? Sorts = null,
    int Offset = 0,
    int Limit = QueryRequest.DefaultPageSize,
    string? Search = null,
    IReadOnlyList<string>? Select = null,
    string? PageToken = null)
    : IRequest<QueryDocumentsPageResponse>
{
    public const int MaximumPageTokenLength = 4096;
}
