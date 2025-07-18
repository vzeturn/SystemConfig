using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Đăng ký các service infrastructure mẫu (có thể mở rộng sau)
        // services.AddSingleton<IRegistryService, WindowsRegistryService>();
        return services;
    }
} 