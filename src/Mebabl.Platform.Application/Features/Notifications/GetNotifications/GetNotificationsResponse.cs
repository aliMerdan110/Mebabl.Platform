namespace Mebabl.Platform.Application.Features.Notifications.GetNotifications;

public sealed record GetNotificationsResponse(
    Guid Id,
    Guid UserId,
    string Type,
    string Title,
    string Message,
    string? Data,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt);