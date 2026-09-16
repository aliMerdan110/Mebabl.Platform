using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mebabl.Platform.Application.Features.SdkPosts.Create;
using Mebabl.Platform.Application.Features.SdkPosts.DTOs;
using Mebabl.Platform.Application.Features.SdkPosts.List;

namespace Mebabl.Platform.API.Controllers;

[ApiController]
[Authorize(Policy = "Application")]
[Route("api/sdk/posts")]
public sealed class SdkPostsController : ControllerBase
{
    private readonly ISender _sender;

    public SdkPostsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> Create(
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreatePostCommand(
                request.Text,
                request.StorageFileIds),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PostDto>>> List(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListPostsQuery(skip, take),
            cancellationToken);

        return Ok(result);
    }
}

public sealed record CreatePostRequest(
    string? Text,
    IReadOnlyList<Guid>? StorageFileIds);