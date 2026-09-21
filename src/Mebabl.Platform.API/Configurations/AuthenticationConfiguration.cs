using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Mebabl.Platform.Infrastructure.Authentication.Authorization;
using Mebabl.Platform.Infrastructure.Authentication.Jwt;

namespace Mebabl.Platform.API.Configurations;

public static class AuthenticationConfiguration
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwt = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "JWT configuration is missing.");

        if (string.IsNullOrWhiteSpace(jwt.Secret))
            throw new InvalidOperationException(
                "JWT secret is missing.");

        if (string.IsNullOrWhiteSpace(jwt.Issuer))
            throw new InvalidOperationException(
                "JWT issuer is missing.");

        if (string.IsNullOrWhiteSpace(jwt.Audience))
            throw new InvalidOperationException(
                "JWT audience is missing.");

        if (jwt.ExpiryMinutes <= 0)
            throw new InvalidOperationException(
                "JWT expiry must be greater than zero.");

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt.Secret));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = jwt.Issuer,

                            ValidateAudience = true,
                            ValidAudience = jwt.Audience,

                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = signingKey,

                            NameClaimType = "userId",
                            RoleClaimType = ClaimTypes.Role
                        };
                });

        services.AddAuthorization(options =>
        {
            // سياسات الهوية الأساسية للتطبيق والمستخدم والمطور.
            options.AddPolicy("Application", policy =>
            {
                policy.AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme);

                policy.RequireAuthenticatedUser();

                policy.RequireClaim(
                    "type",
                    "application");

                policy.RequireClaim("applicationId");
                policy.RequireClaim("credentialId");
            });

            options.AddPolicy("User", policy =>
            {
                policy.AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme);

                policy.RequireAuthenticatedUser();

                policy.RequireClaim(
                    "type",
                    "user");

                policy.RequireClaim("userId");
                policy.RequireClaim("accountId");
                policy.RequireClaim("applicationId");
            });

            options.AddPolicy("ApplicationUser", policy =>
            {
                policy.AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme);

                policy.RequireAuthenticatedUser();

                policy.RequireClaim(
                    "type",
                    "user");

                policy.RequireClaim("userId");
                policy.RequireClaim("accountId");
                policy.RequireClaim("applicationId");

                policy.Requirements.Add(
                    new ApplicationUserRequirement());
            });

            options.AddPolicy("Developer", policy =>
            {
                policy.AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme);

                policy.RequireAuthenticatedUser();

                policy.RequireClaim(
                    "type",
                    "developer");

                policy.RequireClaim("developerId");
            });
        });

        return services;
    }
}