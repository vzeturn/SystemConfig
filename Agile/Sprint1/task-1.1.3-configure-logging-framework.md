# Task 1.1.3: Configure Logging Framework

## 📋 Task Information

| **Attribute** | **Value** |
|---------------|-----------|
| **Task ID** | 1.1.3 |
| **Sprint** | Sprint 1 (Weeks 1-2) |
| **Story** | 1.1 - Project Architecture Setup |
| **Priority** | High |
| **Story Points** | 4 |
| **Estimated Hours** | 4 |
| **Assigned To** | Senior Developer |
| **Status** | Not Started |
| **Dependencies** | Task 1.1.2 (Setup Dependency Injection) |

## 🎯 Objective

Implement comprehensive structured logging using Serilog with multiple output targets, performance monitoring, security audit trails, and enterprise-grade log management for the POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements

#### **Structured Logging Setup**
- [ ] Configure Serilog as primary logging framework
- [ ] Implement multiple log output targets (File, Console, EventLog)
- [ ] Setup structured logging with JSON formatting
- [ ] Configure log level hierarchy and filtering
- [ ] Implement correlation IDs for request tracking

#### **Log Categories and Levels**
- [ ] **Application Events** (Information): Business operation logging
- [ ] **Security Events** (Warning/Error): Authentication, authorization, access
- [ ] **Performance Events** (Information): Operation timing and metrics
- [ ] **Error Events** (Error/Critical): Exceptions and failures
- [ ] **Audit Events** (Information): Configuration changes and user actions
- [ ] **Debug Events** (Debug): Development and troubleshooting

#### **Output Targets Configuration**
- [ ] **Rolling File Logs**: Daily rotation with 30-day retention
- [ ] **Console Output**: Development and debugging
- [ ] **Windows Event Log**: System-level event integration
- [ ] **Performance Counters**: Custom metrics for monitoring
- [ ] **Database Logs**: Critical events for audit compliance

### Technical Requirements

#### **Serilog Configuration**
```csharp
// Logging/SerilogConfiguration.cs
public static class SerilogConfiguration
{
    public static ILogger CreateLogger(IConfiguration configuration)
    {
        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithUserName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithCorrelationId()
            .WriteTo.File(
                path: Path.Combine("logs", "application-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 100_000_000,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {SourceContext} {Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.Console(
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext:l} {Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Debug)
            .WriteTo.EventLog(
                source: "SystemConfig",
                logName: "Application",
                restrictedToMinimumLevel: LogEventLevel.Warning)
            .WriteTo.Conditional(
                condition: evt => evt.Level >= LogEventLevel.Error,
                configureSink: sink => sink.File(
                    path: Path.Combine("logs", "errors-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 90))
            .Filter.ByExcluding(Matching.FromSource("Microsoft"))
            .Filter.ByExcluding(Matching.FromSource("System"));
            
        return loggerConfig.CreateLogger();
    }
}
```

#### **Structured Logging Models**
```csharp
// Logging/Models/LogEvent.cs
public static class LogEvents
{
    // Application Events
    public const string ConfigurationCreated = "Configuration.Created";
    public const string ConfigurationUpdated = "Configuration.Updated";
    public const string ConfigurationDeleted = "Configuration.Deleted";
    public const string BackupCreated = "Backup.Created";
    public const string BackupRestored = "Backup.Restored";
    
    // Security Events
    public const string UserAuthenticated = "Security.UserAuthenticated";
    public const string UnauthorizedAccess = "Security.UnauthorizedAccess";
    public const string PermissionDenied = "Security.PermissionDenied";
    public const string DataEncrypted = "Security.DataEncrypted";
    public const string DataDecrypted = "Security.DataDecrypted";
    
    // Performance Events
    public const string OperationTiming = "Performance.OperationTiming";
    public const string CacheHit = "Performance.CacheHit";
    public const string CacheMiss = "Performance.CacheMiss";
    public const string DatabaseQuery = "Performance.DatabaseQuery";
    
    // Error Events
    public const string Exception = "Error.Exception";
    public const string ValidationFailed = "Error.ValidationFailed";
    public const string ConfigurationError = "Error.ConfigurationError";
    public const string SystemError = "Error.SystemError";
}

public static class LogProperties
{
    public const string UserId = "UserId";
    public const string OperationId = "OperationId";
    public const string ConfigurationId = "ConfigurationId";
    public const string Duration = "Duration";
    public const string ErrorCode = "ErrorCode";
    public const string IPAddress = "IPAddress";
    public const string UserAgent = "UserAgent";
    public const string RequestPath = "RequestPath";
}
```

