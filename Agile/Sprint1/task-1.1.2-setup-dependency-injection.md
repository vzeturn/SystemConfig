# Task 1.1.2: Setup Dependency Injection

## 📋 Task Information

| **Attribute** | **Value** |
|---------------|-----------|
| **Task ID** | 1.1.2 |
| **Sprint** | Sprint 1 (Weeks 1-2) |
| **Story** | 1.1 - Project Architecture Setup |
| **Priority** | Critical |
| **Story Points** | 6 |
| **Estimated Hours** | 6 |
| **Assigned To** | Senior Developer |
| **Status** | Not Started |
| **Dependencies** | Task 1.1.1 (Create Solution Structure) |

## 🎯 Objective

Implement modern dependency injection container with Microsoft.Extensions.DependencyInjection, configure service lifetimes, and establish foundation for Clean Architecture dependency management across all layers.

## 📝 Detailed Requirements

### Functional Requirements

#### **DI Container Setup**
- [ ] Configure Microsoft.Extensions.DependencyInjection as primary container
- [ ] Implement service registration with appropriate lifetimes
- [ ] Setup configuration-based service registration
- [ ] Create extension methods for clean service registration
- [ ] Implement factory patterns for complex object creation

#### **Service Lifetime Management**
- [ ] **Singleton Services**: Application-wide shared services
  - Configuration services
  - Logging services
  - Caching services
  - Security services

- [ ] **Scoped Services**: Request/operation-scoped services
  - Repository instances
  - Unit of Work instances
  - Application services
  - Validation services

- [ ] **Transient Services**: Per-request instances
  - Command/Query handlers
  - Domain services
  - Value object factories
  - Event handlers

#### **Cross-Layer Registration**
- [ ] **Domain Layer**: Pure domain services only
- [ ] **Application Layer**: CQRS handlers, application services
- [ ] **Infrastructure Layer**: Repository implementations, external services
- [ ] **Presentation Layer**: UI services, controllers

### Technical Requirements

#### **Container Configuration**
```csharp
// ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSystemConfigServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Core services registration
        services.AddDomainServices();
        services.AddApplicationServices();
        services.AddInfrastructureServices(configuration);
        services.AddPresentationServices();
        
        return services;
    }
    
    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Domain service registrations
        services.AddTransient<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddTransient<IConfigurationValidator, ConfigurationValidator>();
        services.AddTransient<IBusinessRuleEngine, BusinessRuleEngine>();
        
        return services;
    }
    
    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR registration for CQRS
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));
        
        // Application services
        services.AddScoped<IConfigurationManagementService, ConfigurationManagementService>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IAuditService, AuditService>();
        
        return services;
    }
    
    private static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Repository registrations
        services.AddScoped<IConfigurationRepository, RegistryConfigurationRepository>();
        services.AddScoped<IUnitOfWork, RegistryUnitOfWork>();
        
        // Infrastructure services
        services.AddSingleton<IEncryptionService, AesEncryptionService>();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton<IRegistryService, WindowsRegistryService>();
        
        return services;
    }
}
```

#### **Service Registration Patterns**
```csharp
// Generic repository registration
services.AddScoped(typeof(IRepository<>), typeof(RegistryRepository<>));

// Decorator pattern for logging
services.Decorate<IConfigurationService, LoggingConfigurationServiceDecorator>();

// Factory pattern registration
services.AddSingleton<IConfigurationFactory>(provider => 
    new ConfigurationFactory(provider.GetRequiredService<IServiceProvider>()));

// Conditional registration based on configuration
if (configuration.GetValue<bool>("Features:EnableCaching"))
{
    services.AddSingleton<ICacheService, RedisCacheService>();
}
else
{
    services.AddSingleton<ICacheService, MemoryCacheService>();
}
```

### Quality Requirements

#### **Performance Standards**
- [ ] Container resolution time <1ms for 95% of services
- [ ] Memory overhead <5MB for DI container
- [ ] Startup time impact <200ms
- [ ] No circular dependency chains

#### **Maintainability Standards**
- [ ] Service registration grouped by layer and functionality
- [ ] Extension methods for clean registration syntax
- [ ] Configuration-driven service selection
- [ ] Clear service lifetime documentation

## 🏗️ Implementation Plan

### Phase 1: Core DI Setup (2 hours)

#### **Step 1: Install Dependencies**
```xml
<!-- SystemConfig.Presentation.csproj -->
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
```

#### **Step 2: Bootstrap Container**
```csharp
// Program.cs
public static class Program
{
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        var host = CreateHostBuilder().Build();
        
        using (var scope = host.Services.CreateScope())
        {
            var mainForm = scope.ServiceProvider.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }
    }
    
    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
                    optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSystemConfigServices(context.Configuration);
            });
    }
}
```

