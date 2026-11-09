using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
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
[Route("api/sdk/social")]
[Authorize(Policy = "ApplicationUser")]
[EnableCors("MebablSdk")]
public sealed class SdkSocialController : ControllerBase
{
    private const int MaximumPageSize = 100;
    private const int MaximumCommentLength = 5000;
    private const int MaximumPostIds = 100;

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
        if (postId == Guid.Empty)
            return BadRequest(new { error = "Invalid post ID." });

        if (request is null ||
            string.IsNullOrWhiteSpace(request.Type) ||
            request.Type.Length > 50 ||
            request.Type.Any(char.IsControl))
        {
            return BadRequest(new { error = "Invalid reaction type." });
        }

        var result = await _sender.Send(
            new ReactCommand(postId, request.Type.Trim()),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("posts/{postId:guid}/reactions")]
    public async Task<IActionResult> RemoveReaction(
        Guid postId,
        CancellationToken cancellationToken)
    {
        if (postId == Guid.Empty)
            return BadRequest(new { error = "Invalid post ID." });

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
        if (postId == Guid.Empty)
            return BadRequest(new { error = "Invalid post ID." });

        var result = await _sender.Send(
            new GetReactionsQuery(postId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("posts/{postId:guid}/comments")]
    public async Task<IActionResult> CreateComment(
        Guid postId,
        [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (postId == Guid.Empty)
            return BadRequest(new { error = "Invalid post ID." });

        if (request is null ||
            !IsValidComment(request.Text))
        {
            return BadRequest(new
            {
                error = $"Comment text must contain 1 to {MaximumCommentLength} characters."
            });
        }

        var result = await _sender.Send(
            new CreateCommentCommand(
                postId,
                request.Text.Trim()),
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
        if (postId == Guid.Empty)
            return BadRequest(new { error = "Invalid post ID." });

        if (page < 1 || pageSize < 1 || pageSize > MaximumPageSize)
        {
            return BadRequest(new
            {
                error = $"Page must be positive and pageSize must be between 1 and {MaximumPageSize}."
            });
        }

        var result = await _sender.Send(
            new GetCommentsQuery(postId, page, pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("comments/{commentId:guid}")]
    public async Task<IActionResult> UpdateComment(
        Guid commentId,
        [FromBody] UpdateCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (commentId == Guid.Empty)
            return BadRequest(new { error = "Invalid comment ID." });

        if (request is null ||
            !IsValidComment(request.Text))
        {
            return BadRequest(new
            {
                error = $"Comment text must contain 1 to {MaximumCommentLength} characters."
            });
        }

        var result = await _sender.Send(
            new UpdateCommentCommand(
                commentId,
                request.Text.Trim()),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(
        Guid commentId,
        CancellationToken cancellationToken)
    {
        if (commentId == Guid.Empty)
            return BadRequest(new { error = "Invalid comment ID." });

        await _sender.Send(
            new DeleteCommentCommand(commentId),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("posts/{postId:guid}/shares")]
    public async Task<IActionResult> Share(
        Guid postId,
        CancellationToken cancellationToken)
    {
        if (postId == Guid.Empty)
            return BadRequest(new { error = "Invalid post ID." });

        var result = await _sender.Send(
            new ShareCommand(postId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("posts/{postId:guid}/reposts")]
    public async Task<IActionResult> Repost(
        Guid postId,
        CancellationToken cancellationToken)
    {
        if (postId == Guid.Empty)
            return BadRequest(new { error = "Invalid post ID." });

        var result = await _sender.Send(
            new RepostCommand(postId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("posts/stats")]
    public async Task<IActionResult> GetPostStats(
        [FromQuery] string? postIds,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(postIds))
        {
            return BadRequest(new
            {
                error = "postIds is required."
            });
        }

        var rawIds = postIds.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

        if (rawIds.Length == 0 ||
            rawIds.Length > MaximumPostIds)
        {
            return BadRequest(new
            {
                error = $"Provide between 1 and {MaximumPostIds} post IDs."
            });
        }

        var ids = new List<Guid>(rawIds.Length);

        foreach (var rawId in rawIds)
        {
            if (!Guid.TryParse(rawId, out var id) ||
                id == Guid.Empty)
            {
                return BadRequest(new
                {
                    error = "One or more post IDs are invalid."
                });
            }

            ids.Add(id);
        }

        var result = await _sender.Send(
            new GetPostStatsQuery(ids.Distinct().ToArray()),
            cancellationToken);

        return Ok(result);
    }

    private static bool IsValidComment(string? text)
    {
        return !string.IsNullOrWhiteSpace(text) &&
               text.Trim().Length <= MaximumCommentLength &&
               !text.Any(c => c == '\0');
    }
}

public sealed record ReactRequest(string Type);

public sealed record CreateCommentRequest(string Text);

public sealed record UpdateCommentRequest(string Text);