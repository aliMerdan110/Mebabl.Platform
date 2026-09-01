using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.Realtime.Channels.CreateChannel;
using Mebabl.Platform.Application.Features.Realtime.Channels.GetChannels;
using Mebabl.Platform.Application.Features.Realtime.Channels.GetChannelById;
using Mebabl.Platform.Application.Features.Realtime.Channels.UpdateChannel;
using Mebabl.Platform.Application.Features.Realtime.Channels.DeleteChannel;

using Mebabl.Platform.Application.Features.Realtime.Events.PublishEvent;
using Mebabl.Platform.Application.Features.Realtime.Events.GetChannelEvents;

namespace Mebabl.Platform.API.Controllers;

[Authorize]
[Route("api/realtime")]
public sealed class RealtimeController : BaseApiController
{
    // Channels

    [HttpPost("channels")]
    public async Task<IActionResult> CreateChannel(
        CreateChannelCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }


    [HttpGet("channels")]
    public async Task<IActionResult> GetChannels(
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetChannelsQuery(),
            cancellationToken);

        return Ok(result);
    }


    [HttpGet("channels/{id:guid}")]
    public async Task<IActionResult> GetChannelById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetChannelByIdQuery(id),
            cancellationToken);

        return Ok(result);
    }


    [HttpPut("channels/{id:guid}")]
    public async Task<IActionResult> UpdateChannel(
        Guid id,
        UpdateChannelCommand command,
        CancellationToken cancellationToken)
    {
        var request = command with
        {
            Id = id
        };

        await Sender.Send(
            request,
            cancellationToken);

        return NoContent();
    }


    [HttpDelete("channels/{id:guid}")]
    public async Task<IActionResult> DeleteChannel(
        Guid id,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new DeleteChannelCommand(id),
            cancellationToken);

        return NoContent();
    }


    // Events

    [HttpPost("channels/{channelId:guid}/events")]
    public async Task<IActionResult> PublishEvent(
        Guid channelId,
        JsonDocument payload,
        [FromQuery] string name,
        CancellationToken cancellationToken)
    {
        var command = new PublishEventCommand(
            channelId,
            name,
            payload);

        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }


    [HttpGet("channels/{channelId:guid}/events")]
    public async Task<IActionResult> GetChannelEvents(
        Guid channelId,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(
            new GetChannelEventsQuery(
                channelId,
                offset,
                limit),
            cancellationToken);

        return Ok(result);
    }
}