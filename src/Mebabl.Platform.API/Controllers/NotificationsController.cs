using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mebabl.Platform.Application.Features.Notifications.CreateNotification;
using Mebabl.Platform.Application.Features.Notifications.GetNotifications;
using Mebabl.Platform.Application.Features.Notifications.MarkNotificationAsRead;
using Mebabl.Platform.Application.Features.Notifications.MarkAllNotificationsAsRead;
using Mebabl.Platform.Application.Features.Notifications.DeleteNotification;

namespace Mebabl.Platform.API.Controllers;

[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            command,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] Guid userId,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(
            new GetNotificationsQuery(
                userId,
                offset,
                limit),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new MarkNotificationAsReadCommand(id),
            cancellationToken);

        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new MarkAllNotificationsAsReadCommand(userId),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await Sender.Send(
            new DeleteNotificationCommand(id),
            cancellationToken);

        return NoContent();
    }
}