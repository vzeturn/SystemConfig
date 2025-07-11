# Task 1.1.2: Setup Dependency Injection

## 📋 Task Overview
**Sprint**: 1  
**Story**: 1.1 - Project Structure Setup  
**Priority**: High  
**Estimated Hours**: 6  
**Assigned To**: Senior Developer  
**Dependencies**: Task 1.1.1 - Create Solution Structure

## 🎯 Objective
Thiết lập dependency injection container với Microsoft.Extensions.DependencyInjection để quản lý dependencies và đảm bảo loose coupling giữa các layer theo Clean Architecture.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Core DI Container Setup**:
  - Configure Microsoft.Extensions.DependencyInjection
  - Register all services với appropriate lifetimes
  - Setup service interfaces và implementations
  - Configure logging services (Serilog)
  - Setup configuration services

- [ ] **Service Registration by Layer**:
  - **Domain Services**: Domain validators, business rules
  - **Application Services**: CQRS handlers, use cases
  - **Infrastructure Services**: Repositories, external services
  - **Presentation Services**: Form controllers, UI services

- [ ] **Lifetime Management**:
  - Singleton: Configuration services, logging services
  - Scoped: Repository services, unit of work
  - Transient: Command/Query handlers, validators

### Technical Requirements
- [ ] **DI Container Configuration**:
  ```csharp
  // Program.cs hoặc Startup.cs
  var services = new ServiceCollection();
  
  // Register core services
  services.AddLogging(builder => 
  {
      builder.AddSerilog(new LoggerConfiguration()
          .WriteTo.File("logs/systemconfig.log")
          .WriteTo.Console()
          .CreateLogger());
  });
  
  services.AddConfiguration(configuration);
  services.AddValidation();
  ```

- [ ] **Service Registration Structure**:
  ```csharp
  // Register application services
  services.AddScoped<IDatabaseConfigurationService, DatabaseConfigurationService>();
  services.AddScoped<IPrinterConfigurationService, PrinterConfigurationService>();
  services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
  
  // Register infrastructure services
  services.AddSingleton<IRegistryService, RegistryService>();
  services.AddSingleton<IEncryptionService, EncryptionService>();
  services.AddScoped<IUnitOfWork, UnitOfWork>();
  services.AddScoped<IRepository<DatabaseConfiguration>, DatabaseConfigurationRepository>();
  services.AddScoped<IRepository<PrinterConfiguration>, PrinterConfigurationRepository>();
  services.AddScoped<IRepository<SystemConfiguration>, SystemConfigurationRepository>();
  
  // Register CQRS handlers
  services.AddMediatR(cfg => 
  {
      cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
  });
  
  // Register validation services
  services.AddScoped<IValidationService, ValidationService>();
  services.AddScoped<IConfigurationValidator, ConfigurationValidator>();
  ```

- [ ] **Configuration Binding**:
  ```csharp
  // AppSettings binding
  services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
  services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
  services.Configure<SecuritySettings>(configuration.GetSection("Security"));
  services.Configure<LoggingSettings>(configuration.GetSection("Logging"));
  ```

### Quality Requirements
- [ ] **No Circular Dependencies**: Strict dependency direction enforcement
- [ ] **Proper Service Lifetime**: Appropriate lifetime cho từng service type
- [ ] **Testability**: Easy to mock và test services
- [ ] **Performance**: Efficient service resolution
- [ ] **Error Handling**: Graceful handling of service resolution errors

## 🏗️ Implementation Plan

### Phase 1: Core DI Setup (2 hours)
```csharp
// SystemConfig.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register infrastructure services
        services.AddSingleton<IRegistryService, RegistryService>();
        services.AddSingleton<IEncryptionService, EncryptionService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILoggingService, LoggingService>();
        
        // Register repositories
        services.AddScoped<IRepository<DatabaseConfiguration>, DatabaseConfigurationRepository>();
        services.AddScoped<IRepository<PrinterConfiguration>, PrinterConfigurationRepository>();
        services.AddScoped<IRepository<SystemConfiguration>, SystemConfigurationRepository>();
        
        // Register factories
        services.AddScoped<IDatabaseConnectionFactory, DatabaseConnectionFactory>();
        services.AddScoped<IPrinterServiceFactory, PrinterServiceFactory>();
        
        return services;
    }
    
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IDatabaseConfigurationService, DatabaseConfigurationService>();
        services.AddScoped<IPrinterConfigurationService, PrinterConfigurationService>();
        services.AddScoped<ISystemConfigurationService, SystemConfigurationService>();
        
        // Register CQRS handlers
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });
        
        // Register validation
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IConfigurationValidator, ConfigurationValidator>();
        
        return services;
    }
    
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        // Register domain services
        services.AddScoped<IDomainService, DomainService>();
        services.AddScoped<IConfigurationValidator, ConfigurationValidator>();
        
        return services;
    }
}
```

