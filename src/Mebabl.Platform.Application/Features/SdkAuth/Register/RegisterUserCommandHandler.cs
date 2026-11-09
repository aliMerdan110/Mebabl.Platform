using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Options;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Services.Email;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Application.Services.PasswordReset;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.SdkAuth.Register;

public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
    private static readonly TimeSpan EmailVerificationTokenLifetime =
        TimeSpan.FromMinutes(30);

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailService _emailService;
    private readonly IPasswordResetTokenService _tokenService;
    private readonly ConsoleOptions _consoleOptions;

    public RegisterUserCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailService emailService,
        IPasswordResetTokenService tokenService,
        IOptions<ConsoleOptions> consoleOptions)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailService = emailService;
        _tokenService = tokenService;
        _consoleOptions = consoleOptions.Value;
    }

    public async Task<RegisterUserResponse> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentApplication.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Application authentication is required.");
        }

        var applicationId = _currentApplication.ApplicationId;
        if (applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "Application authentication is required.");
        }

        // Resolve ProjectId from the trusted database relationship, not from
        // request data. The application token is separately validated against it.
        var projectId = await _dbContext.Applications
            .AsNoTracking()
            .Where(x =>
                x.Id == applicationId &&
                x.IsActive &&
                !x.IsDeleted &&
                x.Project.IsActive &&
                !x.Project.IsDeleted)
            .Select(x => x.ProjectId)
            .FirstOrDefaultAsync(cancellationToken);

        if (projectId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The application or its project is unavailable.");
        }

        if (string.IsNullOrWhiteSpace(request.Email) ||
            request.Email.Length > 256 ||
            string.IsNullOrWhiteSpace(request.Username) ||
            request.Username.Length > 100 ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Registration details are invalid.");
        }

        var email = request.Email.Trim();
        var username = request.Username.Trim();
        var normalizedEmail = email.ToUpperInvariant();
        var normalizedUsername = username.ToUpperInvariant();

        var emailExists = await _dbContext.ApplicationUsers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.NormalizedEmail == normalizedEmail &&
                    !x.IsDeleted,
                cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "User with this email already exists in this application.");
        }

        var usernameExists = await _dbContext.ApplicationUsers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.NormalizedUsername == normalizedUsername &&
                    !x.IsDeleted,
                cancellationToken);

        if (usernameExists)
        {
            throw new InvalidOperationException(
                "User with this username already exists in this application.");
        }

        var ownerRole = await _dbContext.Roles
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == applicationId &&
                    x.Name == "Owner" &&
                    x.IsActive &&
                    !x.IsDeleted,
                cancellationToken);

        if (ownerRole is null)
        {
            throw new InvalidOperationException(
                "The default Owner role is not configured for this application.");
        }

        var now = DateTime.UtcNow;
        var account = new Account();
        _dbContext.Accounts.Add(account);

        var applicationUser = new ApplicationUser
        {
            Account = account,
            ApplicationId = applicationId,
            Email = email,
            NormalizedEmail = normalizedEmail,
            Username = username,
            NormalizedUsername = normalizedUsername,
            PasswordHash = _passwordHasher.Hash(request.Password),
            SecurityStamp = Guid.NewGuid().ToString("N"),
            EmailConfirmed = false,
            IsActive = true
        };

        _dbContext.ApplicationUsers.Add(applicationUser);

        var rawRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            ApplicationUser = applicationUser,
            Token = null,
            TokenHash = RefreshTokenHashing.Hash(rawRefreshToken),
            ExpiresAt = now.Add(RefreshTokenLifetime)
        };
        applicationUser.RefreshTokens.Add(refreshToken);

        _dbContext.ApplicationUserRoles.Add(
            new ApplicationUserRole
            {
                Id = Guid.NewGuid(),
                ApplicationUser = applicationUser,
                RoleId = ownerRole.Id
            });

        await _dbContext.SaveChangesAsync(cancellationToken);

        var rawVerificationToken = _tokenService.GenerateToken();
        var verificationToken = new ApplicationUserEmailVerificationToken
        {
            ApplicationUserId = applicationUser.Id,
            TokenHash = _tokenService.HashToken(rawVerificationToken),
            ExpiresAt = now.Add(EmailVerificationTokenLifetime)
        };

        _dbContext.ApplicationUserEmailVerificationTokens.Add(verificationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var consoleUrl = _consoleOptions.BaseUrl.TrimEnd('/');
        var verificationUrl =
            $"{consoleUrl}/verify-email?token={Uri.EscapeDataString(rawVerificationToken)}";

        await _emailService.SendAsync(
            applicationUser.Email,
            "Verify your email",
            $"""
            Hello,

            Thank you for creating your account.

            Please verify your email address using the following link:

            {verificationUrl}

            This link will expire in {(int)EmailVerificationTokenLifetime.TotalMinutes} minutes.

            If you did not create this account, you can safely ignore this email.

            Mebabl Platform
            """,
            cancellationToken);

        var roles = await _dbContext.ApplicationUserRoles
            .AsNoTracking()
            .Where(x =>
                x.ApplicationUserId == applicationUser.Id &&
                x.Role.ApplicationId == applicationId &&
                x.Role.IsActive &&
                !x.Role.IsDeleted)
            .Select(x => x.Role.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        var permissions = await _dbContext.ApplicationUserRoles
            .AsNoTracking()
            .Where(x =>
                x.ApplicationUserId == applicationUser.Id &&
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

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            account.Id,
            applicationUser.Id,
            applicationId,
            projectId,
            applicationUser.SecurityStamp,
            roles,
            permissions);

        return new RegisterUserResponse(
            account.Id,
            applicationUser.Id,
            accessToken,
            rawRefreshToken);
    }
}
