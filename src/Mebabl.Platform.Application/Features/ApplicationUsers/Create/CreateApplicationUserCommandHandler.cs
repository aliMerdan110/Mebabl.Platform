using Microsoft.EntityFrameworkCore;
using MediatR;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Identity;
using Mebabl.Platform.Application.Services.Password;

namespace Mebabl.Platform.Application.Features.Applications.Users.CreateApplicationUser;

public sealed class CreateApplicationUserCommandHandler
    : IRequestHandler<
        CreateApplicationUserCommand,
        CreateApplicationUserResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentDeveloper _currentDeveloper;
    private readonly IPasswordHasher _passwordHasher;

    public CreateApplicationUserCommandHandler(
        IApplicationDbContext db,
        ICurrentDeveloper currentDeveloper,
        IPasswordHasher passwordHasher)
    {
        _db = db;
        _currentDeveloper = currentDeveloper;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateApplicationUserResponse> Handle(
        CreateApplicationUserCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException(
                "Developer authentication is required.");

        // ------------------------------------------------------------
        // Verify Application ownership
        // ------------------------------------------------------------

        var applicationExists = await _db.Applications
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == request.ApplicationId &&
                    x.DeveloperId == _currentDeveloper.DeveloperId &&
                    !x.IsDeleted,
                cancellationToken);

        if (!applicationExists)
            throw new KeyNotFoundException(
                "Application was not found.");

        // ------------------------------------------------------------
        // Normalize data
        // ------------------------------------------------------------

        var email = request.Email.Trim();
        var username = request.Username.Trim();
        var displayName = request.DisplayName.Trim();

        var normalizedEmail =
            email.ToUpperInvariant();

        var normalizedUsername =
            username.ToUpperInvariant();

        // ------------------------------------------------------------
        // Check email inside this application
        // ------------------------------------------------------------

        var emailExists = await _db.ApplicationUsers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.ApplicationId == request.ApplicationId &&
                    !x.IsDeleted &&
                    x.Account.NormalizedEmail == normalizedEmail &&
                    !x.Account.IsDeleted,
                cancellationToken);

        if (emailExists)
            throw new InvalidOperationException(
                "A user with this email already exists in this application.");

        // ------------------------------------------------------------
        // Check username inside this application
        // ------------------------------------------------------------

        var usernameExists = await _db.ApplicationUsers
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.ApplicationId == request.ApplicationId &&
                    !x.IsDeleted &&
                    x.Account.NormalizedUsername == normalizedUsername &&
                    !x.Account.IsDeleted,
                cancellationToken);

        if (usernameExists)
            throw new InvalidOperationException(
                "A user with this username already exists in this application.");

        // ------------------------------------------------------------
        // Create Account
        // ------------------------------------------------------------

        var account = new Account
        {
            Id = Guid.NewGuid(),

            Email = email,
            NormalizedEmail = normalizedEmail,

            Username = username,
            NormalizedUsername = normalizedUsername,

            PasswordHash =
                _passwordHasher.Hash(request.Password),

            SecurityStamp = Guid.NewGuid().ToString(),

            EmailConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            LockoutEnd = null,
            AccessFailedCount = 0,

            IsActive = true
        };

        // ------------------------------------------------------------
        // Create Profile
        // ------------------------------------------------------------

        var profile = new Profile
        {
            Id = Guid.NewGuid(),

            AccountId = account.Id,

            Username = username,
            DisplayName = displayName
        };

        // ------------------------------------------------------------
        // Create ApplicationUser
        // ------------------------------------------------------------

        var applicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),

            ApplicationId = request.ApplicationId,

            AccountId = account.Id,

            IsActive = true,

            LastLoginAt = null
        };

        // ------------------------------------------------------------
        // Relationships
        // ------------------------------------------------------------

        account.Profile = profile;

        account.ApplicationUsers.Add(
            applicationUser);

        // ------------------------------------------------------------
        // Persist
        // ------------------------------------------------------------

        _db.Accounts.Add(account);

        await _db.SaveChangesAsync(
            cancellationToken);

        // ------------------------------------------------------------
        // Response
        // ------------------------------------------------------------

        return new CreateApplicationUserResponse(
            applicationUser.Id,
            applicationUser.ApplicationId,
            account.Id,
            account.Email,
            account.Username,
            profile.DisplayName,
            applicationUser.IsActive,
            applicationUser.CreatedAt
        );
    }
}