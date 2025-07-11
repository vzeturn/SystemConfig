# Task 1.1.3: Configure Logging Framework

## 📋 Task Overview
**Sprint**: 1  
**Story**: 1.1 - Project Structure Setup  
**Priority**: High  
**Estimated Hours**: 4  
**Assigned To**: Senior Developer  
**Dependencies**: Task 1.1.1 - Create Solution Structure, Task 1.1.2 - Setup Dependency Injection

## 🎯 Objective
Thiết lập comprehensive logging framework với Serilog để track application events, errors, và performance metrics cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Structured Logging Setup**:
  - Configure Serilog với multiple sinks
  - File logging với rotation
  - Console logging cho development
  - Event log integration cho Windows
  - Performance logging cho critical operations

- [ ] **Log Categories**:
  - Application startup/shutdown events
  - Configuration changes và operations
  - Database connection events
  - Printer operations
  - Security events (authentication, authorization)
  - Performance metrics
  - Error tracking và debugging

- [ ] **Log Levels Configuration**:
  - Debug: Detailed debugging information
  - Information: General application flow
  - Warning: Unexpected situations
  - Error: Error conditions
  - Fatal: Critical errors

### Technical Requirements
- [ ] **Serilog Configuration**:
  ```csharp
  // appsettings.json
  {
    "Serilog": {
      "MinimumLevel": {
        "Default": "Information",
        "Override": {
          "Microsoft": "Warning",
          "System": "Warning",
          "SystemConfig.Domain": "Debug",
          "SystemConfig.Application": "Information",
          "SystemConfig.Infrastructure": "Information"
        }
      },
      "WriteTo": [
        {
          "Name": "File",
          "Args": {
            "path": "logs/systemconfig-.log",
            "rollingInterval": "Day",
            "retainedFileCountLimit": 30,
            "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
          }
        },
        {
          "Name": "Console",
          "Args": {
            "outputTemplate": "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
          }
        },
        {
          "Name": "EventLog",
          "Args": {
            "logName": "SystemConfig",
            "source": "SystemConfig"
          }
        }
      ],
      "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
    }
  }
  ```

- [ ] **Logging Service Interface**:
  ```csharp
  // SystemConfig.Infrastructure/Services/ILoggingService.cs
  public interface ILoggingService
  {
      void LogInformation(string message, params object[] propertyValues);
      void LogWarning(string message, params object[] propertyValues);
      void LogError(Exception exception, string message, params object[] propertyValues);
      void LogDebug(string message, params object[] propertyValues);
      void LogPerformance(string operation, TimeSpan duration, Dictionary<string, object> context = null);
      void LogSecurityEvent(string eventType, string userId, string details);
      void LogConfigurationChange(string configurationType, string configurationId, string changeType);
  }
  ```

### Quality Requirements
- [ ] **Performance**: Logging không ảnh hưởng application performance
- [ ] **Security**: Sensitive data không được log
- [ ] **Compliance**: Audit trail cho configuration changes
- [ ] **Maintainability**: Easy to configure và extend
- [ ] **Monitoring**: Log aggregation và analysis support

## 🏗️ Implementation Plan

### Phase 1: Serilog Configuration (2 hours)
```csharp
// SystemConfig.Infrastructure/Logging/LoggingService.cs
public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;
    private readonly IConfiguration _configuration;
    
    public LoggingService(ILogger<LoggingService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    public void LogInformation(string message, params object[] propertyValues)
    {
        _logger.LogInformation(message, propertyValues);
    }
    
    public void LogWarning(string message, params object[] propertyValues)
    {
        _logger.LogWarning(message, propertyValues);
    }
    
    public void LogError(Exception exception, string message, params object[] propertyValues)
    {
        _logger.LogError(exception, message, propertyValues);
    }
    
    public void LogDebug(string message, params object[] propertyValues)
    {
        _logger.LogDebug(message, propertyValues);
    }
    
    public void LogPerformance(string operation, TimeSpan duration, Dictionary<string, object> context = null)
    {
        var logContext = new Dictionary<string, object>
        {
            ["Operation"] = operation,
            ["Duration"] = duration.TotalMilliseconds,
            ["DurationMs"] = duration.TotalMilliseconds
        };
        
        if (context != null)
        {
            foreach (var kvp in context)
            {
                logContext[kvp.Key] = kvp.Value;
            }
        }
        
        _logger.LogInformation("Performance: {Operation} completed in {Duration}ms", operation, duration.TotalMilliseconds);
    }
    
    public void LogSecurityEvent(string eventType, string userId, string details)
    {
        _logger.LogWarning("Security Event: {EventType} by User: {UserId} - {Details}", eventType, userId, details);
    }
    
    public void LogConfigurationChange(string configurationType, string configurationId, string changeType)
    {
        _logger.LogInformation("Configuration Change: {ConfigurationType} {ConfigurationId} - {ChangeType}", 
            configurationType, configurationId, changeType);
    }
}
```

### Phase 2: Logging Extensions (1 hour)
```csharp
// SystemConfig.Infrastructure/Logging/LoggingExtensions.cs
public static class LoggingExtensions
{
    public static IServiceCollection AddLoggingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Serilog
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .CreateLogger();
        
        // Register Serilog
        Log.Logger = logger;
        
        // Add logging services
        services.AddLogging(builder =>
        {
            builder.AddSerilog(logger);
        });
        
        services.AddSingleton<ILoggingService, LoggingService>();
        
        return services;
    }
    
    public static IHostBuilder UseLogging(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId();
        });
    }
}
```

