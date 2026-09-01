using MediatR;
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
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var notification = new Notification
        {
            ApplicationId = _currentApplication.ApplicationId,
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