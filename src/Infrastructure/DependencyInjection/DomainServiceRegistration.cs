using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class DomainServiceRegistration
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Đăng ký các service domain mẫu (có thể mở rộng sau)
        // services.AddTransient<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }
} 