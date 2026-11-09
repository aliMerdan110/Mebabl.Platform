using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.ApplicationAuthentication.DTOs;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.TrustedLogin;

public sealed class ApplicationServerLoginCommandHandler
    : IRequestHandler<ApplicationServerLoginCommand, ApplicationAuthResponse>
{
    private const int MaximumApiKeyLength = 100;
    private const int MaximumApiSecretLength = 512;

    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IPasswordHasher _passwordHasher;

    public ApplicationServerLoginCommandHandler(
        IApplicationDbContext dbContext,
        IJwtTokenGenerator jwt,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _jwt = jwt;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApplicationAuthResponse> Handle(
        ApplicationServerLoginCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.ApiKey) ||
            request.ApiKey.Length > MaximumApiKeyLength ||
            string.IsNullOrWhiteSpace(request.ApiSecret) ||
            request.ApiSecret.Length > MaximumApiSecretLength)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var apiKey = request.ApiKey.Trim();

        var credential = await _dbContext.ApplicationCredentials
            .AsNoTracking()
            .Where(x =>
                x.ApiKey == apiKey &&
                x.IsActive &&
                !x.IsDeleted &&
                x.Application.IsActive &&
                !x.Application.IsDeleted &&
                x.Application.Project.IsActive &&
                !x.Application.Project.IsDeleted)
            .Select(x => new
            {
                CredentialId = x.Id,
                x.ApplicationId,
                ProjectId = x.Application.ProjectId,
                x.ApiSecretHash
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (credential is null ||
            string.IsNullOrWhiteSpace(credential.ApiSecretHash) ||
            !_passwordHasher.Verify(request.ApiSecret, credential.ApiSecretHash))
        {
            // Do not reveal whether the API key or secret was incorrect.
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var accessToken = _jwt.GenerateApplicationToken(
            credential.ApplicationId,
            credential.ProjectId,
            credential.CredentialId);

        return new ApplicationAuthResponse(
            credential.ApplicationId,
            accessToken);
    }
}
