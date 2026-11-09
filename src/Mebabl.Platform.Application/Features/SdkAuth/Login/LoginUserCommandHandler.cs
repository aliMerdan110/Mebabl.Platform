using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.SdkAuth.Login;

public sealed class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    private static readonly TimeSpan RefreshTokenLifetime =
        TimeSpan.FromDays(7);

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginUserResponse> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentApplication.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Application authentication is required.");
        }

        var applicationId = _currentApplication.ApplicationId;

        if (applicationId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Email) ||
            request.Email.Length > 256 ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var now = DateTime.UtcNow;

        // Load the owning application so the user token is issued with the
        // ProjectId resolved from the persisted Application -> Project relation.
        var user = await _dbContext.ApplicationUsers
            .Include(x => x.Application)
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.NormalizedEmail == normalizedEmail &&
                    !x.IsDeleted &&
                    !x.Account.IsDeleted &&
                    x.Application.IsActive &&
                    !x.Application.IsDeleted &&
                    x.Application.Project.IsActive &&
                    !x.Application.Project.IsDeleted,
                cancellationToken);

        if (user is null ||
            !user.IsActive ||
            (user.LockoutEnabled &&
             user.LockoutEnd.HasValue &&
             user.LockoutEnd.Value > now) ||
            string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var projectId = user.Application.ProjectId;
        if (projectId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "The application is not associated with a valid project.");
        }

        if (string.IsNullOrWhiteSpace(user.SecurityStamp))
        {
            user.SecurityStamp = Guid.NewGuid().ToString("N");
        }

        user.LastLoginAt = now;

        var roles = await _dbContext.ApplicationUserRoles
            .AsNoTracking()
            .Where(x =>
                x.ApplicationUserId == user.Id &&
                x.Role.ApplicationId == applicationId &&
                x.Role.IsActive &&
                !x.Role.IsDeleted)
            .Select(x => x.Role.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        var permissions = await _dbContext.ApplicationUserRoles
            .AsNoTracking()
            .Where(x =>
                x.ApplicationUserId == user.Id &&
                x.Role.ApplicationId == applicationId &&
                x.Role.IsActive &&
                !x.Role.IsDeleted)
            .SelectMany(x => x.Role.RolePermissions)
            .Where(x =>
                x.Permission.ApplicationId == applicationId &&
                x.Permission.IsActive &&
                !x.Permission.IsDeleted)
            .Select(x => x.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var rawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            ApplicationUserId = user.Id,
            Token = null,
            TokenHash = RefreshTokenHashing.Hash(rawRefreshToken),
            ExpiresAt = now.Add(RefreshTokenLifetime)
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.AccountId,
            user.Id,
            applicationId,
            projectId,
            user.SecurityStamp,
            roles,
            permissions);

        return new LoginUserResponse(
            user.AccountId,
            user.Id,
            accessToken,
            rawRefreshToken);
    }
}
