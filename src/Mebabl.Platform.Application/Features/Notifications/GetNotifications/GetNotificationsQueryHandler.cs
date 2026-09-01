using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Notifications.GetNotifications;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<
        GetNotificationsQuery,
        IReadOnlyList<GetNotificationsResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public GetNotificationsQueryHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task<IReadOnlyList<GetNotificationsResponse>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        return await _context.Notifications
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == _currentApplication.ApplicationId &&
                x.UserId == request.UserId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(request.Offset)
            .Take(request.Limit)
            .Select(x => new GetNotificationsResponse(
                x.Id,
                x.UserId,
                x.Type,
                x.Title,
                x.Message,
                x.Data,
                x.IsRead,
                x.ReadAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}