### Quality Requirements

#### **Performance Standards**
- [ ] Logging overhead <5% of operation time
- [ ] Asynchronous logging for all file operations
- [ ] Buffer size optimization for high-throughput scenarios
- [ ] Memory usage monitoring and alerting

#### **Security Standards**
- [ ] Sensitive data masking in logs
- [ ] Log file encryption for confidential data
- [ ] Access control for log files and directories
- [ ] Audit trail integrity verification

## 🏗️ Implementation Plan

### Phase 1: Core Logging Setup (1.5 hours)

#### **Step 1: Install Serilog Dependencies**
```xml
<!-- All Projects -->
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.EventLog" Version="3.1.0" />
<PackageReference Include="Serilog.Sinks.PerformanceCounters" Version="2.0.0" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="2.3.0" />
<PackageReference Include="Serilog.Enrichers.Process" Version="2.0.2" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="3.1.0" />
<PackageReference Include="Serilog.Enrichers.CorrelationId" Version="3.0.1" />
```

#### **Step 2: Configuration Setup**
```json
// appsettings.json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.File",
      "Serilog.Sinks.Console",
      "Serilog.Sinks.EventLog"
    ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "SystemConfig": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/application-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 104857600,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {SourceContext} {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext:l} {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "EventLog",
        "Args": {
          "source": "SystemConfig",
          "logName": "Application",
          "restrictedToMinimumLevel": "Warning"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithProcessId",
      "WithThreadId",
      "WithCorrelationId"
    ],
    "Properties": {
      "Application": "SystemConfig",
      "Version": "1.0.0"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  }
}
```

### Phase 2: Logging Services Implementation (1.5 hours)

#### **Step 3: Logging Service Interfaces**
```csharp
// Application/Interfaces/IApplicationLogger.cs
public interface IApplicationLogger<T>
{
    void LogConfigurationCreated(Guid configurationId, string configurationType, string userId);
    void LogConfigurationUpdated(Guid configurationId, string configurationType, string userId, object changes);
    void LogConfigurationDeleted(Guid configurationId, string configurationType, string userId);
    void LogSecurityEvent(string eventType, string userId, string details, bool success);
    void LogPerformanceMetric(string operation, TimeSpan duration, Dictionary<string, object> properties = null);
    void LogBusinessRuleViolation(string rule, string context, object data);
    void LogAuditEvent(string action, string resource, string userId, object details);
}

// Infrastructure/Logging/IStructuredLogger.cs
public interface IStructuredLogger
{
    void LogInformation(string messageTemplate, params object[] propertyValues);
    void LogWarning(string messageTemplate, params object[] propertyValues);
    void LogError(Exception exception, string messageTemplate, params object[] propertyValues);
    void LogDebug(string messageTemplate, params object[] propertyValues);
    
    void LogWithProperties(LogEventLevel level, string messageTemplate, 
        Dictionary<string, object> properties, params object[] propertyValues);
    
    IDisposable BeginScope(string operationName, Dictionary<string, object> properties = null);
}
```

