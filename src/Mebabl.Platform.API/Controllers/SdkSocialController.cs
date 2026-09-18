using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.SdkSocial.Reactions.GetReactions;
using Mebabl.Platform.Application.Features.SdkSocial.Reactions.React;
using Mebabl.Platform.Application.Features.SdkSocial.Reactions.RemoveReaction;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Authorize(Policy = "Application")]
[Route("api/sdk/social")]
public sealed class SdkSocialController : ControllerBase
{
    private readonly ISender _sender;

    public SdkSocialController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("posts/{postId:guid}/reactions")]
    public async Task<IActionResult> React(
        Guid postId,
        [FromBody] ReactRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ReactCommand(postId, request.Type),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("posts/{postId:guid}/reactions")]
    public async Task<IActionResult> RemoveReaction(
        Guid postId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new RemoveReactionCommand(postId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("posts/{postId:guid}/reactions")]
    public async Task<IActionResult> GetReactions(
        Guid postId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetReactionsQuery(postId),
            cancellationToken);

        return Ok(result);
    }
}

public sealed record ReactRequest(string Type);