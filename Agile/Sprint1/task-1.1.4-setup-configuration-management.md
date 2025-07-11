# Task 1.1.4: Setup Configuration Management

## 📋 Task Overview
**Sprint**: 1  
**Story**: 1.1 - Project Structure Setup  
**Priority**: High  
**Estimated Hours**: 4  
**Assigned To**: Senior Developer  
**Dependencies**: Task 1.1.1 - Create Solution Structure, Task 1.1.2 - Setup Dependency Injection

## 🎯 Objective
Thiết lập comprehensive configuration management system để quản lý application settings, environment-specific configurations, và secure configuration storage cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Configuration Sources**:
  - appsettings.json cho default settings
  - appsettings.{Environment}.json cho environment-specific settings
  - Windows Registry cho secure settings
  - Environment variables cho deployment settings
  - User-specific configuration files

- [ ] **Configuration Categories**:
  - Database connection settings
  - Security và encryption settings
  - Logging configuration
  - UI settings và themes
  - Performance settings
  - Feature flags và toggles

- [ ] **Configuration Security**:
  - Sensitive data encryption
  - Secure storage cho connection strings
  - Access control cho configuration changes
  - Audit trail cho configuration modifications

### Technical Requirements
- [ ] **Configuration Structure**:
  ```json
  // appsettings.json
  {
    "AppSettings": {
      "ApplicationName": "SystemConfig",
      "Version": "1.0.0",
      "Environment": "Development"
    },
    "Database": {
      "DefaultConnectionString": "",
      "ConnectionTimeout": 30,
      "CommandTimeout": 60,
      "MaxPoolSize": 100,
      "MinPoolSize": 5
    },
    "Security": {
      "EncryptionKey": "",
      "EnableAuditLogging": true,
      "SessionTimeout": 30,
      "MaxLoginAttempts": 3
    },
    "Logging": {
      "LogLevel": "Information",
      "LogFilePath": "logs/systemconfig.log",
      "EnableConsoleLogging": true,
      "EnableFileLogging": true,
      "EnableEventLogging": false
    },
    "UI": {
      "Theme": "Light",
      "Language": "en-US",
      "AutoSaveInterval": 30,
      "ShowTooltips": true
    },
    "Performance": {
      "CacheTimeout": 300,
      "MaxConcurrentOperations": 10,
      "EnableAsyncOperations": true
    },
    "Features": {
      "EnableBackupRestore": true,
      "EnableHealthMonitoring": true,
      "EnableAutoDiscovery": true,
      "EnableTemplateManagement": true
    }
  }
  ```

- [ ] **Configuration Service Interface**:
  ```csharp
  // SystemConfig.Infrastructure/Services/IConfigurationService.cs
  public interface IConfigurationService
  {
      T GetValue<T>(string key, T defaultValue = default);
      string GetConnectionString(string name);
      void SetValue<T>(string key, T value);
      void SaveConfiguration();
      void ReloadConfiguration();
      bool HasValue(string key);
      IEnumerable<string> GetKeys();
      void ValidateConfiguration();
      void BackupConfiguration();
      void RestoreConfiguration(string backupPath);
  }
  ```

### Quality Requirements
- [ ] **Type Safety**: Strongly-typed configuration access
- [ ] **Validation**: Configuration validation rules
- [ ] **Performance**: Fast configuration access
- [ ] **Security**: Secure storage cho sensitive data
- [ ] **Flexibility**: Easy to extend và modify

## 🏗️ Implementation Plan

