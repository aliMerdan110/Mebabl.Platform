using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.Users;

public sealed class GetApplicationUsersQueryHandler
    : IRequestHandler<
        GetApplicationUsersQuery,
        IReadOnlyList<ApplicationUserDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetApplicationUsersQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<IReadOnlyList<ApplicationUserDto>> Handle(
        GetApplicationUsersQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Developer authentication is required.");
        }

        // ------------------------------------------------------------
        // Verify that the application belongs to the current developer
        // ------------------------------------------------------------

        var applicationExists =
            await _dbContext.Applications
                .AsNoTracking()
                .AnyAsync(
                    application =>
                        application.Id == request.ApplicationId &&
                        application.DeveloperId ==
                            _currentDeveloper.DeveloperId &&
                        !application.IsDeleted,
                    cancellationToken);

        if (!applicationExists)
        {
            throw new KeyNotFoundException(
                "Application was not found.");
        }

        // ------------------------------------------------------------
        // Get users belonging ONLY to this application
        // ------------------------------------------------------------

        var users =
            await _dbContext.ApplicationUsers
                .AsNoTracking()
                .Include(applicationUser =>
                    applicationUser.Account)
                .Where(
                    applicationUser =>
                        applicationUser.ApplicationId ==
                            request.ApplicationId &&
                        !applicationUser.IsDeleted)
                .OrderByDescending(
                    applicationUser =>
                        applicationUser.CreatedAt)
                .Select(
                    applicationUser =>
                        new ApplicationUserDto(
                            applicationUser.Id,
                            applicationUser.Account.Email,
                            applicationUser.Account.Username,
                            "password",
                            applicationUser.CreatedAt,
                            applicationUser.LastLoginAt,
                            applicationUser.IsActive &&
                            applicationUser.Account.IsActive))
                .ToListAsync(cancellationToken);

        return users;
    }
}