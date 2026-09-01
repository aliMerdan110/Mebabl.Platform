namespace Mebabl.Platform.Application.Features.Notifications.CreateNotification;

public sealed record CreateNotificationResponse(
    Guid Id,
    Guid UserId,
    string Type,
    string Title,
    string Message,
    string? Data,
    bool IsRead,
    DateTime CreatedAt);