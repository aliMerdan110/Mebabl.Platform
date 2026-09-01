using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.ApplicationAuthentication.DTOs;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Login;

public sealed class ApplicationLoginCommandHandler
    : IRequestHandler<ApplicationLoginCommand, ApplicationAuthResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IPasswordHasher _passwordHasher;

    public ApplicationLoginCommandHandler(
        IApplicationDbContext dbContext,
        IJwtTokenGenerator jwt,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _jwt = jwt;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApplicationAuthResponse> Handle(
        ApplicationLoginCommand request,
        CancellationToken cancellationToken)
    {
        var credential = await _dbContext.ApplicationCredentials
            .Include(x => x.Application)
            .FirstOrDefaultAsync(
                x =>
                    x.ApiKey == request.ApiKey &&
                    x.IsActive &&
                    x.Application.IsActive,
                cancellationToken);

        if (credential is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid credentials.");
        }

        var isValid = _passwordHasher.Verify(
            request.ApiSecret,
            credential.ApiSecretHash);

        if (!isValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid credentials.");
        }

        var token = _jwt.GenerateApplicationToken(
            credential.ApplicationId,
            credential.Id);

        return new ApplicationAuthResponse(
            credential.ApplicationId,
            token);
    }
}