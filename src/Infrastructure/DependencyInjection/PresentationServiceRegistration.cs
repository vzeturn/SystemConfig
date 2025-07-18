using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class PresentationServiceRegistration
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        // Đăng ký các service UI, form (nên thực hiện ở Presentation layer)
        return services;
    }
} 