### Phase 1: Configuration Structure Setup (2 hours)
```csharp
// SystemConfig.Infrastructure/Configuration/AppSettings.cs
public class AppSettings
{
    public ApplicationSettings Application { get; set; } = new();
    public DatabaseSettings Database { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
    public UISettings UI { get; set; } = new();
    public PerformanceSettings Performance { get; set; } = new();
    public FeatureSettings Features { get; set; } = new();
}

public class ApplicationSettings
{
    public string ApplicationName { get; set; } = "SystemConfig";
    public string Version { get; set; } = "1.0.0";
    public string Environment { get; set; } = "Development";
}

public class DatabaseSettings
{
    public string DefaultConnectionString { get; set; } = string.Empty;
    public int ConnectionTimeout { get; set; } = 30;
    public int CommandTimeout { get; set; } = 60;
    public int MaxPoolSize { get; set; } = 100;
    public int MinPoolSize { get; set; } = 5;
}

public class SecuritySettings
{
    public string EncryptionKey { get; set; } = string.Empty;
    public bool EnableAuditLogging { get; set; } = true;
    public int SessionTimeout { get; set; } = 30;
    public int MaxLoginAttempts { get; set; } = 3;
}

public class LoggingSettings
{
    public string LogLevel { get; set; } = "Information";
    public string LogFilePath { get; set; } = "logs/systemconfig.log";
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnableFileLogging { get; set; } = true;
    public bool EnableEventLogging { get; set; } = false;
}

public class UISettings
{
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "en-US";
    public int AutoSaveInterval { get; set; } = 30;
    public bool ShowTooltips { get; set; } = true;
}

public class PerformanceSettings
{
    public int CacheTimeout { get; set; } = 300;
    public int MaxConcurrentOperations { get; set; } = 10;
    public bool EnableAsyncOperations { get; set; } = true;
}

public class FeatureSettings
{
    public bool EnableBackupRestore { get; set; } = true;
    public bool EnableHealthMonitoring { get; set; } = true;
    public bool EnableAutoDiscovery { get; set; } = true;
    public bool EnableTemplateManagement { get; set; } = true;
}
```

### Phase 2: Configuration Service Implementation (1 hour)
```csharp
// SystemConfig.Infrastructure/Services/ConfigurationService.cs
public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly IEncryptionService _encryptionService;
    private readonly ILoggingService _loggingService;
    private readonly IRegistryService _registryService;
    
    public ConfigurationService(
        IConfiguration configuration,
        IEncryptionService encryptionService,
        ILoggingService loggingService,
        IRegistryService registryService)
    {
        _configuration = configuration;
        _encryptionService = encryptionService;
        _loggingService = loggingService;
        _registryService = registryService;
    }
    
    public T GetValue<T>(string key, T defaultValue = default)
    {
        try
        {
            var value = _configuration.GetValue<T>(key);
            return value ?? defaultValue;
        }
        catch (Exception ex)
        {
            _loggingService.LogWarning("Failed to get configuration value for key: {Key}, using default", key);
            return defaultValue;
        }
    }
    
    public string GetConnectionString(string name)
    {
        var connectionString = _configuration.GetConnectionString(name);
        if (string.IsNullOrEmpty(connectionString))
        {
            _loggingService.LogWarning("Connection string not found: {Name}", name);
            return string.Empty;
        }
        
        // Decrypt if needed
        if (IsEncrypted(connectionString))
        {
            return _encryptionService.Decrypt(connectionString);
        }
        
        return connectionString;
    }
    
    public void SetValue<T>(string key, T value)
    {
        try
        {
            // Store in registry for persistence
            var stringValue = value?.ToString() ?? string.Empty;
            _registryService.SetValue(key, stringValue);
            
            _loggingService.LogInformation("Configuration value set: {Key} = {Value}", key, value);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to set configuration value: {Key}", key);
            throw;
        }
    }
    
    public void SaveConfiguration()
    {
        try
        {
            // Save current configuration to registry
            var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
            if (appSettings != null)
            {
                _registryService.SaveConfiguration(appSettings);
                _loggingService.LogInformation("Configuration saved successfully");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to save configuration");
            throw;
        }
    }
    
    public void ReloadConfiguration()
    {
        try
        {
            // Reload configuration from sources
            _loggingService.LogInformation("Reloading configuration");
            // Implementation depends on configuration provider
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to reload configuration");
            throw;
        }
    }
    
    public bool HasValue(string key)
    {
        return _configuration.GetSection(key).Exists();
    }
    
    public IEnumerable<string> GetKeys()
    {
        return _configuration.GetChildren().Select(x => x.Key);
    }
    
    public void ValidateConfiguration()
    {
        try
        {
            var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
            if (appSettings == null)
            {
                throw new InvalidOperationException("AppSettings configuration is missing");
            }
            
            // Validate required settings
            if (string.IsNullOrEmpty(appSettings.Application.ApplicationName))
            {
                throw new InvalidOperationException("Application name is required");
            }
            
            _loggingService.LogInformation("Configuration validation passed");
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Configuration validation failed");
            throw;
        }
    }
    
    public void BackupConfiguration()
    {
        try
        {
            var backupPath = $"backups/config_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
            
            var json = JsonSerializer.Serialize(appSettings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            File.WriteAllText(backupPath, json);
            _loggingService.LogInformation("Configuration backed up to: {Path}", backupPath);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to backup configuration");
            throw;
        }
    }
    
    public void RestoreConfiguration(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("Backup file not found", backupPath);
            }
            
            var json = File.ReadAllText(backupPath);
            var appSettings = JsonSerializer.Deserialize<AppSettings>(json);
            
            // Apply restored configuration
            _registryService.SaveConfiguration(appSettings);
            _loggingService.LogInformation("Configuration restored from: {Path}", backupPath);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to restore configuration from: {Path}", backupPath);
            throw;
        }
    }
    
    private bool IsEncrypted(string value)
    {
        // Simple check for encrypted values
        return value.StartsWith("ENC:") || value.StartsWith("AES:");
    }
}
```

