using MediatR;

namespace Mebabl.Platform.Application.Features.Notifications.CreateNotification;

public sealed record CreateNotificationCommand(
    Guid UserId,
    string Type,
    string Title,
    string Message,
    string? Data
) : IRequest<CreateNotificationResponse>;