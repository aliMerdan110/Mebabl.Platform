
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Notifications.MarkAllNotificationsAsRead;

public sealed class MarkAllNotificationsAsReadCommandHandler
    : IRequestHandler<MarkAllNotificationsAsReadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public MarkAllNotificationsAsReadCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;

        if (!_currentApplication.IsAuthenticated ||
            applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Authenticated application credentials are required.");
        }

        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid user ID is required.",
                nameof(request.UserId));
        }

        // Ensure the requested user belongs to this application.
        var userExists = await _context.ApplicationUsers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == request.UserId &&
                    x.ApplicationId == applicationId &&
                    x.IsActive &&
                    !x.IsDeleted &&
                    !x.Account.IsDeleted &&
                    x.Application.IsActive &&
                    !x.Application.IsDeleted,
                cancellationToken);

        if (!userExists)
        {
            throw new KeyNotFoundException(
                "User not found in this application.");
        }

        // Only update unread, non-deleted notifications for this
        // user within the authenticated application's boundary.
        var notifications = await _context.Notifications
            .Where(x =>
                x.ApplicationId == applicationId &&
                x.UserId == request.UserId &&
                !x.IsRead &&
                !x.IsDeleted)
            .ToListAsync(cancellationToken);

        if (notifications.Count == 0)
        {
            return;
        }

        var readAt = DateTime.UtcNow;

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = readAt;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