### Phase 3: Configuration Extensions (1 hour)
```csharp
// SystemConfig.Infrastructure/Configuration/ConfigurationExtensions.cs
public static class ConfigurationExtensions
{
    public static IServiceCollection AddConfigurationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration sections
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
        services.Configure<SecuritySettings>(configuration.GetSection("Security"));
        services.Configure<LoggingSettings>(configuration.GetSection("Logging"));
        services.Configure<UISettings>(configuration.GetSection("UI"));
        services.Configure<PerformanceSettings>(configuration.GetSection("Performance"));
        services.Configure<FeatureSettings>(configuration.GetSection("Features"));
        
        // Register configuration service
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        
        return services;
    }
    
    public static IConfigurationBuilder AddConfigurationSources(this IConfigurationBuilder builder, string environment)
    {
        return builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddRegistryConfiguration();
    }
}

// SystemConfig.Infrastructure/Configuration/RegistryConfigurationProvider.cs
public class RegistryConfigurationProvider : ConfigurationProvider
{
    private readonly string _registryPath;
    
    public RegistryConfigurationProvider(string registryPath)
    {
        _registryPath = registryPath;
    }
    
    public override void Load()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(_registryPath);
            if (key != null)
            {
                foreach (var valueName in key.GetValueNames())
                {
                    var value = key.GetValue(valueName);
                    if (value != null)
                    {
                        Data[$"Registry:{valueName}"] = value.ToString();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail
            Console.WriteLine($"Failed to load registry configuration: {ex.Message}");
        }
    }
}

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddRegistryConfiguration(this IConfigurationBuilder builder)
    {
        return builder.Add(new RegistryConfigurationSource());
    }
}

public class RegistryConfigurationSource : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new RegistryConfigurationProvider(@"SOFTWARE\SystemConfig");
    }
}
```

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Infrastructure/ConfigurationServiceTests.cs
public class ConfigurationServiceTests
{
    private readonly IConfigurationService _configurationService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IEncryptionService> _mockEncryptionService;
    private readonly Mock<ILoggingService> _mockLoggingService;
    private readonly Mock<IRegistryService> _mockRegistryService;
    
    public ConfigurationServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockEncryptionService = new Mock<IEncryptionService>();
        _mockLoggingService = new Mock<ILoggingService>();
        _mockRegistryService = new Mock<IRegistryService>();
        
