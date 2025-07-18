using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Infrastructure.Logging;
using Infrastructure.Logging.Enrichers;
using Serilog.Core;

namespace Infrastructure.DependencyInjection;

public static class LoggingServiceRegistration
{
    public static IServiceCollection AddLoggingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Serilog
        Log.Logger = SerilogConfiguration.CreateLogger(configuration);

        // Register Serilog with Microsoft.Extensions.Logging
        services.AddLogging(builder =>
        {
            // builder.ClearProviders(); // Không cần cho WinForms/Console
            builder.AddSerilog(dispose: true);
        });

        // Register custom logging services
        services.AddScoped(typeof(Application.Interfaces.IApplicationLogger<>), typeof(ApplicationLogger<>));
        services.AddScoped<IStructuredLogger, StructuredLogger>();

        // Register enrichers
        services.AddSingleton<ILogEventEnricher, UserContextEnricher>();
        services.AddSingleton<ILogEventEnricher, PerformanceEnricher>();

        // Register context providers (mock)
        services.AddSingleton<IUserContextProvider, MockUserContextProvider>();
        services.AddSingleton<ICorrelationIdProvider, MockCorrelationIdProvider>();

        return services;
    }
}

// Mock providers for demo (cần thay thế bằng implementation thực tế khi phát triển nghiệp vụ)
public class MockUserContextProvider : IUserContextProvider
{
    public UserContext? GetCurrentUser() => new UserContext { UserId = "demo", UserName = "Demo User", Role = "Admin" };
}
public interface ICorrelationIdProvider { string GetCorrelationId(); }
public class MockCorrelationIdProvider : ICorrelationIdProvider { public string GetCorrelationId() => Guid.NewGuid().ToString(); } 