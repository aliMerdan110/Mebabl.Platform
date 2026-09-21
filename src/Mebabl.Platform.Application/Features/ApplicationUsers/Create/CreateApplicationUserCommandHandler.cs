using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Applications.Users.CreateApplicationUser;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Application.Features.ApplicationUsers.Create;

public sealed class CreateApplicationUserCommandHandler
    : IRequestHandler<CreateApplicationUserCommand, CreateApplicationUserResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;
    private readonly IPasswordHasher _passwordHasher;

    public CreateApplicationUserCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateApplicationUserResponse> Handle(
        CreateApplicationUserCommand request,
        CancellationToken cancellationToken)
    {
        // ينشئ حساب المستخدم وملفه الشخصي ضمن التطبيق المملوك للمطور.
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Developer authentication is required.");
        }

        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.ApplicationId &&
                    x.DeveloperId == _currentDeveloper.DeveloperId &&
                    !x.IsDeleted,
                cancellationToken);

        if (application is null)
        {
            throw new KeyNotFoundException(
                "Application was not found.");
        }

        var email = request.Email.Trim();
        var username = request.Username.Trim();
        var displayName = request.DisplayName.Trim();

        var normalizedEmail = email.ToUpperInvariant();
        var normalizedUsername = username.ToUpperInvariant();

        var emailExists = await _dbContext.ApplicationUsers
            .AnyAsync(
                x =>
                    x.ApplicationId == request.ApplicationId &&
                    x.NormalizedEmail == normalizedEmail &&
                    !x.IsDeleted,
                cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "User with this email already exists in this application.");
        }

        var usernameExists = await _dbContext.ApplicationUsers
            .AnyAsync(
                x =>
                    x.ApplicationId == request.ApplicationId &&
                    x.NormalizedUsername == normalizedUsername &&
                    !x.IsDeleted,
                cancellationToken);

        if (usernameExists)
        {
            throw new InvalidOperationException(
                "User with this username already exists in this application.");
        }

        var account = new Account();

        var profile = new Profile
        {
            Account = account,
            Username = username,
            DisplayName = displayName
        };

        account.Profile = profile;

        var applicationUser = new ApplicationUser
        {
            Account = account,
            ApplicationId = request.ApplicationId,

            Email = email,
            NormalizedEmail = normalizedEmail,

            Username = username,
            NormalizedUsername = normalizedUsername,

            PasswordHash = _passwordHasher.Hash(request.Password),
            SecurityStamp = Guid.NewGuid().ToString(),

            EmailConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            LockoutEnd = null,
            AccessFailedCount = 0,
            IsActive = true
        };

        _dbContext.Accounts.Add(account);
        _dbContext.ApplicationUsers.Add(applicationUser);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateApplicationUserResponse(
            applicationUser.Id,
            applicationUser.AccountId,
            applicationUser.ApplicationId,
            applicationUser.Email,
            applicationUser.Username,
            profile.DisplayName,
            applicationUser.IsActive,
            applicationUser.CreatedAt);
    }
}