        _configurationService = new ConfigurationService(
            _mockConfiguration.Object,
            _mockEncryptionService.Object,
            _mockLoggingService.Object,
            _mockRegistryService.Object);
    }
    
    [Fact]
    public void GetValue_WithValidKey_ShouldReturnValue()
    {
        // Arrange
        var key = "TestKey";
        var expectedValue = "TestValue";
        var section = new Mock<IConfigurationSection>();
        section.Setup(x => x.Value).Returns(expectedValue);
        _mockConfiguration.Setup(x => x.GetSection(key)).Returns(section.Object);
        
        // Act
        var result = _configurationService.GetValue<string>(key);
        
        // Assert
        Assert.Equal(expectedValue, result);
    }
    
    [Fact]
    public void GetValue_WithInvalidKey_ShouldReturnDefault()
    {
        // Arrange
        var key = "InvalidKey";
        var defaultValue = "DefaultValue";
        
        // Act
        var result = _configurationService.GetValue(key, defaultValue);
        
        // Assert
        Assert.Equal(defaultValue, result);
    }
    
    [Fact]
    public void SetValue_WithValidKey_ShouldSaveToRegistry()
    {
        // Arrange
        var key = "TestKey";
        var value = "TestValue";
        
        // Act
        _configurationService.SetValue(key, value);
        
        // Assert
        _mockRegistryService.Verify(x => x.SetValue(key, value), Times.Once);
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Configuration/ConfigurationIntegrationTests.cs
public class ConfigurationIntegrationTests : IDisposable
{
    private readonly string _testConfigPath;
    
    public ConfigurationIntegrationTests()
    {
        _testConfigPath = Path.Combine(Path.GetTempPath(), $"test-config-{Guid.NewGuid()}.json");
    }
    
    [Fact]
    public async Task Configuration_ShouldLoadFromMultipleSources()
    {
        // Arrange
        var testConfig = new AppSettings
        {
            Application = new ApplicationSettings { ApplicationName = "TestApp" },
            Database = new DatabaseSettings { ConnectionTimeout = 60 }
        };
        
        var json = JsonSerializer.Serialize(testConfig);
        await File.WriteAllTextAsync(_testConfigPath, json);
        
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(_testConfigPath)
            .Build();
        
        var services = new ServiceCollection();
        services.AddConfigurationServices(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var configService = serviceProvider.GetRequiredService<IConfigurationService>();
        
        // Act
        var appName = configService.GetValue<string>("Application:ApplicationName");
        var timeout = configService.GetValue<int>("Database:ConnectionTimeout");
        
        // Assert
        Assert.Equal("TestApp", appName);
        Assert.Equal(60, timeout);
    }
    
    public void Dispose()
    {
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }
    }
}
```

## 📊 Definition of Done
- [ ] **Configuration Structure**: AppSettings classes được define đầy đủ
- [ ] **Configuration Service**: IConfigurationService được implement
- [ ] **Multiple Sources**: JSON, Registry, Environment variables được support
- [ ] **Type Safety**: Strongly-typed configuration access
- [ ] **Security**: Sensitive data được encrypt
- [ ] **Validation**: Configuration validation rules được implement
- [ ] **Backup/Restore**: Configuration backup và restore functionality
- [ ] **Unit Tests**: >95% coverage cho configuration services
- [ ] **Integration Tests**: Configuration loading tests pass
- [ ] **Code Review**: Configuration setup được approve

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Configuration security vulnerabilities
- **Mitigation**: Implement proper encryption và access control

- **Risk**: Configuration file corruption
- **Mitigation**: Implement backup/restore functionality

- **Risk**: Performance issues với configuration access
- **Mitigation**: Use caching và efficient loading

### Operational Risks
- **Risk**: Configuration drift between environments
- **Mitigation**: Use environment-specific configuration files

- **Risk**: Sensitive data exposure
- **Mitigation**: Implement data masking và secure storage

## 📚 Resources & References
- .NET 8 Configuration Documentation
- Windows Registry Programming Guide
- Configuration Security Best Practices
- Environment-specific Configuration Patterns
- Configuration Validation Guidelines

## 🔄 Dependencies
- Task 1.1.1: Create Solution Structure
- Task 1.1.2: Setup Dependency Injection
- Task 1.1.3: Configure Logging Framework
- Microsoft.Extensions.Configuration packages

## 📈 Success Metrics
- Configuration loads successfully from all sources
- Type-safe configuration access works correctly
- Sensitive data được encrypt properly
- Configuration validation passes
- Backup/restore functionality works
- Performance benchmarks met

## 📝 Notes
- Use strongly-typed configuration classes
- Implement proper encryption cho sensitive data
- Consider configuration hot-reload cho development
- Document configuration schema cho team
- Regular configuration validation checks 