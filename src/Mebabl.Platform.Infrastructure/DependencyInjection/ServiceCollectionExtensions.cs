using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Options;
using Mebabl.Platform.Application.Common.Realtime;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Common.Services.ApplicationInitialization;
using Mebabl.Platform.Application.Common.Services.Authorization;
using Mebabl.Platform.Application.Common.Storage;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;
using Mebabl.Platform.Application.Features.Live.Media.Srs;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Application.Services.Email;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Live;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Application.Services.PasswordReset;

using Mebabl.Platform.Infrastructure.Authentication;
using Mebabl.Platform.Infrastructure.Authentication.Authorization;
using Mebabl.Platform.Infrastructure.Authentication.Jwt;
using Mebabl.Platform.Infrastructure.Authorization;
using Mebabl.Platform.Infrastructure.Data;
using Mebabl.Platform.Infrastructure.Database.QueryEngine;
using Mebabl.Platform.Infrastructure.Identity;
using Mebabl.Platform.Infrastructure.Live;
using Mebabl.Platform.Infrastructure.Realtime;
using Mebabl.Platform.Infrastructure.Services.Clock;
using Mebabl.Platform.Infrastructure.Services.CurrentUser;
using Mebabl.Platform.Infrastructure.Services.Email;
using Mebabl.Platform.Infrastructure.Services.Password;
using Mebabl.Platform.Infrastructure.Services.PasswordReset;
using Mebabl.Platform.Infrastructure.Storage;

namespace Mebabl.Platform.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ---------------------------------------------------------
        // Password Reset
        // ---------------------------------------------------------

        services.AddScoped<
            IPasswordResetTokenService,
            PasswordResetTokenService>();

        // ---------------------------------------------------------
        // Email
        // ---------------------------------------------------------

        services.Configure<EmailOptions>(
            configuration.GetSection(EmailOptions.SectionName));

        services.AddScoped<
            IEmailService,
            SmtpEmailService>();

        // ---------------------------------------------------------
        // JWT
        // ---------------------------------------------------------

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        // ---------------------------------------------------------
        // Console
        // ---------------------------------------------------------

        services.Configure<ConsoleOptions>(
            configuration.GetSection(ConsoleOptions.SectionName));

        // ---------------------------------------------------------
        // Authorization
        // ---------------------------------------------------------

        services.AddSingleton<
            IAuthorizationPolicyProvider,
            PermissionPolicyProvider>();

        services.AddSingleton<
            IAuthorizationHandler,
            PermissionAuthorizationHandler>();

        services.AddSingleton<
            IAuthorizationHandler,
            ApplicationUserAuthorizationHandler>();

        // ---------------------------------------------------------
        // Database
        // ---------------------------------------------------------

        services.AddDbContext<PlatformDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString(
                    "DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<PlatformDbContext>());

        // ---------------------------------------------------------
        // Permission Checker
        // ---------------------------------------------------------

        services.AddScoped<
            IPermissionChecker,
            PermissionChecker>();

        // ---------------------------------------------------------
        // Live Authorization
        // ---------------------------------------------------------

        services.AddScoped<
            ILiveAuthorizationService,
            LiveAuthorizationService>();

        services.AddScoped<
            IPublishTokenService,
            PublishTokenService>();

        services.AddScoped<
            ISrsPublishAuthorizationService,
            SrsPublishAuthorizationService>();

        // ---------------------------------------------------------
        // Document Security
        // ---------------------------------------------------------

        services.AddScoped<
            IDocumentSecurityService,
            DocumentSecurityService>();

        // ---------------------------------------------------------
        // Realtime
        // ---------------------------------------------------------

        services.AddScoped<
            IRealtimePublisher,
            SignalRRealtimePublisher>();

        // ---------------------------------------------------------
        // Current User
        // ---------------------------------------------------------

        services.AddScoped<
            ICurrentUser,
            CurrentUser>();

        // ---------------------------------------------------------
        // Current Developer
        // ---------------------------------------------------------

        services.AddScoped<
            ICurrentDeveloper,
            CurrentDeveloper>();

        // ---------------------------------------------------------
        // Current Application
        // ---------------------------------------------------------

        services.AddScoped<
            ICurrentApplication,
            CurrentApplication>();

        // ---------------------------------------------------------
        // HTTP Context
        // ---------------------------------------------------------

        services.AddHttpContextAccessor();

        // ---------------------------------------------------------
        // Clock
        // ---------------------------------------------------------

        services.AddScoped<
            IClock,
            Clock>();

        // ---------------------------------------------------------
        // Password Hashing
        // ---------------------------------------------------------

        services.AddScoped<
            IPasswordHasher,
            PasswordHasher>();

        // ---------------------------------------------------------
        // JWT Token Generator
        // ---------------------------------------------------------

        services.AddScoped<
            IJwtTokenGenerator,
            JwtTokenGenerator>();

        // ---------------------------------------------------------
        // Application Initializer
        // ---------------------------------------------------------

        services.AddScoped<
            IApplicationInitializer,
            ApplicationInitializer>();

        // ---------------------------------------------------------
        // Query Engine
        // ---------------------------------------------------------

        services.AddScoped<
            IQueryBuilder,
            PostgreSqlQueryBuilder>();

        // ---------------------------------------------------------
        // Storage
        // ---------------------------------------------------------

        services.AddScoped<
            IStorageProvider,
            LocalStorageProvider>();

        // ---------------------------------------------------------
        // CORS
        // ---------------------------------------------------------

        services.AddCors(options =>
        {
            options.AddPolicy(
                "MebablConsole",
                policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:3000",
                            "https://mebabl.com")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        return services;
    }
}