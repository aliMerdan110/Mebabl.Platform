using Mebabl.Platform.API.DependencyInjection;
using Mebabl.Platform.API.Extensions;
using Mebabl.Platform.Application.DependencyInjection;
using Mebabl.Platform.Infrastructure.DependencyInjection;
using Mebabl.Platform.Infrastructure.Realtime;

var builder = WebApplication.CreateBuilder(args);

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
});

builder.Services.AddSignalR();

builder.Services
    .AddPresentation(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("MebablSdk", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

app.UseCors("MebablSdk");

app.UsePresentation();

app.MapHub<RealtimeHub>("/hubs/realtime");

app.Run();