#### **Step 4: Logging Service Implementation**
```csharp
// Infrastructure/Logging/ApplicationLogger.cs
public class ApplicationLogger<T> : IApplicationLogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly ICorrelationIdProvider _correlationIdProvider;
    private readonly IUserContextProvider _userContextProvider;

    public ApplicationLogger(ILogger<T> logger, 
        ICorrelationIdProvider correlationIdProvider,
        IUserContextProvider userContextProvider)
    {
        _logger = logger;
        _correlationIdProvider = correlationIdProvider;
        _userContextProvider = userContextProvider;
    }

    public void LogConfigurationCreated(Guid configurationId, string configurationType, string userId)
    {
        _logger.LogInformation(LogEvents.ConfigurationCreated,
            "Configuration {ConfigurationType} created with ID {ConfigurationId} by user {UserId}",
            configurationType, configurationId, userId);
    }

    public void LogConfigurationUpdated(Guid configurationId, string configurationType, 
        string userId, object changes)
    {
        _logger.LogInformation(LogEvents.ConfigurationUpdated,
            "Configuration {ConfigurationType} with ID {ConfigurationId} updated by user {UserId}. Changes: {@Changes}",
            configurationType, configurationId, userId, changes);
    }

    public void LogSecurityEvent(string eventType, string userId, string details, bool success)
    {
        var level = success ? LogEventLevel.Information : LogEventLevel.Warning;
        
        _logger.Log(level, "Security event {EventType} for user {UserId}: {Details}. Success: {Success}",
            eventType, userId, details, success);
    }

    public void LogPerformanceMetric(string operation, TimeSpan duration, 
        Dictionary<string, object> properties = null)
    {
        var logProperties = new Dictionary<string, object>
        {
            [LogProperties.Duration] = duration.TotalMilliseconds,
            [LogProperties.OperationId] = _correlationIdProvider.GetCorrelationId()
        };

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                logProperties[prop.Key] = prop.Value;
            }
        }

        _logger.LogInformation(LogEvents.OperationTiming,
            "Operation {Operation} completed in {Duration}ms",
            operation, duration.TotalMilliseconds);
    }

    public void LogAuditEvent(string action, string resource, string userId, object details)
    {
        _logger.LogInformation("AUDIT: User {UserId} performed {Action} on {Resource}. Details: {@Details}",
            userId, action, resource, details);
    }

    public void LogBusinessRuleViolation(string rule, string context, object data)
    {
        _logger.LogWarning("Business rule violation: {Rule} in context {Context}. Data: {@Data}",
            rule, context, data);
    }
}

// Infrastructure/Logging/StructuredLogger.cs
public class StructuredLogger : IStructuredLogger
{
    private readonly ILogger<StructuredLogger> _logger;

    public StructuredLogger(ILogger<StructuredLogger> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string messageTemplate, params object[] propertyValues)
    {
        _logger.LogInformation(messageTemplate, propertyValues);
    }

    public void LogWarning(string messageTemplate, params object[] propertyValues)
    {
        _logger.LogWarning(messageTemplate, propertyValues);
    }

    public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
    {
        _logger.LogError(exception, messageTemplate, propertyValues);
    }

    public void LogDebug(string messageTemplate, params object[] propertyValues)
    {
        _logger.LogDebug(messageTemplate, propertyValues);
    }

    public void LogWithProperties(LogEventLevel level, string messageTemplate, 
        Dictionary<string, object> properties, params object[] propertyValues)
    {
        using var scope = _logger.BeginScope(properties);
        _logger.Log((Microsoft.Extensions.Logging.LogLevel)level, messageTemplate, propertyValues);
    }

    public IDisposable BeginScope(string operationName, Dictionary<string, object> properties = null)
    {
        var scopeProperties = new Dictionary<string, object>
        {
            ["OperationName"] = operationName,
            ["OperationId"] = Guid.NewGuid().ToString()
        };

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                scopeProperties[prop.Key] = prop.Value;
            }
        }

        return _logger.BeginScope(scopeProperties);
    }
}
```

### Phase 3: Enrichers and Filters (1 hour)

