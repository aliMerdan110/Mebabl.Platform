using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Application.Services.CurrentUser;
using Mebabl.Platform.Application.Services.Jwt;
using Mebabl.Platform.Application.Services.Password;
using Mebabl.Platform.Infrastructure.Authentication.Jwt;
using Mebabl.Platform.Infrastructure.Services.Clock;
using Mebabl.Platform.Infrastructure.Services.CurrentUser;
using Mebabl.Platform.Infrastructure.Services.Password;

namespace Mebabl.Platform.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

            services.AddDbContext<PlatformDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection")));

services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<PlatformDbContext>());

        services.AddHttpContextAccessor();

        services.AddScoped<IClock, Clock>();

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        

        return services;
    }
}