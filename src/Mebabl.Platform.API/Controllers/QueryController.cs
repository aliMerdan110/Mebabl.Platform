using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Database.Query;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/sdk/data/query")]
[Authorize]
public sealed class QueryController : ControllerBase
{
    private readonly IMediator _mediator;

    public QueryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Query(
        QueryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new QueryDocumentsCommand(
                request.CollectionId,
                request.Filters,
                request.Sorts,
                request.Offset,
                request.Limit,
                request.Search,
                request.Select),
            cancellationToken);

        return Ok(result);
    }
}