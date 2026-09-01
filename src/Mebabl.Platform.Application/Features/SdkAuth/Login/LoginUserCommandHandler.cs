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
        // ------------------------------------------------------------
        // Validate Application
        // ------------------------------------------------------------

       

        var applicationId = _currentApplication.ApplicationId;

        // ------------------------------------------------------------
        // Find Account
        // ------------------------------------------------------------

        var normalizedEmail = request.Email
            .Trim()
            .ToUpperInvariant();

        var account = await _dbContext.Accounts
            .Include(x => x.ApplicationUsers)
            .FirstOrDefaultAsync(
                x => x.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (account is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        // ------------------------------------------------------------
        // Verify Password
        // ------------------------------------------------------------

        if (string.IsNullOrWhiteSpace(account.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var passwordValid = _passwordHasher.Verify(
            request.Password,
            account.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        // ------------------------------------------------------------
        // Application User
        // ------------------------------------------------------------

        var applicationUser = account.ApplicationUsers
            .FirstOrDefault(x =>
                x.ApplicationId == applicationId);

        if (applicationUser is null)
        {
            throw new UnauthorizedAccessException(
                "User is not registered in this application.");
        }

        // ------------------------------------------------------------
        // Roles
        // ------------------------------------------------------------

        var roles = await _dbContext.ApplicationUserRoles
            .Where(x =>
                x.ApplicationUserId == applicationUser.Id)
            .Select(x => x.Role.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        // ------------------------------------------------------------
        // Permissions
        // ------------------------------------------------------------

        var permissions = await _dbContext.ApplicationUserRoles
            .Where(x =>
                x.ApplicationUserId == applicationUser.Id)
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        // ------------------------------------------------------------
        // Refresh Token
        // ------------------------------------------------------------

        var refreshToken = new RefreshToken
        {
            ApplicationUserId = applicationUser.Id,
            Token = _jwtTokenGenerator.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _dbContext.RefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // ------------------------------------------------------------
        // Access Token
        // ------------------------------------------------------------

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            account.Id,
            applicationUser.Id,
            applicationUser.ApplicationId,
            roles,
            permissions);

        return new LoginUserResponse(
            account.Id,
            applicationUser.Id,
            accessToken,
            refreshToken.Token);
    }
}