#### **Step 5: Custom Enrichers**
```csharp
// Infrastructure/Logging/Enrichers/UserContextEnricher.cs
public class UserContextEnricher : ILogEventEnricher
{
    private readonly IUserContextProvider _userContextProvider;

    public UserContextEnricher(IUserContextProvider userContextProvider)
    {
        _userContextProvider = userContextProvider;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var userContext = _userContextProvider.GetCurrentUser();
        if (userContext != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userContext.UserId));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserName", userContext.UserName));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserRole", userContext.Role));
        }
    }
}

// Infrastructure/Logging/Enrichers/PerformanceEnricher.cs
public class PerformanceEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Add memory usage
        var memoryUsage = GC.GetTotalMemory(false);
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MemoryUsage", memoryUsage));

        // Add processor time
        using var process = Process.GetCurrentProcess();
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ProcessorTime", 
            process.TotalProcessorTime.TotalMilliseconds));
    }
}

// Infrastructure/Logging/Filters/SensitiveDataFilter.cs
public class SensitiveDataFilter : ILogEventFilter
{
    private static readonly string[] SensitiveProperties = 
    {
        "password", "connectionstring", "key", "token", "secret", "credential"
    };

    public bool IsEnabled(LogEvent logEvent)
    {
        return !ContainsSensitiveData(logEvent);
    }

    private bool ContainsSensitiveData(LogEvent logEvent)
    {
        foreach (var property in logEvent.Properties)
        {
            if (SensitiveProperties.Any(sensitive => 
                property.Key.Contains(sensitive, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }
        return false;
    }
}
```

### Phase 4: Integration and Testing (0 hour)

#### **Step 6: DI Registration**
```csharp
// Infrastructure/DependencyInjection/LoggingServiceRegistration.cs
public static class LoggingServiceRegistration
{
    public static IServiceCollection AddLoggingServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Configure Serilog
        Log.Logger = SerilogConfiguration.CreateLogger(configuration);
        
        // Register Serilog with Microsoft.Extensions.Logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        // Register custom logging services
        services.AddScoped(typeof(IApplicationLogger<>), typeof(ApplicationLogger<>));
        services.AddScoped<IStructuredLogger, StructuredLogger>();
        
        // Register enrichers
        services.AddSingleton<ILogEventEnricher, UserContextEnricher>();
        services.AddSingleton<ILogEventEnricher, PerformanceEnricher>();
        
        // Register context providers
        services.AddScoped<IUserContextProvider, WindowsUserContextProvider>();
        services.AddScoped<ICorrelationIdProvider, GuidCorrelationIdProvider>();
        
        return services;
    }
}

// Update main service registration
public static IServiceCollection AddSystemConfigServices(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    services.AddLoggingServices(configuration);
    // ... other services
    
    return services;
}
```

## 🧪 Testing Strategy

### Unit Tests

#### **Test 1: Logging Configuration Validation**
```csharp
[TestFixture]
public class LoggingConfigurationTests
{
    [Test]
    public void SerilogConfiguration_ShouldCreateValidLogger()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var logger = SerilogConfiguration.CreateLogger(configuration);

        Assert.That(logger, Is.Not.Null);
        Assert.That(logger, Is.InstanceOf<ILogger>());
    }

    [Test]
    public void LoggerConfiguration_ShouldHaveCorrectSinks()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Serilog:WriteTo:0:Name", "File"},
                {"Serilog:WriteTo:1:Name", "Console"},
                {"Serilog:WriteTo:2:Name", "EventLog"}
            })
            .Build();

        Assert.DoesNotThrow(() =>
        {
            var logger = SerilogConfiguration.CreateLogger(configuration);
            logger.Information("Test message");
        });
    }
}
```

