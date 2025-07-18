using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSystemConfigServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainServices();
        services.AddApplicationServices();
        services.AddInfrastructureServices(configuration);
        services.AddPresentationServices();
        services.AddLoggingServices(configuration);
        return services;
    }
} 