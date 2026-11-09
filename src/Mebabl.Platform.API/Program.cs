
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;

using Mebabl.Platform.API.DependencyInjection;
using Mebabl.Platform.API.Extensions;
using Mebabl.Platform.Application.Common.Options;
using Mebabl.Platform.Application.DependencyInjection;
using Mebabl.Platform.Infrastructure.DependencyInjection;
using Mebabl.Platform.Infrastructure.Realtime;

var builder = WebApplication.CreateBuilder(args);

// Configure explicit origins for browser-based realtime connections.
// In Development, localhost:3000 is used when no origins are configured.
// In other environments, configure Cors:RealtimeAllowedOrigins explicitly.
var realtimeAllowedOrigins = GetAllowedOrigins(
    builder.Configuration,
    "Cors:RealtimeAllowedOrigins",
    builder.Environment.IsDevelopment(),
    ["http://localhost:3000"]);

// CORS policies.
builder.Services.AddCors(options =>
{
    options.AddPolicy("MebablConsole", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    // Preserve the existing public SDK HTTP policy.
    // Do not use this policy for the realtime hub.
    options.AddPolicy("MebablSdk", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });

    // Dedicated policy for SignalR negotiation and browser transports.
    options.AddPolicy("MebablRealtime", policy =>
    {
        policy
            .WithOrigins(realtimeAllowedOrigins)
            .WithMethods("GET", "POST")
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Rate limiting.
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ApplicationServerToken", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy("ApplicationToken", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                context.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder =
                    QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
});

builder.Services.AddSignalR();

builder.Services
    .AddPresentation(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.Configure<AuthOptions>(
    builder.Configuration.GetSection("Auth"));

var app = builder.Build();

// Configure WebSocket origin restrictions before routing.
// Authentication is still required; Origin is not an authentication mechanism.
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
};

foreach (var origin in realtimeAllowedOrigins)
{
    webSocketOptions.AllowedOrigins.Add(origin);
}

app.UseWebSockets(webSocketOptions);

// UsePresentation configures routing, CORS, rate limiting,
// authentication, authorization, and controller endpoints.
app.UsePresentation();

// Realtime uses its own restrictive CORS policy.
app.MapHub<RealtimeHub>("/hubs/realtime")
    .RequireCors("MebablRealtime");

app.Run();

static string[] GetAllowedOrigins(
    IConfiguration configuration,
    string sectionPath,
    bool isDevelopment,
    string[] developmentFallback)
{
    var configuredOrigins =
        configuration.GetSection(sectionPath).Get<string[]>() ?? [];

    if (configuredOrigins.Length == 0)
    {
        if (!isDevelopment)
        {
            throw new InvalidOperationException(
                $"Configure '{sectionPath}' with the approved browser origins.");
        }

        configuredOrigins = developmentFallback;
    }

    var normalizedOrigins = new List<string>();

    foreach (var origin in configuredOrigins)
    {
        if (string.IsNullOrWhiteSpace(origin) ||
            !Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps) ||
            uri.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment) ||
            !string.IsNullOrEmpty(uri.UserInfo))
        {
            throw new InvalidOperationException(
                $"Invalid browser origin '{origin}' in '{sectionPath}'. " +
                "Specify only an origin, such as https://console.example.com.");
        }

        // Normalize to scheme + authority, without a trailing slash or path.
        normalizedOrigins.Add(
            $"{uri.Scheme}://{uri.Authority}");
    }

    return normalizedOrigins
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}
