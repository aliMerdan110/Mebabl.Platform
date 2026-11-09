using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.Database.Query;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/sdk/data/query")]
[Authorize(Policy = "ApplicationUser")]
[EnableCors("MebablSdk")]
public sealed class QueryController : ControllerBase
{
    private readonly IMediator _mediator;

    public QueryController(IMediator mediator)
    {
        _mediator = mediator
            ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Executes a project-scoped collection query and returns a page.
    /// Supply the returned nextPageToken as pageToken to continue using
    /// keyset pagination with the same collection, filters, and search.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(QueryDocumentsPageResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<QueryDocumentsPageResponse>> Query(
        [FromBody] QueryRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _mediator.Send(
            new QueryDocumentsCommand(
                request.CollectionId,
                request.Filters,
                request.Sorts,
                request.Offset,
                request.Limit,
                request.Search,
                request.Select,
                request.PageToken),
            cancellationToken);

        return Ok(result);
    }
}
