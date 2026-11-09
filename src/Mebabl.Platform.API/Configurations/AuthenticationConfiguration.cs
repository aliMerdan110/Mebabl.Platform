using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Infrastructure.Authentication.Authorization;
using Mebabl.Platform.Infrastructure.Authentication.Jwt;

namespace Mebabl.Platform.API.Configurations;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var jwt = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT configuration is missing.");

        if (string.IsNullOrWhiteSpace(jwt.Secret) ||
            Encoding.UTF8.GetByteCount(jwt.Secret) < 32)
        {
            throw new InvalidOperationException(
                "JWT signing secret must contain at least 32 UTF-8 bytes.");
        }

        if (string.IsNullOrWhiteSpace(jwt.Issuer))
        {
            throw new InvalidOperationException("JWT issuer is missing.");
        }

        if (string.IsNullOrWhiteSpace(jwt.Audience))
        {
            throw new InvalidOperationException("JWT audience is missing.");
        }

        if (jwt.ExpiryMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JWT expiry must be greater than zero.");
        }

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt.Secret));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.MapInboundClaims = false;
                    options.SaveToken = false;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = jwt.Issuer,

                            ValidateAudience = true,
                            ValidAudience = jwt.Audience,

                            ValidateLifetime = true,
                            RequireExpirationTime = true,
                            RequireSignedTokens = true,
                            ClockSkew = TimeSpan.Zero,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = signingKey,

                            ValidAlgorithms =
                            [
                                SecurityAlgorithms.HmacSha256
                            ],

                            NameClaimType = "userId",
                            RoleClaimType = ClaimTypes.Role
                        };

                    options.Events = new JwtBearerEvents
                    {
                        // SignalR browser transports may send a JWT through
                        // the access_token query parameter. Accept it only
                        // for the realtime hub path.
                        OnMessageReceived = context =>
                        {
                            if (context.Request.Path.StartsWithSegments(
                                    new PathString("/hubs/realtime")))
                            {
                                var accessToken = context.Request
                                    .Query["access_token"]
                                    .ToString();

                                if (!string.IsNullOrWhiteSpace(accessToken))
                                {
                                    context.Token = accessToken;
                                }
                            }

                            return Task.CompletedTask;
                        },

                        OnTokenValidated = ValidateTokenAsync
                    };
                });

        services.AddAuthorization(options =>
        {
            // Read-only catalog access can be performed by a valid app
            // principal or a valid signed-in user principal.
            options.AddPolicy("ApplicationCatalogRead", policy =>
            {
                policy.AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                {
                    var principal = context.User;
                    var type = principal.FindFirst("type")?.Value;

                    if (type is "application" or "application_public")
                    {
                        return HasApplicationClaims(principal) &&
                               (type == "application"
                                   ? principal.HasClaim("authLevel", "secret")
                                   : principal.HasClaim("authLevel", "public") &&
                                     principal.HasClaim("scope", "public"));
                    }

                    return type == "user" && HasUserClaims(principal);
                });
            });

            // Trusted server-to-server application token.
            // Never use this policy for mobile or browser clients.
            options.AddPolicy("Application", policy =>
            {
                policy.AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                {
                    var principal = context.User;
                    return principal.HasClaim("type", "application") &&
                           principal.HasClaim("authLevel", "secret") &&
                           HasApplicationClaims(principal);
                });
            });

            // Public application token. It is intended only for explicitly
            // public SDK operations such as registration and login.
            options.AddPolicy("ApplicationPublic", policy =>
            {
                policy.AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                {
                    var principal = context.User;
                    return principal.HasClaim("type", "application_public") &&
                           principal.HasClaim("authLevel", "public") &&
                           principal.HasClaim("scope", "public") &&
                           HasApplicationClaims(principal);
                });
            });

            // Use only where both trusted server and public application
            // principals are intentionally accepted.
            options.AddPolicy("ApplicationOrPublic", policy =>
            {
                policy.AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                {
                    var principal = context.User;
                    if (!HasApplicationClaims(principal))
                    {
                        return false;
                    }

                    var type = principal.FindFirst("type")?.Value;
                    var authLevel = principal.FindFirst("authLevel")?.Value;

                    return (type == "application" && authLevel == "secret") ||
                           (type == "application_public" &&
                            authLevel == "public" &&
                            principal.HasClaim("scope", "public"));
                });
            });

            options.AddPolicy("User", policy =>
            {
                policy.AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    context.User.HasClaim("type", "user") &&
                    HasUserClaims(context.User));
            });

            options.AddPolicy("ApplicationUser", policy =>
            {
                policy.AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    context.User.HasClaim("type", "user") &&
                    HasUserClaims(context.User));
                policy.Requirements.Add(new ApplicationUserRequirement());
            });

            options.AddPolicy("Developer", ConfigureDeveloperPolicy);

            // Project Console management is developer-authenticated.
            // Every command/query handler must still enforce ownership:
            // Project.DeveloperId must match the developer claim.
            options.AddPolicy("ProjectManagement", ConfigureDeveloperPolicy);

            // Canonical policy for creating and managing project-scoped
            // database definitions (databases, collections, indexes, rules).
            // Resource ownership is enforced in the application handler.
            options.AddPolicy("DatabaseManagement", ConfigureDeveloperPolicy);

            // Canonical policy for end-user document operations.
            // Collection/document Security Rules are enforced separately
            // by the database security engine in the application layer.
            options.AddPolicy("DatabaseUserAccess", policy =>
            {
                policy.AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                    context.User.HasClaim("type", "user") &&
                    HasUserClaims(context.User));
                policy.Requirements.Add(new ApplicationUserRequirement());
            });
        });

        return services;
    }

    private static void ConfigureDeveloperPolicy(AuthorizationPolicyBuilder policy)
    {
        policy.AddAuthenticationSchemes(
            JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim("type", "developer") &&
            HasGuidClaim(context.User, "developerId"));
    }

    private static async Task ValidateTokenAsync(
        TokenValidatedContext context)
    {
        var principal = context.Principal;
        var tokenType = principal?.FindFirst("type")?.Value;

        if (principal is null || string.IsNullOrWhiteSpace(tokenType))
        {
            context.Fail("The token is invalid.");
            return;
        }

        switch (tokenType)
        {
            case "application":
            case "application_public":
                await ValidateApplicationTokenAsync(
                    context,
                    principal,
                    tokenType);
                return;

            case "user":
                await ValidateUserTokenAsync(context, principal);
                return;

            case "developer":
                await ValidateDeveloperTokenAsync(context, principal);
                return;

            default:
                context.Fail("The token type is not supported.");
                return;
        }
    }

    private static async Task ValidateApplicationTokenAsync(
        TokenValidatedContext context,
        ClaimsPrincipal principal,
        string tokenType)
    {
        var expectedAuthLevel = tokenType == "application"
            ? "secret"
            : "public";

        if (principal.FindFirst("authLevel")?.Value != expectedAuthLevel ||
            !TryGetGuidClaim(principal, "applicationId", out var applicationId) ||
            !TryGetGuidClaim(principal, "projectId", out var projectId) ||
            !TryGetGuidClaim(principal, "credentialId", out var credentialId))
        {
            context.Fail("The application token is invalid.");
            return;
        }

        if (tokenType == "application_public" &&
            principal.FindFirst("scope")?.Value != "public")
        {
            context.Fail("The public application token is invalid.");
            return;
        }

        var dbContext = context.HttpContext.RequestServices
            .GetRequiredService<IApplicationDbContext>();

        try
        {
            var credentialIsActive = await dbContext.ApplicationCredentials
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id == credentialId &&
                        x.ApplicationId == applicationId &&
                        x.Application.ProjectId == projectId &&
                        x.IsActive &&
                        !x.IsDeleted &&
                        x.Application.IsActive &&
                        !x.Application.IsDeleted &&
                        x.Application.Project.IsActive &&
                        !x.Application.Project.IsDeleted,
                    context.HttpContext.RequestAborted);

            if (!credentialIsActive)
            {
                context.Fail(
                    "The application credential or project is inactive or revoked.");
            }
        }
        catch (OperationCanceledException)
            when (context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            // A database outage is an infrastructure failure, not proof that
            // the supplied token is invalid. Let the exception middleware
            // return a server error instead of incorrectly returning 401.
            CreateLogger(context).LogError(
                exception,
                "Failed to validate application credential and project state.");

            throw new InvalidOperationException(
                "Application authentication state could not be verified.",
                exception);
        }
    }

    private static async Task ValidateUserTokenAsync(
        TokenValidatedContext context,
        ClaimsPrincipal principal)
    {
        if (!TryGetGuidClaim(principal, "applicationId", out var applicationId) ||
            !TryGetGuidClaim(principal, "projectId", out var projectId) ||
            !TryGetGuidClaim(principal, "userId", out var userId) ||
            !TryGetGuidClaim(principal, "accountId", out var accountId))
        {
            context.Fail("The user token is invalid.");
            return;
        }

        var securityStamp = principal.FindFirst("securityStamp")?.Value;
        if (string.IsNullOrWhiteSpace(securityStamp))
        {
            context.Fail("The user token is missing a security stamp.");
            return;
        }

        var dbContext = context.HttpContext.RequestServices
            .GetRequiredService<IApplicationDbContext>();

        try
        {
            var nowUtc = DateTime.UtcNow;
            var userIsActive = await dbContext.ApplicationUsers
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id == userId &&
                        x.ApplicationId == applicationId &&
                        x.Application.ProjectId == projectId &&
                        x.AccountId == accountId &&
                        x.SecurityStamp == securityStamp &&
                        x.IsActive &&
                        !x.IsDeleted &&
                        !x.Account.IsDeleted &&
                        x.Application.IsActive &&
                        !x.Application.IsDeleted &&
                        x.Application.Project.IsActive &&
                        !x.Application.Project.IsDeleted &&
                        (!x.LockoutEnabled ||
                         !x.LockoutEnd.HasValue ||
                         x.LockoutEnd <= nowUtc),
                    context.HttpContext.RequestAborted);

            if (!userIsActive)
            {
                context.Fail(
                    "The user token is invalid, revoked, or its project is unavailable.");
            }
        }
        catch (OperationCanceledException)
            when (context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            CreateLogger(context).LogError(
                exception,
                "Failed to validate user, account, application, or project state.");

            throw new InvalidOperationException(
                "User authentication state could not be verified.",
                exception);
        }
    }

    private static async Task ValidateDeveloperTokenAsync(
        TokenValidatedContext context,
        ClaimsPrincipal principal)
    {
        if (!TryGetGuidClaim(principal, "developerId", out var developerId))
        {
            context.Fail("The developer token is invalid.");
            return;
        }

        var dbContext = context.HttpContext.RequestServices
            .GetRequiredService<IApplicationDbContext>();

        try
        {
            var developerIsActive = await dbContext.Developers
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id == developerId &&
                        x.IsActive &&
                        !x.IsDeleted,
                    context.HttpContext.RequestAborted);

            if (!developerIsActive)
            {
                context.Fail("The developer is inactive or deleted.");
            }
        }
        catch (OperationCanceledException)
            when (context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            CreateLogger(context).LogError(
                exception,
                "Failed to validate developer state.");

            throw new InvalidOperationException(
                "Developer authentication state could not be verified.",
                exception);
        }
    }

    private static bool HasApplicationClaims(ClaimsPrincipal principal) =>
        HasGuidClaim(principal, "applicationId") &&
        HasGuidClaim(principal, "projectId") &&
        HasGuidClaim(principal, "credentialId");

    private static bool HasUserClaims(ClaimsPrincipal principal) =>
        HasGuidClaim(principal, "userId") &&
        HasGuidClaim(principal, "accountId") &&
        HasGuidClaim(principal, "applicationId") &&
        HasGuidClaim(principal, "projectId");

    private static bool HasGuidClaim(
        ClaimsPrincipal principal,
        string claimType) =>
        TryGetGuidClaim(principal, claimType, out _);

    private static bool TryGetGuidClaim(
        ClaimsPrincipal principal,
        string claimType,
        out Guid value)
    {
        value = Guid.Empty;

        return Guid.TryParse(
                   principal.FindFirst(claimType)?.Value,
                   out value) &&
               value != Guid.Empty;
    }

    private static ILogger CreateLogger(TokenValidatedContext context) =>
        context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(AuthenticationConfiguration));
}
