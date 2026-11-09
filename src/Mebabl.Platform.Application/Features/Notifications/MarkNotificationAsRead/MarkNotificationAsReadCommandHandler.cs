
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandHandler
    : IRequestHandler<MarkNotificationAsReadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public MarkNotificationAsReadCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var applicationId = _currentApplication.ApplicationId;

        if (!_currentApplication.IsAuthenticated ||
            applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Authenticated application credentials are required.");
        }

        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid notification ID is required.",
                nameof(request.Id));
        }

        // Only locate notifications belonging to this application
        // and exclude notifications that have already been deleted.
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.ApplicationId == applicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (notification is null)
        {
            throw new KeyNotFoundException(
                "Notification not found in this application.");
        }

        // Keep the operation idempotent.
        if (notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
