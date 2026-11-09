using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.ApplicationAuthentication.DTOs;
using Mebabl.Platform.Application.Services.Jwt;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Login;

public sealed class ApplicationLoginCommandHandler
    : IRequestHandler<ApplicationLoginCommand, ApplicationAuthResponse>
{
    private const int MaximumApiKeyLength = 256;

    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtTokenGenerator _jwt;

    public ApplicationLoginCommandHandler(
        IApplicationDbContext dbContext,
        IJwtTokenGenerator jwt)
    {
        _dbContext = dbContext;
        _jwt = jwt;
    }

    public async Task<ApplicationAuthResponse> Handle(
        ApplicationLoginCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var suppliedApiKey = request.ApiKey;

        if (string.IsNullOrWhiteSpace(suppliedApiKey) ||
            suppliedApiKey.Length > MaximumApiKeyLength)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var apiKey = suppliedApiKey.Trim();

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
                ProjectId = x.Application.ProjectId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (credential is null ||
            credential.ApplicationId == Guid.Empty ||
            credential.ProjectId == Guid.Empty ||
            credential.CredentialId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // The public API key only creates a restricted public application token.
        // ProjectId is obtained from the persisted Application -> Project relation,
        // never from client-supplied data.
        var accessToken = _jwt.GeneratePublicApplicationToken(
            credential.ApplicationId,
            credential.ProjectId,
            credential.CredentialId);

        return new ApplicationAuthResponse(
            credential.ApplicationId,
            accessToken);
    }
}