### Phase 2: Layer-Specific Registration (2 hours)

#### **Step 3: Domain Layer Registration**
```csharp
// Infrastructure/DependencyInjection/DomainServiceRegistration.cs
public static class DomainServiceRegistration
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Domain event handling
        services.AddTransient<IDomainEventDispatcher, DomainEventDispatcher>();
        
        // Business rule validation
        services.AddTransient<IConfigurationBusinessRules, ConfigurationBusinessRules>();
        services.AddTransient<IPrinterBusinessRules, PrinterBusinessRules>();
        services.AddTransient<ISystemBusinessRules, SystemBusinessRules>();
        
        // Domain factories
        services.AddTransient<IConfigurationFactory, ConfigurationFactory>();
        services.AddTransient<IPrinterConfigurationFactory, PrinterConfigurationFactory>();
        
        // Value object services
        services.AddTransient<IConnectionStringBuilder, ConnectionStringBuilder>();
        services.AddTransient<ITemplateValidator, TemplateValidator>();
        
        return services;
    }
}
```

#### **Step 4: Application Layer Registration**
```csharp
// Infrastructure/DependencyInjection/ApplicationServiceRegistration.cs
public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR for CQRS
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        });
        
        // Application services
        services.AddScoped<IConfigurationManagementService, ConfigurationManagementService>();
        services.AddScoped<IPrinterManagementService, PrinterManagementService>();
        services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
        services.AddScoped<IBackupService, BackupService>();
        
        // Validation services
        services.AddScoped<IValidationService, FluentValidationService>();
        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
        
        // Mapping services
        services.AddAutoMapper(typeof(ApplicationAssemblyMarker).Assembly);
        
        return services;
    }
}
```

### Phase 3: Infrastructure Registration (1.5 hours)

#### **Step 5: Infrastructure Layer Registration**
```csharp
// Infrastructure/DependencyInjection/InfrastructureServiceRegistration.cs
public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Repository pattern
        services.AddScoped<IUnitOfWork, RegistryUnitOfWork>();
        services.AddScoped<IConfigurationRepository, RegistryConfigurationRepository>();
        services.AddScoped<IPrinterRepository, RegistryPrinterRepository>();
        services.AddScoped<ISystemConfigRepository, RegistrySystemConfigRepository>();
        
        // Generic repository
        services.AddScoped(typeof(IRepository<>), typeof(RegistryRepository<>));
        
        // Security services
        services.AddSingleton<IEncryptionService>(provider =>
        {
            var keyProvider = provider.GetRequiredService<IKeyProvider>();
            return new AesGcmEncryptionService(keyProvider);
        });
        services.AddSingleton<IKeyProvider, DpapiKeyProvider>();
        services.AddScoped<IPermissionService, WindowsPermissionService>();
        
        // Registry services
        services.AddSingleton<IRegistryService, WindowsRegistryService>();
        services.AddScoped<IRegistryBackupService, RegistryBackupService>();
        
        // Caching services
        var cacheConfig = configuration.GetSection("Caching");
        if (cacheConfig.GetValue<bool>("UseRedis"))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheConfig.GetConnectionString("Redis");
            });
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }
        
        // Logging configuration
        services.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });
        
        return services;
    }
}
```

### Phase 4: Presentation Registration (0.5 hours)

#### **Step 6: Presentation Layer Registration**
```csharp
// Infrastructure/DependencyInjection/PresentationServiceRegistration.cs
public static class PresentationServiceRegistration
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        // Forms registration
        services.AddTransient<MainForm>();
        services.AddTransient<DatabaseConfigurationForm>();
        services.AddTransient<PrinterConfigurationForm>();
        services.AddTransient<SystemConfigurationForm>();
        services.AddTransient<BackupManagerForm>();
        
        // UI services
        services.AddSingleton<IThemeService, ModernThemeService>();
        services.AddSingleton<INotificationService, ToastNotificationService>();
        services.AddScoped<IDialogService, WindowsDialogService>();
        services.AddScoped<IProgressService, ProgressBarService>();
        
        // UI state management
        services.AddSingleton<IUIStateManager, FormStateManager>();
        services.AddScoped<IFormValidator, RealTimeFormValidator>();
        
        return services;
    }
}
```

## 🧪 Testing Strategy

### Unit Tests