#### **Test 2: Application Logger Tests**
```csharp
[TestFixture]
public class ApplicationLoggerTests
{
    private Mock<ILogger<TestClass>> _mockLogger;
    private Mock<ICorrelationIdProvider> _mockCorrelationProvider;
    private Mock<IUserContextProvider> _mockUserProvider;
    private ApplicationLogger<TestClass> _applicationLogger;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<TestClass>>();
        _mockCorrelationProvider = new Mock<ICorrelationIdProvider>();
        _mockUserProvider = new Mock<IUserContextProvider>();
        
        _applicationLogger = new ApplicationLogger<TestClass>(
            _mockLogger.Object,
            _mockCorrelationProvider.Object,
            _mockUserProvider.Object);
    }

    [Test]
    public void LogConfigurationCreated_ShouldLogCorrectMessage()
    {
        var configId = Guid.NewGuid();
        var configType = "Database";
        var userId = "testuser";

        _applicationLogger.LogConfigurationCreated(configId, configType, userId);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(configType) && 
                                           v.ToString().Contains(configId.ToString()) && 
                                           v.ToString().Contains(userId)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public void LogSecurityEvent_ShouldLogWithCorrectLevel()
    {
        var eventType = "Login";
        var userId = "testuser";
        var details = "Successful login";

        _applicationLogger.LogSecurityEvent(eventType, userId, details, true);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public void LogPerformanceMetric_ShouldIncludeDuration()
    {
        var operation = "DatabaseQuery";
        var duration = TimeSpan.FromMilliseconds(150);

        _applicationLogger.LogPerformanceMetric(operation, duration);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("150")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
```

### Integration Tests

#### **Test 3: End-to-End Logging Test**
```csharp
[TestFixture]
public class LoggingIntegrationTests
{
    private IServiceProvider _serviceProvider;
    private string _testLogPath;

    [SetUp]
    public void Setup()
    {
        _testLogPath = Path.Combine(Path.GetTempPath(), "test-logs");
        Directory.CreateDirectory(_testLogPath);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Serilog:WriteTo:0:Name", "File"},
                {"Serilog:WriteTo:0:Args:path", Path.Combine(_testLogPath, "test-.log")},
                {"Serilog:MinimumLevel:Default", "Debug"}
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLoggingServices(configuration);
        _serviceProvider = services.BuildServiceProvider();
    }

    [TearDown]
    public void Cleanup()
    {
        _serviceProvider?.Dispose();
        if (Directory.Exists(_testLogPath))
        {
            Directory.Delete(_testLogPath, true);
        }
    }

    [Test]
    public void LoggingService_ShouldWriteToFile()
    {
        var logger = _serviceProvider.GetRequiredService<IApplicationLogger<LoggingIntegrationTests>>();
        var testMessage = "Test configuration created";
        var configId = Guid.NewGuid();

        logger.LogConfigurationCreated(configId, "Test", "testuser");

        // Allow time for file write
        Thread.Sleep(100);

        var logFiles = Directory.GetFiles(_testLogPath, "*.log");
        Assert.That(logFiles.Length, Is.GreaterThan(0));

        var logContent = File.ReadAllText(logFiles[0]);
        Assert.That(logContent, Does.Contain(configId.ToString()));
        Assert.That(logContent, Does.Contain("testuser"));
    }

    [Test]
    public void StructuredLogger_ShouldMaintainStructure()
    {
        var logger = _serviceProvider.GetRequiredService<IStructuredLogger>();
        var properties = new Dictionary<string, object>
        {
            ["TestProperty"] = "TestValue",
            ["TestNumber"] = 42
        };

        logger.LogWithProperties(LogEventLevel.Information, 
            "Test message with {Property}", properties, "TestValue");

        Thread.Sleep(100);

        var logFiles = Directory.GetFiles(_testLogPath, "*.log");
        var logContent = File.ReadAllText(logFiles[0]);
        
        Assert.That(logContent, Does.Contain("TestValue"));
        Assert.That(logContent, Does.Contain("Test message"));
    }
}
```

### Performance Tests

