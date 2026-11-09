
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Notifications.GetNotifications;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<
        GetNotificationsQuery,
        IReadOnlyList<GetNotificationsResponse>>
{
    private const int MaxPageSize = 100;

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

        if (request.Offset < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Offset),
                "Offset cannot be negative.");
        }

        if (request.Limit < 1 || request.Limit > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Limit),
                $"Limit must be between 1 and {MaxPageSize}.");
        }

        // Verify that the requested user belongs to the current
        // application and that the account is still usable.
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

        return await _context.Notifications
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == applicationId &&
                x.UserId == request.UserId &&
                !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
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