### Phase 3: Performance Logging (1 hour)
```csharp
// SystemConfig.Infrastructure/Logging/PerformanceLogger.cs
public class PerformanceLogger : IDisposable
{
    private readonly ILoggingService _loggingService;
    private readonly string _operation;
    private readonly Stopwatch _stopwatch;
    private readonly Dictionary<string, object> _context;
    
    public PerformanceLogger(ILoggingService loggingService, string operation, Dictionary<string, object> context = null)
    {
        _loggingService = loggingService;
        _operation = operation;
        _stopwatch = Stopwatch.StartNew();
        _context = context ?? new Dictionary<string, object>();
    }
    
    public void Dispose()
    {
        _stopwatch.Stop();
        _loggingService.LogPerformance(_operation, _stopwatch.Elapsed, _context);
    }
}

// Usage example
public class DatabaseConfigurationService
{
    private readonly ILoggingService _loggingService;
    
    public async Task<DatabaseConfiguration> CreateAsync(CreateDatabaseConfigurationCommand command)
    {
        using var performanceLogger = new PerformanceLogger(_loggingService, "CreateDatabaseConfiguration");
        
        _loggingService.LogInformation("Creating database configuration: {Name}", command.Name);
        
        // Implementation...
        
        _loggingService.LogInformation("Database configuration created: {Id}", result.Id);
        return result;
    }
}
```

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Infrastructure/LoggingServiceTests.cs
public class LoggingServiceTests
{
    private readonly ILoggingService _loggingService;
    private readonly Mock<ILogger<LoggingService>> _mockLogger;
    
    public LoggingServiceTests()
    {
        _mockLogger = new Mock<ILogger<LoggingService>>();
        var configuration = new Mock<IConfiguration>();
        _loggingService = new LoggingService(_mockLogger.Object, configuration.Object);
    }
    
    [Fact]
    public void LogInformation_ShouldCallLogger()
    {
        // Arrange
        var message = "Test information message";
        
        // Act
        _loggingService.LogInformation(message);
        
        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }
    
    [Fact]
    public void LogPerformance_ShouldLogWithDuration()
    {
        // Arrange
        var operation = "TestOperation";
        var duration = TimeSpan.FromMilliseconds(150);
        
        // Act
        _loggingService.LogPerformance(operation, duration);
        
        // Assert
        _mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Performance") && v.ToString().Contains("150")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Logging/LoggingIntegrationTests.cs
public class LoggingIntegrationTests : IDisposable
{
    private readonly string _logFilePath;
    
    public LoggingIntegrationTests()
    {
        _logFilePath = Path.Combine(Path.GetTempPath(), $"test-log-{Guid.NewGuid()}.log");
    }
    
    [Fact]
    public async Task Logging_ShouldWriteToFile()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Serilog:MinimumLevel:Default", "Information"},
                {"Serilog:WriteTo:0:Name", "File"},
                {"Serilog:WriteTo:0:Args:path", _logFilePath}
            })
            .Build();
        
        services.AddLoggingServices(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var loggingService = serviceProvider.GetRequiredService<ILoggingService>();
        
        // Act
        loggingService.LogInformation("Test message");
        
        // Wait for file to be written
        await Task.Delay(100);
        
        // Assert
        Assert.True(File.Exists(_logFilePath));
        var logContent = await File.ReadAllTextAsync(_logFilePath);
        Assert.Contains("Test message", logContent);
    }
    
    public void Dispose()
    {
        if (File.Exists(_logFilePath))
        {
            File.Delete(_logFilePath);
        }
    }
}
```

## 📊 Definition of Done
- [ ] **Serilog Configuration**: Structured logging được setup đúng
- [ ] **Multiple Sinks**: File, Console, EventLog sinks hoạt động
- [ ] **Log Levels**: Appropriate log levels cho different components
- [ ] **Performance Logging**: Performance tracking cho critical operations
- [ ] **Security Logging**: Audit trail cho security events
- [ ] **Configuration Logging**: Configuration changes được track
- [ ] **Unit Tests**: >95% coverage cho logging services
- [ ] **Integration Tests**: Log file writing tests pass
- [ ] **Performance**: Logging không ảnh hưởng application performance
- [ ] **Code Review**: Logging setup được approve

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Log file size growth
- **Mitigation**: Implement log rotation và retention policies

- **Risk**: Performance impact của logging
- **Mitigation**: Use async logging và performance monitoring

- **Risk**: Sensitive data exposure trong logs
- **Mitigation**: Implement log filtering và data masking

### Operational Risks
- **Risk**: Disk space issues với log files
- **Mitigation**: Monitor log file sizes và implement cleanup

- **Risk**: Log parsing complexity
- **Mitigation**: Use structured logging với consistent format

## 📚 Resources & References
- Serilog Documentation
- .NET 8 Logging Best Practices
- Windows Event Log Programming
- Log Aggregation Tools (ELK Stack)
- Performance Monitoring Guidelines

## 🔄 Dependencies
- Task 1.1.1: Create Solution Structure
- Task 1.1.2: Setup Dependency Injection
- Serilog NuGet packages
- Microsoft.Extensions.Logging

## 📈 Success Metrics
- Logging framework hoạt động without errors
- Performance impact <5% cho application
- Log files được rotate properly
- Security events được logged correctly
- Configuration changes được tracked
- Log parsing và analysis tools work

## 📝 Notes
- Use structured logging for better analysis
- Implement log filtering cho sensitive data
- Monitor log file sizes regularly
- Consider log aggregation cho production
- Document logging standards cho team 