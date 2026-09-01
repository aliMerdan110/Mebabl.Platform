using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;



namespace Mebabl.Platform.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                Assembly.GetExecutingAssembly()));

        return services;
    }
}