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

        var users =
            await _dbContext.ApplicationUsers
                .AsNoTracking()
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
                            applicationUser.Email,
                            applicationUser.Username,
                            "password",
                            applicationUser.CreatedAt,
                            applicationUser.LastLoginAt,
                            applicationUser.IsActive))
                .ToListAsync(cancellationToken);

        return users;
    }
}