#### **Test 4: Logging Performance Test**
```csharp
[TestFixture]
public class LoggingPerformanceTests
{
    private IServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Serilog:WriteTo:0:Name", "File"},
                {"Serilog:WriteTo:0:Args:path", "logs/perf-test-.log"}
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLoggingServices(configuration);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Test]
    public void LoggingPerformance_ShouldMeetRequirements()
    {
        var logger = _serviceProvider.GetRequiredService<IApplicationLogger<LoggingPerformanceTests>>();
        var iterations = 1000;
        
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            logger.LogConfigurationCreated(Guid.NewGuid(), "Performance", "perfuser");
        }
        
        stopwatch.Stop();
        
        // Should log 1000 entries in less than 100ms (0.1ms per log entry)
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(100));
        
        // Memory usage should not increase significantly
        GC.Collect();
        var memoryAfter = GC.GetTotalMemory(true);
        Assert.That(memoryAfter, Is.LessThan(50_000_000)); // Less than 50MB
    }
}
```

## 📊 Acceptance Criteria

### Primary Acceptance Criteria
- [ ] Serilog configured with multiple output targets (File, Console, EventLog)
- [ ] Structured logging with JSON formatting implemented
- [ ] Application logger service provides business-specific logging methods
- [ ] Log rotation and retention policies configured (30-day retention)
- [ ] Correlation IDs tracked across operations
- [ ] Security audit logging implemented
- [ ] Performance logging with timing metrics
- [ ] Custom enrichers for user context and performance data

### Quality Acceptance Criteria
- [ ] Logging overhead <5% of operation time
- [ ] Log files properly rotated with size limits
- [ ] Sensitive data filtered from logs
- [ ] All log levels correctly configured and filtered
- [ ] Memory usage remains stable under load

### Technical Acceptance Criteria
- [ ] Integration with dependency injection container
- [ ] Proper disposal of logging resources
- [ ] Configuration-driven log level management
- [ ] Thread-safe logging operations
- [ ] Exception handling in logging operations

## 🚨 Risk Management

### Technical Risks
| **Risk** | **Probability** | **Impact** | **Mitigation** |
|----------|----------------|------------|----------------|
| Log file corruption | Low | Medium | File rotation, backup logging targets |
| Performance degradation | Medium | Medium | Asynchronous logging, performance monitoring |
| Disk space exhaustion | Medium | High | Log rotation, size limits, monitoring |
| Memory leaks | Low | High | Proper disposal, memory monitoring |

### Operational Risks
| **Risk** | **Probability** | **Impact** | **Mitigation** |
|----------|----------------|------------|----------------|
| Log analysis complexity | High | Low | Structured logging, standardized format |
| Sensitive data exposure | Medium | High | Data filtering, access controls |
| Compliance violations | Low | High | Audit logging, retention policies |

## 📚 Resources & References

### Technical Documentation
- [Serilog Documentation](https://serilog.net/)
- [Microsoft Logging Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)
- [Structured Logging Best Practices](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging-providers)

### Security References
- OWASP Logging Guidelines
- Security audit trail best practices
- Data privacy in logging

## 📈 Success Metrics

### Completion Metrics
- [ ] All logging sinks configured and operational
- [ ] Business logging methods implemented
- [ ] Performance logging capturing metrics
- [ ] Security audit trail functional

### Quality Metrics
- [ ] Zero logging-related performance issues
- [ ] All sensitive data properly filtered
- [ ] Log rotation working correctly
- [ ] Memory usage stable

### Operational Metrics
- [ ] Log analysis tools can parse structured logs
- [ ] Monitoring alerts configured for log errors
- [ ] Retention policies enforced automatically

## 📝 Definition of Done

### Code Complete
- [ ] Serilog configuration implemented with all required sinks
- [ ] Application logger service with business-specific methods
- [ ] Custom enrichers for context and performance data
- [ ] Sensitive data filtering implemented
- [ ] Integration with dependency injection

### Quality Complete
- [ ] All unit tests passing with >95% coverage
- [ ] Integration tests validating log output
- [ ] Performance tests meeting requirements
- [ ] Security review of logging implementation

### Operational Complete
- [ ] Log rotation and retention working
- [ ] Monitoring configured for log health
- [ ] Documentation for log analysis
- [ ] Troubleshooting guide created

---

**Note**: This logging framework will serve as the foundation for monitoring, debugging, and auditing throughout the application lifecycle. Ensure proper configuration for both development and production environments.