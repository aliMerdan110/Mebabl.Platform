
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.SdkSocial.Comments.CreateComment;
using Mebabl.Platform.Application.Features.SdkSocial.Comments.DeleteComment;
using Mebabl.Platform.Application.Features.SdkSocial.Comments.GetComments;
using Mebabl.Platform.Application.Features.SdkSocial.Comments.UpdateComment;
using Mebabl.Platform.Application.Features.SdkSocial.Reposts.CreateRepost;
using Mebabl.Platform.Application.Features.SdkSocial.Shares.CreateShare;
using Mebabl.Platform.Application.Features.SdkSocial.PostStats.GetPostStats;

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

    // =========================================================
    // Reactions
    // =========================================================

    [HttpPost("posts/{postId:guid}/reactions")]
    public async Task<IActionResult> React(
        Guid postId,
        [FromBody] ReactRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ReactCommand(
                postId,
                request.Type),
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

    // =========================================================
    // Comments
    // =========================================================

    [HttpPost("posts/{postId:guid}/comments")]
    public async Task<IActionResult> CreateComment(
        Guid postId,
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateCommentCommand(
                postId,
                request.Text),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("posts/{postId:guid}/comments")]
    public async Task<IActionResult> GetComments(
        Guid postId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetCommentsQuery(
                postId,
                page,
                pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("comments/{commentId:guid}")]
    public async Task<IActionResult> UpdateComment(
        Guid commentId,
        [FromBody] UpdateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateCommentCommand(
                commentId,
                request.Text),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(
        Guid commentId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteCommentCommand(commentId),
            cancellationToken);

        return NoContent();
    }

    // =========================================================
    // Shares
    // =========================================================

    [HttpPost("posts/{postId:guid}/shares")]
    public async Task<IActionResult> Share(
        Guid postId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ShareCommand(postId),
            cancellationToken);

        return Ok(result);
    }

    // =========================================================
    // Reposts
    // =========================================================

    [HttpPost("posts/{postId:guid}/reposts")]
    public async Task<IActionResult> Repost(
        Guid postId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RepostCommand(postId),
            cancellationToken);

        return Ok(result);
    }


// 


    // 
[HttpGet("posts/stats")]
public async Task<IActionResult> GetPostStats(
    [FromQuery] string postIds,
    CancellationToken cancellationToken)
{
    var ids = postIds
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(Guid.Parse)
        .Distinct()
        .ToArray();

    var result = await _sender.Send(
        new GetPostStatsQuery(ids),
        cancellationToken);

    return Ok(result);
}


}

public sealed record ReactRequest(
    string Type);

public sealed record CreateCommentRequest(
    string Text);

public sealed record UpdateCommentRequest(
    string Text);
