
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Notifications.DeleteNotification;

public sealed class DeleteNotificationCommandHandler
    : IRequestHandler<DeleteNotificationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public DeleteNotificationCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task Handle(
        DeleteNotificationCommand request,
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

        // Restrict the operation to a notification owned by this
        // application and exclude notifications already deleted.
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

        // Soft delete.
        notification.IsDeleted = true;
        notification.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
