using MediatR;

namespace Mebabl.Platform.Application.Features.Notifications.DeleteNotification;

public sealed record DeleteNotificationCommand(
    Guid Id
) : IRequest;