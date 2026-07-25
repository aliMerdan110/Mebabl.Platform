using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mebabl.Platform.API.Configuration;

namespace Mebabl.Platform.API.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();

        services.AddEndpointsApiExplorer();

        services.AddOpenApi();

        services.AddJwtAuthentication(configuration);

        return services;
    }
}