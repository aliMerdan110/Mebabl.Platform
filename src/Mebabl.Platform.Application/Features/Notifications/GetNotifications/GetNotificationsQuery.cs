using MediatR;

namespace Mebabl.Platform.Application.Features.Notifications.GetNotifications;

public sealed record GetNotificationsQuery(
    Guid UserId,
    int Offset = 0,
    int Limit = 50
) : IRequest<IReadOnlyList<GetNotificationsResponse>>;