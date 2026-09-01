using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Options;
using Mebabl.Platform.Application.Services.Email;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Application.Services.PasswordReset;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.SdkAuth.Register;

public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
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
        var applicationId = _currentApplication.ApplicationId;

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
            x.ApplicationId == applicationId);

        if (existsInApplication)
        {
            throw new Exception("User already exists in this application.");
        }

        var applicationUser = new ApplicationUser
        {
            Account = account,
            ApplicationId = applicationId
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
                x =>
                    x.ApplicationId == applicationUser.ApplicationId &&
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

        // -------------------------------------------------
        // Email Verification
        // -------------------------------------------------

        var rawToken = _tokenService.GenerateToken();
        var tokenHash = _tokenService.HashToken(rawToken);

        var verificationToken = new ApplicationUserEmailVerificationToken
        {
            UserId = applicationUser.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        _dbContext.ApplicationUserEmailVerificationTokens
            .Add(verificationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var consoleUrl = _consoleOptions.BaseUrl.TrimEnd('/');

        var verificationUrl =
            $"{consoleUrl}/sdk/verify-email?token={Uri.EscapeDataString(rawToken)}";

        await _emailService.SendAsync(
            account.Email,
            "Verify your email",
            $"""
            Hello,

            Thank you for creating your account.

            Please verify your email address using the following link:

            {verificationUrl}

            This link will expire in 30 minutes.

            If you did not create this account, you can safely ignore this email.

            Mebabl Platform
            """,
            cancellationToken);

        // -------------------------------------------------
        // Roles & Permissions
        // -------------------------------------------------

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