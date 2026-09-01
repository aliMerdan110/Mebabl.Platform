using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.SdkAuth.Register;

public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterUserCommandHandler(
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

    public async Task<RegisterUserResponse> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
        {
            throw new UnauthorizedAccessException();
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var normalizedUsername = request.Username.Trim().ToUpperInvariant();

        var account = await _dbContext.Accounts
            .Include(x => x.ApplicationUsers)
            .FirstOrDefaultAsync(
                x => x.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (account is null)
        {
            account = new Account
            {
                Email = request.Email.Trim(),
                NormalizedEmail = normalizedEmail,
                Username = request.Username.Trim(),
                NormalizedUsername = normalizedUsername,
                PasswordHash = _passwordHasher.Hash(request.Password)
            };

            _dbContext.Accounts.Add(account);
        }

        var existsInApplication = account.ApplicationUsers.Any(x =>
            x.ApplicationId == _currentApplication.ApplicationId);

        if (existsInApplication)
        {
            throw new Exception("User already exists in this application.");
        }

        var applicationUser = new ApplicationUser
{
    Account = account,
    ApplicationId = _currentApplication.ApplicationId
};

var refreshToken = new RefreshToken
{
    ApplicationUser = applicationUser,
    Token = _jwtTokenGenerator.GenerateRefreshToken(),
    ExpiresAt = DateTime.UtcNow.AddDays(7)
};

applicationUser.RefreshTokens.Add(refreshToken);

_dbContext.ApplicationUsers.Add(applicationUser);

await _dbContext.SaveChangesAsync(cancellationToken);

var ownerRole = await _dbContext.Roles
    .FirstAsync(
        x => x.ApplicationId == applicationUser.ApplicationId &&
             x.Name == "Owner",
        cancellationToken);

_dbContext.ApplicationUserRoles.Add(
    new ApplicationUserRole
    {
        Id = Guid.NewGuid(),
        ApplicationUserId = applicationUser.Id,
        RoleId = ownerRole.Id
    });

await _dbContext.SaveChangesAsync(cancellationToken);

var roles = await _dbContext.ApplicationUserRoles
    .Where(x => x.ApplicationUserId == applicationUser.Id)
    .Select(x => x.Role.Name)
    .Distinct()
    .ToListAsync(cancellationToken);

var permissions = await _dbContext.ApplicationUserRoles
    .Where(x => x.ApplicationUserId == applicationUser.Id)
    .SelectMany(x => x.Role.RolePermissions)
    .Select(x => x.Permission.Code)
    .Distinct()
    .ToListAsync(cancellationToken);

var accessToken = _jwtTokenGenerator.GenerateAccessToken(
    account.Id,
    applicationUser.Id,
    applicationUser.ApplicationId,
    roles,
    permissions);

        return new RegisterUserResponse(
            account.Id,
            applicationUser.Id,
            accessToken,
            refreshToken.Token);
    }
}