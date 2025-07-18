using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Đăng ký các service application mẫu (có thể mở rộng sau)
        // services.AddScoped<IConfigurationManagementService, ConfigurationManagementService>();
        return services;
    }
} 