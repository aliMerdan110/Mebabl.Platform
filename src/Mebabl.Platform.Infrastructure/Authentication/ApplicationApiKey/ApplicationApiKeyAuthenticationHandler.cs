using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Infrastructure.Authentication.ApplicationApiKey;

public sealed class ApplicationApiKeyAuthenticationHandler
    : AuthenticationHandler<ApplicationApiKeyAuthenticationOptions>
{
    private readonly IApplicationDbContext _dbContext;

    public ApplicationApiKeyAuthenticationHandler(
        IOptionsMonitor<ApplicationApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IApplicationDbContext dbContext)
        : base(options, logger, encoder, clock)
    {
        _dbContext = dbContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(
                "X-Application-Id",
                out var applicationIdHeader))
        {
            return AuthenticateResult.NoResult();
        }

        if (!Request.Headers.TryGetValue(
                "X-Api-Key",
                out var apiKeyHeader))
        {
            return AuthenticateResult.NoResult();
        }

        if (!Guid.TryParse(
                applicationIdHeader.ToString(),
                out var applicationId))
        {
            return AuthenticateResult.Fail(
                "Invalid application id.");
        }

        var apiKey = apiKeyHeader.ToString();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return AuthenticateResult.Fail(
                "Invalid API key.");
        }

        var credential = await _dbContext.ApplicationCredentials
            .AsNoTracking()
            .Include(x => x.Application)
            .FirstOrDefaultAsync(
                x =>
                    x.ApiKey == apiKey &&
                    x.ApplicationId == applicationId &&
                    x.IsActive &&
                    x.Application.IsActive,
                Context.RequestAborted);

        if (credential is null)
        {
            return AuthenticateResult.Fail(
                "Invalid application credentials.");
        }

        var claims = new[]
        {
            new Claim(
                "applicationId",
                credential.ApplicationId.ToString()),

            new Claim(
                "credentialId",
                credential.Id.ToString()),

            new Claim(
                "type",
                "application")
        };

        var identity = new ClaimsIdentity(
            claims,
            Scheme.Name);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}