using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Mebabl.Platform.Infrastructure.Authentication.ApplicationApiKey;
using Mebabl.Platform.Infrastructure.Authentication.Jwt;

namespace Mebabl.Platform.API.Configuration;

public static class AuthenticationConfiguration
{
    public const string ApplicationScheme = "ApplicationApiKey";

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwt = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()!;

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
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer = jwt.Issuer,
                            ValidAudience = jwt.Audience,

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(jwt.Secret))
                        };
                })
            .AddScheme<
                ApplicationApiKeyAuthenticationOptions,
                ApplicationApiKeyAuthenticationHandler>(
                ApplicationScheme,
                _ =>
                {
                });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Application", policy =>
            {
                policy.AddAuthenticationSchemes(
                    ApplicationScheme);

                policy.RequireAuthenticatedUser();

                policy.RequireClaim(
                    "type",
                    "application");
            });

            options.AddPolicy("User", policy =>
            {
                policy.AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme);

                policy.RequireAuthenticatedUser();

                policy.RequireClaim(
                    "type",
                    "user");
            });
        });

        return services;
    }
}