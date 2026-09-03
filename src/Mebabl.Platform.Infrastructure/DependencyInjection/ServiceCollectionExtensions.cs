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
using Mebabl.Platform.Application.Common.Options;
using Mebabl.Platform.Application.Services.Email;
using Mebabl.Platform.Infrastructure.Services.Email;
using Mebabl.Platform.Application.Features.Live.Media.Srs;

// Live Streaming
using Mebabl.Platform.Application.Common.Services.Authorization;
using Mebabl.Platform.Application.Services.Live;
using Mebabl.Platform.Infrastructure.Authorization;
using Mebabl.Platform.Infrastructure.Live;


namespace Mebabl.Platform.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ---------------------------------------------------------
        // Password Reset
        // مسؤول عن إنشاء والتحقق من Reset Tokens
        // ---------------------------------------------------------

        services.AddScoped<
            IPasswordResetTokenService,
            PasswordResetTokenService>();


        // ---------------------------------------------------------
        // Email
        // مسؤول عن إرسال رسائل البريد الإلكتروني
        // ---------------------------------------------------------

        services.Configure<EmailOptions>(
            configuration.GetSection(EmailOptions.SectionName));

        services.AddScoped<
            IEmailService,
            SmtpEmailService>();


        // ---------------------------------------------------------
        // JWT
        // إعدادات وإنشاء JWT Tokens
        // ---------------------------------------------------------

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));


        // ---------------------------------------------------------
        // Console
        // إعدادات روابط وخدمات Mebabl Console
        // ---------------------------------------------------------

        services.Configure<ConsoleOptions>(
            configuration.GetSection(ConsoleOptions.SectionName));


        // ---------------------------------------------------------
        // Authorization
        // نظام Permission-based Authorization
        // يسمح باستخدام [Authorize(Policy = "...")]
        // ---------------------------------------------------------

        services.AddSingleton<
            IAuthorizationPolicyProvider,
            PermissionPolicyProvider>();

        services.AddSingleton<
            IAuthorizationHandler,
            PermissionAuthorizationHandler>();


        // ---------------------------------------------------------
        // Database
        // PostgreSQL + Entity Framework Core
        // ---------------------------------------------------------

        services.AddDbContext<PlatformDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString(
                    "DefaultConnection")));


        // ---------------------------------------------------------
        // Application DbContext
        // يجعل Application Layer تتعامل مع قاعدة البيانات
        // من خلال IApplicationDbContext
        // ---------------------------------------------------------

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<PlatformDbContext>());


        // =========================================================
        // LIVE STREAMING
        // =========================================================

        // ---------------------------------------------------------
        // Permission Checker
        //
        // يفحص:
        //
        // ApplicationUser
        //      ↓
        // ApplicationUserRole
        //      ↓
        // Role
        //      ↓
        // RolePermission
        //      ↓
        // Permission
        //
        // مثال:
        // live.publish
        // live.view
        // ---------------------------------------------------------

        services.AddScoped<
            IPermissionChecker,
            PermissionChecker>();

            // Infrastructure/DependencyInjection.cs
//
// تسجيل الخدمات المطلوبة.

services.AddScoped<ILiveAuthorizationService, LiveAuthorizationService>();


services.AddScoped<
    IPublishTokenService,
    PublishTokenService>();
// Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs
// Live Token Service

// Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs

services.AddScoped<
    ISrsPublishAuthorizationService,
    SrsPublishAuthorizationService>();


        // ---------------------------------------------------------
        // Live Authorization
        //
        // يقرر هل المستخدم يستطيع:
        //
        // Publish
        // View
        //
        // ولا يفرض أن Developer هو الذي يبث.
        // التطبيق نفسه يحدد الصلاحيات.
        // ---------------------------------------------------------

        services.AddScoped<
            ILiveAuthorizationService,
            LiveAuthorizationService>();


        // ---------------------------------------------------------
        // Document Security
        // قواعد أمان Collections / Documents
        // ---------------------------------------------------------

        services.AddScoped<
            IDocumentSecurityService,
            DocumentSecurityService>();


        // ---------------------------------------------------------
        // Realtime
        // نشر الأحداث عبر SignalR
        // ---------------------------------------------------------

        services.AddScoped<
            IRealtimePublisher,
            SignalRRealtimePublisher>();


        // ---------------------------------------------------------
        // Current User
        // المستخدم الحالي من Application JWT
        // ---------------------------------------------------------

        services.AddScoped<
            ICurrentUser,
            CurrentUser>();


        // ---------------------------------------------------------
        // Current Developer
        // Developer JWT الحالي
        // ---------------------------------------------------------

        services.AddScoped<
            ICurrentDeveloper,
            CurrentDeveloper>();


        // ---------------------------------------------------------
        // Current Application
        // Application Authentication Context
        // ---------------------------------------------------------

        services.AddScoped<
            ICurrentApplication,
            CurrentApplication>();


        // ---------------------------------------------------------
        // HTTP Context
        // الوصول إلى HttpContext من الخدمات
        // ---------------------------------------------------------

        services.AddHttpContextAccessor();


        // ---------------------------------------------------------
        // Clock
        // مصدر موحد للوقت UTC
        // ---------------------------------------------------------

        services.AddScoped<
            IClock,
            Clock>();


        // ---------------------------------------------------------
        // Password Hashing
        // تشفير كلمات المرور والتحقق منها
        // ---------------------------------------------------------

        services.AddScoped<
            IPasswordHasher,
            PasswordHasher>();


        // ---------------------------------------------------------
        // JWT Token Generator
        // إنشاء Access / Refresh JWT Tokens
        // ---------------------------------------------------------

        services.AddScoped<
            IJwtTokenGenerator,
            JwtTokenGenerator>();


        // ---------------------------------------------------------
        // Application Initializer
        // إنشاء البيانات الافتراضية عند إنشاء Application
        // ---------------------------------------------------------

        services.AddScoped<
            IApplicationInitializer,
            ApplicationInitializer>();


        // ---------------------------------------------------------
        // Query Engine
        // بناء وتنفيذ استعلامات PostgreSQL
        // ---------------------------------------------------------

        services.AddScoped<
            IQueryBuilder,
            PostgreSqlQueryBuilder>();


        // ---------------------------------------------------------
        // Storage
        // التخزين المحلي للملفات
        // ---------------------------------------------------------

        services.AddScoped<
            IStorageProvider,
            LocalStorageProvider>();


        // ---------------------------------------------------------
        // CORS
        // السماح لـ Mebabl Console بالوصول إلى API
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