### Phase 2: Service Interfaces & Implementations (2 hours)
```csharp
// SystemConfig.Application/Services/IDatabaseConfigurationService.cs
public interface IDatabaseConfigurationService
{
    Task<DatabaseConfiguration> GetByIdAsync(Guid id);
    Task<IEnumerable<DatabaseConfiguration>> GetAllAsync();
    Task<DatabaseConfiguration> CreateAsync(CreateDatabaseConfigurationCommand command);
    Task<DatabaseConfiguration> UpdateAsync(UpdateDatabaseConfigurationCommand command);
    Task DeleteAsync(Guid id);
    Task<bool> TestConnectionAsync(Guid id);
    Task<HealthStatus> GetHealthStatusAsync(Guid id);
}

// SystemConfig.Application/Services/DatabaseConfigurationService.cs
public class DatabaseConfigurationService : IDatabaseConfigurationService
{
    private readonly IRepository<DatabaseConfiguration> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly ILogger<DatabaseConfigurationService> _logger;
    
    public DatabaseConfigurationService(
        IRepository<DatabaseConfiguration> repository,
        IUnitOfWork unitOfWork,
        IValidationService validationService,
        ILogger<DatabaseConfigurationService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _logger = logger;
    }
    
    public async Task<DatabaseConfiguration> CreateAsync(CreateDatabaseConfigurationCommand command)
    {
        _logger.LogInformation("Creating database configuration: {Name}", command.Name);
        
        // Validate command
        await _validationService.ValidateAsync(command);
        
        // Create configuration
        var configuration = DatabaseConfiguration.Create(
            command.Name, 
            command.ConnectionSettings, 
            command.DatabaseType);
        
        // Save to repository
        await _repository.AddAsync(configuration);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Database configuration created: {Id}", configuration.Id);
        return configuration;
    }
    
    // Implement other methods...
}
```