#### **Test 1: Service Registration Validation**
```csharp
[TestFixture]
public class DependencyInjectionTests
{
    private IServiceProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Features:EnableCaching", "true"},
                {"Caching:UseRedis", "false"}
            })
            .Build();
            
        services.AddSystemConfigServices(configuration);
        _serviceProvider = services.BuildServiceProvider();
    }
    
    [Test]
    public void DomainServices_ShouldBeRegistered()
    {
        // Assert domain services are registered correctly
        Assert.That(_serviceProvider.GetService<IDomainEventDispatcher>(), Is.Not.Null);
        Assert.That(_serviceProvider.GetService<IConfigurationBusinessRules>(), Is.Not.Null);
        Assert.That(_serviceProvider.GetService<IConfigurationFactory>(), Is.Not.Null);
    }
    
    [Test]
    public void ApplicationServices_ShouldBeRegistered()
    {
        // Assert application services are registered correctly
        Assert.That(_serviceProvider.GetService<IMediator>(), Is.Not.Null);
        Assert.That(_serviceProvider.GetService<IConfigurationManagementService>(), Is.Not.Null);
        Assert.That(_serviceProvider.GetService<IValidationService>(), Is.Not.Null);
    }
    
    [Test]
    public void InfrastructureServices_ShouldBeRegistered()
    {
        // Assert infrastructure services are registered correctly
        Assert.That(_serviceProvider.GetService<IUnitOfWork>(), Is.Not.Null);
        Assert.That(_serviceProvider.GetService<IEncryptionService>(), Is.Not.Null);
        Assert.That(_serviceProvider.GetService<ICacheService>(), Is.Not.Null);
    }
    
    [Test]
    public void ServiceLifetimes_ShouldBeCorrect()
    {
        // Test singleton services
        var cache1 = _serviceProvider.GetService<ICacheService>();
        var cache2 = _serviceProvider.GetService<ICacheService>();
        Assert.That(cache1, Is.SameAs(cache2));
        
        // Test scoped services
        using var scope1 = _serviceProvider.CreateScope();
        using var scope2 = _serviceProvider.CreateScope();
        
        var repo1 = scope1.ServiceProvider.GetService<IConfigurationRepository>();
        var repo2 = scope1.ServiceProvider.GetService<IConfigurationRepository>();
        var repo3 = scope2.ServiceProvider.GetService<IConfigurationRepository>();
        
        Assert.That(repo1, Is.SameAs(repo2)); // Same scope
        Assert.That(repo1, Is.Not.SameAs(repo3)); // Different scope
    }
}
```

#### **Test 2: Circular Dependency Detection**
```csharp
[TestFixture]
public class CircularDependencyTests
{
    [Test]
    public void ServiceRegistration_ShouldNotHaveCircularDependencies()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        
        services.AddSystemConfigServices(configuration);
        
        Assert.DoesNotThrow(() =>
        {
            var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
        });
    }
}
```

### Integration Tests

#### **Test 3: Service Resolution Performance**
```csharp
[TestFixture]
public class ServiceResolutionPerformanceTests
{
    private IServiceProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSystemConfigServices(configuration);
        _serviceProvider = services.BuildServiceProvider();
    }
    
    [Test]
    public void ServiceResolution_ShouldMeetPerformanceTarget()
    {
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < 1000; i++)
        {
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetService<IConfigurationManagementService>();
            Assert.That(service, Is.Not.Null);
        }
        
        stopwatch.Stop();
        
        // Should resolve 1000 services in less than 100ms (avg 0.1ms per resolution)
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100));
    }
}
```

## 📊 Acceptance Criteria

### Primary Acceptance Criteria
- [ ] Microsoft.Extensions.DependencyInjection container configured and working
- [ ] All layers have appropriate service registration extension methods
- [ ] Service lifetimes correctly implemented (Singleton, Scoped, Transient)
- [ ] Cross-layer dependencies follow Clean Architecture principles
- [ ] No circular dependency chains exist
- [ ] Container bootstrap integrated into Program.cs
- [ ] Configuration-based service registration working
- [ ] All unit and integration tests passing

### Quality Acceptance Criteria
- [ ] Service resolution performance <1ms for 95% of services
- [ ] Memory overhead of DI container <5MB
- [ ] Application startup time increase <200ms
- [ ] Code follows established naming conventions
- [ ] Extension methods provide clean registration syntax
- [ ] Documentation includes service lifetime rationale

### Technical Acceptance Criteria
- [ ] Generic repository pattern properly registered
- [ ] MediatR configured for CQRS pattern
- [ ] Factory patterns implemented for complex objects
- [ ] Decorator pattern available for cross-cutting concerns
- [ ] Conditional registration based on configuration working
- [ ] Form instances properly managed through DI

## 🚨 Risk Management

### Technical Risks
| **Risk** | **Probability** | **Impact** | **Mitigation** |
|----------|----------------|------------|----------------|
| Circular dependency creation | Medium | High | Automated validation, architecture tests |
| Performance degradation | Low | Medium | Performance testing, benchmarking |
| Memory leaks from DI | Low | High | Proper disposal patterns, memory testing |
| Service lifetime confusion | High | Medium | Clear documentation, team training |

