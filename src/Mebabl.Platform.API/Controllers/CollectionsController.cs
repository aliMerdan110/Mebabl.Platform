using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.Database.Collections.CreateCollection;
using Mebabl.Platform.Application.Features.Database.Collections.DeleteCollection;
using Mebabl.Platform.Application.Features.Database.Collections.GetCollection;
using Mebabl.Platform.Application.Features.Database.Collections.ListCollections;
using Mebabl.Platform.Application.Features.Database.Collections.UpdateCollection;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Route("api/sdk/data/collections")]
[EnableCors("MebablSdk")]
public sealed class CollectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CollectionsController(IMediator mediator)
    {
        _mediator = mediator
            ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Creates a collection in the project database associated with the
    /// authenticated trusted application credential.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Application")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Create(
        [FromBody] CreateCollectionCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(Get),
            new { collectionId = result.Id },
            result);
    }

    /// <summary>
    /// Lists collections in the authenticated user's project that are
    /// visible under the published database security policies.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "ApplicationUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> List(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ListCollectionsQuery(),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets a collection only when it belongs to the authenticated user's
    /// project and its security policy allows the requested operation.
    /// </summary>
    [HttpGet("{collectionId:guid}")]
    [Authorize(Policy = "ApplicationUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Get(
        [FromRoute] Guid collectionId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetCollectionQuery(collectionId),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Updates collection metadata using a trusted application credential.
    /// Collection ownership is resolved and enforced by the command handler.
    /// </summary>
    [HttpPut("{collectionId:guid}")]
    [Authorize(Policy = "Application")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(
        [FromRoute] Guid collectionId,
        [FromBody] UpdateCollectionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _mediator.Send(
            new UpdateCollectionCommand(
                collectionId,
                request.Name,
                request.Description),
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Soft-deletes a collection using a trusted application credential.
    /// </summary>
    [HttpDelete("{collectionId:guid}")]
    [Authorize(Policy = "Application")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid collectionId,
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