### Phase 3: Configuration & Validation Setup (2 hours)
```csharp
// SystemConfig.Infrastructure/Configuration/ConfigurationExtensions.cs
public static class ConfigurationExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration sections
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
        services.Configure<SecuritySettings>(configuration.GetSection("Security"));
        services.Configure<LoggingSettings>(configuration.GetSection("Logging"));
        
        // Register configuration services
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        
        return services;
    }
}

// SystemConfig.Application/Validation/IValidationService.cs
public interface IValidationService
{
    Task<ValidationResult> ValidateAsync<T>(T command) where T : class;
    Task<bool> IsValidAsync<T>(T command) where T : class;
    Task<IEnumerable<ValidationError>> GetValidationErrorsAsync<T>(T command) where T : class;
}

// SystemConfig.Application/Validation/ValidationService.cs
public class ValidationService : IValidationService
{
    private readonly IEnumerable<IValidator> _validators;
    private readonly ILogger<ValidationService> _logger;
    
    public ValidationService(IEnumerable<IValidator> validators, ILogger<ValidationService> logger)
    {
        _validators = validators;
        _logger = logger;
    }
    
    public async Task<ValidationResult> ValidateAsync<T>(T command) where T : class
    {
        var validator = _validators.OfType<IValidator<T>>().FirstOrDefault();
        if (validator == null)
        {
            _logger.LogWarning("No validator found for type: {Type}", typeof(T).Name);
            return ValidationResult.Success();
        }
        
        var result = await validator.ValidateAsync(command);
        return result;
    }
}
```

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Infrastructure/DependencyInjectionTests.cs
public class DependencyInjectionTests
{
    [Fact]
    public void ServiceCollection_WhenConfigured_ShouldResolveAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"AppSettings:Database:DefaultConnectionString", "test"},
                {"AppSettings:Security:EncryptionKey", "test-key"},
                {"AppSettings:Logging:LogLevel", "Information"}
            })
            .Build();
        
        // Act
        services.AddInfrastructure(configuration);
        services.AddApplication();
        services.AddDomain();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        Assert.NotNull(serviceProvider.GetService<IRegistryService>());
        Assert.NotNull(serviceProvider.GetService<IEncryptionService>());
        Assert.NotNull(serviceProvider.GetService<IUnitOfWork>());
        Assert.NotNull(serviceProvider.GetService<IDatabaseConfigurationService>());
        Assert.NotNull(serviceProvider.GetService<IPrinterConfigurationService>());
        Assert.NotNull(serviceProvider.GetService<ISystemConfigurationService>());
    }
    
    [Fact]
    public void ServiceLifetime_WhenResolved_ShouldHaveCorrectLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        services.AddApplication();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Act & Assert
        var registryService1 = serviceProvider.GetService<IRegistryService>();
        var registryService2 = serviceProvider.GetService<IRegistryService>();
        Assert.Same(registryService1, registryService2); // Singleton
        
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();
        
        var unitOfWork1 = scope1.ServiceProvider.GetService<IUnitOfWork>();
        var unitOfWork2 = scope2.ServiceProvider.GetService<IUnitOfWork>();
        Assert.NotSame(unitOfWork1, unitOfWork2); // Scoped
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Services/DatabaseConfigurationServiceTests.cs
public class DatabaseConfigurationServiceTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDatabaseConfigurationService _service;
    
    public DatabaseConfigurationServiceTests()
    {
        var services = new ServiceCollection();
        var configuration = CreateTestConfiguration();
        
        services.AddInfrastructure(configuration);
        services.AddApplication();
        services.AddDomain();
        
        _serviceProvider = services.BuildServiceProvider();
        _service = _serviceProvider.GetRequiredService<IDatabaseConfigurationService>();
    }
    
    [Fact]
    public async Task CreateAsync_WithValidCommand_ShouldCreateConfiguration()
    {
        // Arrange
        var command = new CreateDatabaseConfigurationCommand
        {
            Name = "Test Config",
            ConnectionSettings = new ConnectionSettings("localhost", "testdb", "user", "pass"),
            DatabaseType = DatabaseType.SqlServer
        };
        
        // Act
        var result = await _service.CreateAsync(command);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.DatabaseType, result.DatabaseType);
    }
}
```

## 📊 Definition of Done
- [ ] **DI Container**: Microsoft.Extensions.DependencyInjection được configure đúng
- [ ] **Service Registration**: Tất cả services được register với appropriate lifetimes
- [ ] **Configuration Binding**: AppSettings được bind đúng
- [ ] **Logging Setup**: Serilog được configure và hoạt động
- [ ] **Validation Pipeline**: Validation services được setup
- [ ] **Unit Tests**: >95% coverage cho DI setup
- [ ] **Integration Tests**: Service resolution tests pass 100%
- [ ] **No Circular Dependencies**: Dependency graph được validate
- [ ] **Code Review**: DI setup được approve
- [ ] **Documentation**: Service registration docs hoàn thành

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Circular dependencies trong service registration
- **Mitigation**: Use dependency graph analysis tools

- **Risk**: Service lifetime issues (memory leaks)
- **Mitigation**: Document lifetime requirements và review

- **Risk**: Performance issues với service resolution
- **Mitigation**: Use appropriate service lifetimes và caching

### Quality Risks
- **Risk**: Services không được register đúng
- **Mitigation**: Comprehensive unit tests cho service resolution

- **Risk**: Configuration binding errors
- **Mitigation**: Validate configuration loading

## 📚 Resources & References
- Microsoft.Extensions.DependencyInjection Documentation
- .NET 8 Dependency Injection Best Practices
- Service Lifetime Guidelines
- DI Container Performance Tips
- Clean Architecture DI Patterns

## 🔄 Dependencies
- Task 1.1.1: Create Solution Structure
- .NET 8 SDK
- Microsoft.Extensions.DependencyInjection package
- Serilog packages
- MediatR package

## 📈 Success Metrics
- All services resolve correctly without errors
- No circular dependency errors detected
- Proper service lifetime management verified
- Configuration loads successfully
- Logging works properly
- Validation pipeline functions correctly
- Performance benchmarks met

## 📝 Notes
- Use constructor injection where possible
- Avoid service locator pattern
- Document service lifetimes clearly
- Consider performance implications of service lifetimes
- Regular dependency graph analysis
- Monitor service resolution performance 