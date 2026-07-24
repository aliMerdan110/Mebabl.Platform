using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mebabl.Platform.Infrastructure.Data;

namespace Mebabl.Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. تسجيل قاعدة البيانات (PostgreSQL) باستخدام Entity Framework Core
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<PlatformDbContext>(options =>
            options.UseNpgsql(connectionString));

        // 2. تسجيل الـ Repositories و UnitOfWork
        // سيتم إضافة المستودعات الخاصة بالموديولات هنا تباعاً

        // 3. تسجيل خدمات المصادقة والخدمات الأمنية
        // سيتم إضافة خدمات الـ Token والـ Identity هنا تباعاً

        return services;
    }
}