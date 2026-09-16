using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.SdkAuth.Login;

public sealed class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
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
        var applicationId = _currentApplication.ApplicationId;

        if (applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Application authentication is required.");
        }

        var normalizedEmail = request.Email
            .Trim()
            .ToUpperInvariant();

        var applicationUser = await _dbContext.ApplicationUsers
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.NormalizedEmail == normalizedEmail &&
                    !x.IsDeleted,
                cancellationToken);

        if (applicationUser is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        if (!applicationUser.IsActive)
        {
            throw new UnauthorizedAccessException(
                "User account is inactive.");
        }

        if (string.IsNullOrWhiteSpace(
                applicationUser.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var passwordValid = _passwordHasher.Verify(
            request.Password,
            applicationUser.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        applicationUser.LastLoginAt = DateTime.UtcNow;

        var roles = await _dbContext.ApplicationUserRoles
            .Where(
                x =>
                    x.ApplicationUserId ==
                    applicationUser.Id)
            .Select(x => x.Role.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        var permissions = await _dbContext.ApplicationUserRoles
            .Where(
                x =>
                    x.ApplicationUserId ==
                    applicationUser.Id)
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var refreshToken = new RefreshToken
        {
            ApplicationUserId = applicationUser.Id,
            Token = _jwtTokenGenerator.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _dbContext.RefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            applicationUser.AccountId,
            applicationUser.Id,
            applicationUser.ApplicationId,
            roles,
            permissions);

        return new LoginUserResponse(
            applicationUser.AccountId,
            applicationUser.Id,
            accessToken,
            refreshToken.Token);
    }
}