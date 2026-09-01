using MediatR;

namespace Mebabl.Platform.Application.Features.Notifications.MarkAllNotificationsAsRead;

public sealed record MarkAllNotificationsAsReadCommand(
    Guid UserId
) : IRequest;