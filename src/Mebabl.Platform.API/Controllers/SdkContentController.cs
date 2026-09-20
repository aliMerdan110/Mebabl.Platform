using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.SdkContent.CreatePost;
using Mebabl.Platform.Application.Features.SdkContent.DeletePost;
using Mebabl.Platform.Application.Features.SdkContent.GetPost;
using Mebabl.Platform.Application.Features.SdkContent.GetPosts;
using Mebabl.Platform.Application.Features.SdkContent.UpdatePost;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Authorize(Policy = "ApplicationUser")]
[Route("api/sdk/content/posts")]
public sealed class SdkContentController : ControllerBase
{
    private readonly ISender _sender;

    public SdkContentController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePostCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(Get),
            new { postId = result.Id },
            result);
    }

    [HttpGet("{postId:guid}")]
    public async Task<IActionResult> Get(
        Guid postId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetPostQuery(postId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPosts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetPostsQuery(page, pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{postId:guid}")]
    public async Task<IActionResult> Update(
        Guid postId,
        [FromBody] UpdatePostRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdatePostCommand(
                postId,
                request.Text),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{postId:guid}")]
    public async Task<IActionResult> Delete(
        Guid postId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeletePostCommand(postId),
            cancellationToken);

        return NoContent();
    }
}

public sealed record UpdatePostRequest(
    string? Text);