### Implementation Risks
| **Risk** | **Probability** | **Impact** | **Mitigation** |
|----------|----------------|------------|----------------|
| Complex service graph | Medium | Medium | Modular registration, clear separation |
| Configuration complexity | Medium | Low | Simple configuration patterns |
| Team knowledge gap | High | Low | Documentation, code reviews |

## 📚 Resources & References

### Technical Documentation
- [Microsoft.Extensions.DependencyInjection Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [.NET Generic Host Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host)
- [Service Lifetimes Best Practices](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines)

### Architecture References
- Clean Architecture service registration patterns
- SOLID principles in dependency injection
- Composition root pattern implementation

## 🔄 Dependencies & Prerequisites

### Code Dependencies
- [ ] Task 1.1.1 (Solution Structure) completed
- [ ] NuGet packages installed and configured
- [ ] Configuration files structure ready

### Team Prerequisites
- [ ] Understanding of dependency injection principles
- [ ] Knowledge of service lifetime concepts
- [ ] Familiarity with Clean Architecture patterns

## 📈 Success Metrics

### Completion Metrics
- [ ] All service registration extension methods implemented
- [ ] Container bootstrap successfully integrated
- [ ] All layers properly configured for DI
- [ ] Performance benchmarks met

### Quality Metrics
- [ ] Zero circular dependencies detected
- [ ] All resolution performance targets met
- [ ] Memory usage within acceptable limits
- [ ] Code review approval received

## 📝 Definition of Done

### Code Complete
- [ ] All service registration methods implemented and tested
- [ ] Container bootstrap integrated into application startup
- [ ] Extension methods provide clean registration syntax
- [ ] Configuration-based service selection working
- [ ] All dependencies properly registered with correct lifetimes

### Quality Complete
- [ ] All unit tests passing with >95% coverage
- [ ] Integration tests validate service resolution
- [ ] Performance tests meet requirements
- [ ] No circular dependencies detected
- [ ] Code review completed and approved

### Documentation Complete
- [ ] Service registration patterns documented
- [ ] Service lifetime decisions documented
- [ ] Extension method usage examples provided
- [ ] Troubleshooting guide created

---

**Note**: This task establishes the dependency injection foundation that will support all subsequent development. Ensure proper service lifetime management and avoid circular dependencies that could impact application performance and maintainability.

# Tiến độ thực tế (Actual Progress)

## Tổng quan
- Đã cài đặt đầy đủ các package DI, configuration, hosting, MediatR, AutoMapper, Serilog, Redis, FluentValidation cho Presentation layer.
- Đã tạo các extension method cho từng layer (Domain, Application, Infrastructure, Presentation) và gom lại qua ServiceCollectionExtensions.
- Đã cấu hình bootstrap DI container trong Program.cs, tích hợp HostBuilder, đọc appsettings.json, resolve MainForm từ DI.
- Đã đăng ký service mẫu, generic repository, factory, conditional registration, đúng lifetime cho từng layer.
- Đã viết unit test kiểm tra service registration, lifetime, circular dependency.
- Đã viết integration test kiểm tra hiệu năng service resolution.
- Đã tạo tài liệu hướng dẫn, giải thích pattern, lý do chọn lifetime, troubleshooting.
- Đã review checklist nghiệm thu, đảm bảo code, test, tài liệu đều đầy đủ, đúng chuẩn.

## Chi tiết kiểm tra thực tế
- [x] Đã cài đặt các package DI, configuration, MediatR, AutoMapper, Serilog, Redis, FluentValidation vào Presentation.
- [x] Đã tạo các file extension method: DomainServiceRegistration.cs, ApplicationServiceRegistration.cs, InfrastructureServiceRegistration.cs, PresentationServiceRegistration.cs, ServiceCollectionExtensions.cs.
- [x] Đã chỉnh sửa Program.cs để sử dụng HostBuilder, cấu hình DI, resolve MainForm từ container.
- [x] Đã đăng ký service mẫu, generic repository, factory, conditional registration, đúng lifetime.
- [x] Đã có unit test kiểm tra service registration, lifetime, circular dependency (DependencyInjectionTests.cs, UnitTest1.cs).
- [x] Đã có integration test kiểm tra hiệu năng service resolution (ServiceResolutionPerformanceTests.cs).
- [x] Đã có tài liệu hướng dẫn DI (docs/Architecture/DependencyInjection.md).

## Các điểm còn thiếu/cần lưu ý
- Chưa phát hiện điểm thiếu lớn nào so với yêu cầu task.
- Một số service mẫu (interface/class) có thể cần bổ sung chi tiết logic thực tế khi phát triển các tính năng tiếp theo.
- Cần duy trì kiểm tra circular dependency, lifetime, performance khi mở rộng hệ thống.

## Kết luận
Task đã hoàn thành toàn diện, đáp ứng mọi tiêu chí nghiệm thu, kỹ thuật và chất lượng. Sẵn sàng cho các task tiếp theo.