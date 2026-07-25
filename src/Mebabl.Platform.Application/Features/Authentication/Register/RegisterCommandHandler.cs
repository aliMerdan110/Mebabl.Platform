using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Features.Authentication.DTOs;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities;
using Mebabl.Platform.Domain.Entities.Identity;

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
        var emailExists =
            await _dbContext.Accounts
                .AnyAsync(
                    x => x.Email == request.Email,
                    cancellationToken);

        if (emailExists)
        {
            throw new Exception("Email already exists");
        }


        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.TenantName,
            Code = request.TenantName.ToLower()
        };


        var application = new Domain.Entities.Application
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Name = request.ApplicationName,
            Code = request.ApplicationName.ToLower()
        };


        var account = new Account
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Email = request.Email,
            NormalizedEmail = request.Email.ToUpper(),
            Username = request.Username,
            NormalizedUsername = request.Username.ToUpper(),
            PasswordHash =
                _passwordHasher.Hash(request.Password)
        };


        var applicationUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            ApplicationId = application.Id,
            AccountId = account.Id
        };


        var refreshToken =
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = applicationUser.Id,
                Token = _jwtTokenGenerator.GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };


        var accessToken =
            _jwtTokenGenerator.GenerateAccessToken(
                account.Id);


        _dbContext.Tenants.Add(tenant);
        _dbContext.Applications.Add(application);
        _dbContext.Accounts.Add(account);
        _dbContext.ApplicationUsers.Add(applicationUser);
        _dbContext.RefreshTokens.Add(refreshToken);


        await _dbContext.SaveChangesAsync(
            cancellationToken);


        return new AuthResponse(
            accessToken,
            refreshToken.Token);
    }
}