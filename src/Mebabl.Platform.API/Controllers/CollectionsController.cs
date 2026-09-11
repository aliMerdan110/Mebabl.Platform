using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.Database.Collections.CreateCollection;
using Mebabl.Platform.Application.Features.Database.Collections.GetCollection;
using Mebabl.Platform.Application.Features.Database.Collections.ListCollections;
using Mebabl.Platform.Application.Features.Database.Collections.UpdateCollection;
using Mebabl.Platform.Application.Features.Database.Collections.DeleteCollection;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/sdk/data/collections")]
[Authorize]
public sealed class CollectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CollectionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateCollectionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ListCollectionsQuery(),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{collectionId:guid}")]
    public async Task<IActionResult> Get(
        Guid collectionId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetCollectionQuery(collectionId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{collectionId:guid}")]
    public async Task<IActionResult> Update(
        Guid collectionId,
        UpdateCollectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateCollectionCommand(
                collectionId,
                request.Name,
                request.Description),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{collectionId:guid}")]
    public async Task<IActionResult> Delete(
        Guid collectionId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new DeleteCollectionCommand(collectionId),
            cancellationToken);

        return NoContent();
    }

    public sealed record UpdateCollectionRequest(
        string Name,
        string Description);
}