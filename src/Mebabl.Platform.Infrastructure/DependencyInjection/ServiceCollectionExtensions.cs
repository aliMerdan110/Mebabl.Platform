using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Infrastructure.Authentication.Jwt;
using Mebabl.Platform.Infrastructure.Services.Clock;
using Mebabl.Platform.Infrastructure.Services.CurrentUser;
using Mebabl.Platform.Infrastructure.Services.Password;
using Mebabl.Platform.Infrastructure.Authentication;
using Mebabl.Platform.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Mebabl.Platform.Infrastructure.Authentication.Authorization;
using Mebabl.Platform.Application.Common.Services.ApplicationInitialization;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Infrastructure.Security;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;
using Mebabl.Platform.Infrastructure.Database.QueryEngine;
using Mebabl.Platform.Application.Common.Storage;
using Mebabl.Platform.Infrastructure.Storage;
using Mebabl.Platform.Application.Common.Realtime;
using Mebabl.Platform.Infrastructure.Realtime;
using Mebabl.Platform.Application.Services.PasswordReset;
using Mebabl.Platform.Infrastructure.Services.PasswordReset;
using Mebabl.Platform.Infrastructure.Services.Email;
using Mebabl.Platform.Application.Common.Options;



namespace Mebabl.Platform.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {


services.AddScoped<IPasswordResetTokenService,PasswordResetTokenService>();

services.Configure<EmailOptions>(
    configuration.GetSection(EmailOptions.SectionName));

services.AddScoped<IEmailService, SmtpEmailService>();


        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

            services.Configure<ConsoleOptions>(
    configuration.GetSection(ConsoleOptions.SectionName));


    services.AddSingleton<IAuthorizationPolicyProvider,
    PermissionPolicyProvider>();

services.AddSingleton<IAuthorizationHandler,
    PermissionAuthorizationHandler>();       

            services.AddDbContext<PlatformDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection")));

services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<PlatformDbContext>());


    services.AddScoped<IDocumentSecurityService, DocumentSecurityService>();

    services.AddScoped<IRealtimePublisher,  SignalRRealtimePublisher>();

    services.AddScoped<ICurrentUser, CurrentUser>();

    services.AddScoped<ICurrentDeveloper, CurrentDeveloper>();

    services.AddScoped<ICurrentApplication, CurrentApplication>();

        services.AddHttpContextAccessor();

        services.AddScoped<IClock, Clock>();

        

        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IApplicationInitializer, ApplicationInitializer>();

        services.AddScoped<IQueryBuilder, PostgreSqlQueryBuilder>();

        services.AddScoped<IStorageProvider, LocalStorageProvider>();

        services.AddCors(options =>
{
    options.AddPolicy("MebablConsole", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "https://mebabl.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

        return services;
    }
}