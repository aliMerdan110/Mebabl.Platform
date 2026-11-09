
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Notifications;

namespace Mebabl.Platform.Application.Features.Notifications.CreateNotification;

public sealed class CreateNotificationCommandHandler
    : IRequestHandler<CreateNotificationCommand, CreateNotificationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentApplication _currentApplication;

    public CreateNotificationCommandHandler(
        IApplicationDbContext context,
        ICurrentApplication currentApplication)
    {
        _context = context;
        _currentApplication = currentApplication;
    }

    public async Task<CreateNotificationResponse> Handle(
        CreateNotificationCommand request,
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
                "A valid recipient user ID is required.",
                nameof(request.UserId));
        }

        // Ensure the recipient belongs to this application and
        // that the user, account, and application are usable.
        var recipientExists = await _context.ApplicationUsers
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

        if (!recipientExists)
        {
            throw new KeyNotFoundException(
                "Recipient user not found in this application.");
        }

        var notification = new Notification
        {
            ApplicationId = applicationId,
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            Data = request.Data,
            IsRead = false
        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateNotificationResponse(
            notification.Id,
            notification.UserId,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.Data,
            notification.IsRead,
            notification.CreatedAt);
    }
}
