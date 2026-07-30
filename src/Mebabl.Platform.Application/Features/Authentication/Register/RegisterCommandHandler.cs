using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Authentication.DTOs;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Domain.Entities.Identity;
using RefreshTokenEntity = Mebabl.Platform.Domain.Entities.Identity.RefreshToken;

namespace Mebabl.Platform.Application.Features.Authentication.Register;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        var normalizedUsername = request.Username.Trim().ToUpperInvariant();

        var emailExists = await _dbContext.Accounts.AnyAsync(
            x => x.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (emailExists)
        {
            throw new Exception("Email already exists");
        }

        var usernameExists = await _dbContext.Accounts.AnyAsync(
            x => x.NormalizedUsername == normalizedUsername,
            cancellationToken);

        if (usernameExists)
        {
            throw new Exception("Username already exists");
        }

        // إنشاء Tenant
        var tenant = new Tenant
        {
            Name = request.TenantName,
            Code = request.TenantName.Trim().ToLowerInvariant()
        };

        // إنشاء التطبيق وربطه بالـ Tenant
        var application = new PlatformApplication
        {
            Tenant = tenant,
            Name = request.ApplicationName,
            Code = request.ApplicationName.Trim().ToLowerInvariant()
        };

        // إنشاء الحساب وربطه بالـ Tenant
        var account = new Account
        {
            Tenant = tenant,
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            Username = request.Username.Trim(),
            NormalizedUsername = normalizedUsername,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        // إنشاء مستخدم التطبيق
        var applicationUser = new ApplicationUser
        {
            Account = account,
            Application = application
        };

        // إنشاء Refresh Token
        var refreshToken = new RefreshTokenEntity
        {
            ApplicationUser = applicationUser,
            Token = _jwtTokenGenerator.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // ربط الكيانات (اختياري لكنه يجعل الرسم البياني واضحاً)
        tenant.Applications.Add(application);
        tenant.Accounts.Add(account);

        application.Users.Add(applicationUser);

        account.ApplicationUsers.Add(applicationUser);

        applicationUser.RefreshTokens.Add(refreshToken);

        // إضافة الكيان الجذر فقط
        _dbContext.Tenants.Add(tenant);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // إنشاء Access Token بعد حفظ البيانات
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
    account.Id,
    applicationUser.Id,
    applicationUser.ApplicationId,
    account.TenantId);

        return new AuthResponse(
            application.Id,
            accessToken,
            refreshToken.Token);
    }
}