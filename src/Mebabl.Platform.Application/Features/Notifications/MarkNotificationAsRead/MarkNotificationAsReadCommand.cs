using MediatR;

namespace Mebabl.Platform.Application.Features.Notifications.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(
    Guid Id
) : IRequest;