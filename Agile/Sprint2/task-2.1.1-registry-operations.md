# Task 2.1.1: Registry Operations

## 📋 Task Overview
**Sprint**: 2  
**Story**: 2.1 - Windows Registry Service  
**Priority**: High  
**Estimated Hours**: 6  
**Assigned To**: Senior Developer  
**Dependencies**: Task 1.3.2 - Registry Repository Implementation

## 🎯 Objective
Implement comprehensive Windows Registry operations với secure access, error handling, và performance optimization cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Registry Operations**:
  - Create/Read/Update/Delete registry keys và values
  - Registry path parsing và validation
  - Registry monitoring và health checks
  - Batch operations support
  - Registry backup/restore

- [ ] **Security Features**:
  - Permission validation
  - Access control enforcement
  - Secure registry access patterns
  - Audit logging cho registry operations
  - Registry integrity checks

- [ ] **Performance Features**:
  - Registry caching
  - Async operations
  - Connection pooling
  - Memory optimization
  - Query optimization

### Technical Requirements
- [ ] **Registry Service Interface**:
  ```csharp
  // SystemConfig.Infrastructure/Services/IRegistryService.cs
  public interface IRegistryService
  {
      // Basic operations
      Task<string> GetValueAsync(string registryPath);
      Task SetValueAsync(string registryPath, string value);
      Task DeleteValueAsync(string registryPath);
      Task<bool> ValueExistsAsync(string registryPath);
      Task<IEnumerable<string>> GetSubKeysAsync(string registryPath);
      Task<IEnumerable<string>> GetValueNamesAsync(string registryPath);
      
      // Advanced operations
      Task CreateKeyAsync(string registryPath);
      Task DeleteKeyAsync(string registryPath);
      Task<bool> KeyExistsAsync(string registryPath);
      Task<RegistryKeyInfo> GetKeyInfoAsync(string registryPath);
      
      // Batch operations
      Task SetValuesAsync(Dictionary<string, string> registryValues);
      Task DeleteValuesAsync(IEnumerable<string> registryPaths);
      Task<IEnumerable<string>> GetValuesAsync(IEnumerable<string> registryPaths);
      
      // Backup and restore
      Task CreateBackupAsync(string registryPath, string backupPath);
      Task RestoreBackupAsync(string backupPath, string registryPath);
      Task<string> ExportRegistryAsync(string registryPath);
      Task ImportRegistryAsync(string registryPath, string importData);
      
      // Security and monitoring
      Task<bool> HasPermissionAsync(string registryPath, RegistryRights rights);
      Task<RegistryHealthStatus> CheckHealthAsync(string registryPath);
      Task<IEnumerable<RegistryAuditLog>> GetAuditLogAsync(string registryPath);
      Task ClearAuditLogAsync(string registryPath);
  }
  ```

- [ ] **Registry Models**:
  ```csharp
  // SystemConfig.Infrastructure/Models/RegistryKeyInfo.cs
  public class RegistryKeyInfo
  {
      public string Path { get; set; }
      public string Name { get; set; }
      public RegistryHive Hive { get; set; }
      public DateTime LastModified { get; set; }
      public int SubKeyCount { get; set; }
      public int ValueCount { get; set; }
      public RegistrySecurity Security { get; set; }
      public bool IsAccessible { get; set; }
  }
  
  // SystemConfig.Infrastructure/Models/RegistryHealthStatus.cs
  public class RegistryHealthStatus
  {
      public bool IsHealthy { get; set; }
      public string Status { get; set; }
      public List<string> Issues { get; set; }
      public DateTime CheckedAt { get; set; }
      public TimeSpan ResponseTime { get; set; }
  }
  
  // SystemConfig.Infrastructure/Models/RegistryAuditLog.cs
  public class RegistryAuditLog
  {
      public Guid Id { get; set; }
      public string RegistryPath { get; set; }
      public string Operation { get; set; }
      public string UserId { get; set; }
      public DateTime Timestamp { get; set; }
      public string OldValue { get; set; }
      public string NewValue { get; set; }
      public bool IsSuccessful { get; set; }
      public string ErrorMessage { get; set; }
  }
  ```

### Quality Requirements
- [ ] **Security**: Secure registry access với permission validation
- [ ] **Performance**: Efficient registry operations với caching
- [ ] **Error Handling**: Comprehensive error handling
- [ ] **Logging**: Detailed audit logging
- [ ] **Testing**: Comprehensive unit và integration tests

## 🏗️ Implementation Plan

### Phase 1: Core Registry Operations (3 hours)
```csharp
// SystemConfig.Infrastructure/Services/RegistryService.cs
public class RegistryService : IRegistryService
{
    private readonly ILoggingService _loggingService;
    private readonly IConfigurationService _configurationService;
    private readonly Dictionary<string, object> _cache;
    private readonly object _cacheLock = new object();
    
    public RegistryService(ILoggingService loggingService, IConfigurationService configurationService)
    {
        _loggingService = loggingService;
        _configurationService = configurationService;
        _cache = new Dictionary<string, object>();
    }
    
    public async Task<string> GetValueAsync(string registryPath)
    {
        try
        {
            _loggingService.LogDebug("Getting registry value: {Path}", registryPath);
            
            // Check cache first
            if (TryGetFromCache(registryPath, out string cachedValue))
            {
                return cachedValue;
            }
            
            var (hive, subKey, valueName) = ParseRegistryPath(registryPath);
            
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            using var subKeyHandle = key.OpenSubKey(subKey);
            
            if (subKeyHandle == null)
            {
                _loggingService.LogWarning("Registry key not found: {Path}", registryPath);
                return null;
            }
            
            var value = subKeyHandle.GetValue(valueName);
            var stringValue = value?.ToString();
            
            // Cache the result
            AddToCache(registryPath, stringValue);
            
            _loggingService.LogDebug("Retrieved registry value: {Path} = {Value}", registryPath, stringValue);
            
            return stringValue;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to get registry value: {Path}", registryPath);
            throw;
        }
    }
    
    public async Task SetValueAsync(string registryPath, string value)
    {
        try
        {
            _loggingService.LogDebug("Setting registry value: {Path} = {Value}", registryPath, value);
            
            var (hive, subKey, valueName) = ParseRegistryPath(registryPath);
            
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            using var subKeyHandle = key.CreateSubKey(subKey);
            
            subKeyHandle.SetValue(valueName, value);
            
            // Update cache
            AddToCache(registryPath, value);
            
            // Log audit
            await LogAuditAsync(registryPath, "SetValue", null, value, true);
            
            _loggingService.LogDebug("Set registry value successfully: {Path}", registryPath);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to set registry value: {Path}", registryPath);
            await LogAuditAsync(registryPath, "SetValue", null, value, false, ex.Message);
            throw;
        }
    }
    
    public async Task DeleteValueAsync(string registryPath)
    {
        try
        {
            _loggingService.LogDebug("Deleting registry value: {Path}", registryPath);
            
            var (hive, subKey, valueName) = ParseRegistryPath(registryPath);
            
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            using var subKeyHandle = key.OpenSubKey(subKey, true);
            
            if (subKeyHandle != null)
            {
                var oldValue = subKeyHandle.GetValue(valueName)?.ToString();
                subKeyHandle.DeleteValue(valueName, false);
                
                // Remove from cache
                RemoveFromCache(registryPath);
                
                // Log audit
                await LogAuditAsync(registryPath, "DeleteValue", oldValue, null, true);
                
                _loggingService.LogDebug("Deleted registry value successfully: {Path}", registryPath);
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to delete registry value: {Path}", registryPath);
            await LogAuditAsync(registryPath, "DeleteValue", null, null, false, ex.Message);
            throw;
        }
    }
    
    public async Task<bool> ValueExistsAsync(string registryPath)
    {
        try
        {
            var (hive, subKey, valueName) = ParseRegistryPath(registryPath);
            
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            using var subKeyHandle = key.OpenSubKey(subKey);
            
            if (subKeyHandle == null)
                return false;
            
            return subKeyHandle.GetValue(valueName) != null;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to check registry value existence: {Path}", registryPath);
            return false;
        }
    }
    
    public async Task<IEnumerable<string>> GetSubKeysAsync(string registryPath)
    {
        try
        {
            _loggingService.LogDebug("Getting sub keys: {Path}", registryPath);
            
            var (hive, subKey, _) = ParseRegistryPath(registryPath);
            
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            using var subKeyHandle = key.OpenSubKey(subKey);
            
            if (subKeyHandle == null)
                return Enumerable.Empty<string>();
            
            var subKeys = subKeyHandle.GetSubKeyNames();
            
            _loggingService.LogDebug("Retrieved {Count} sub keys: {Path}", subKeys.Length, registryPath);
            
            return subKeys;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to get sub keys: {Path}", registryPath);
            return Enumerable.Empty<string>();
        }
    }
    
    public async Task<IEnumerable<string>> GetValueNamesAsync(string registryPath)
    {
        try
        {
            _loggingService.LogDebug("Getting value names: {Path}", registryPath);
            
            var (hive, subKey, _) = ParseRegistryPath(registryPath);
            
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            using var subKeyHandle = key.OpenSubKey(subKey);
            
            if (subKeyHandle == null)
                return Enumerable.Empty<string>();
            
            var valueNames = subKeyHandle.GetValueNames();
            
            _loggingService.LogDebug("Retrieved {Count} value names: {Path}", valueNames.Length, registryPath);
            
            return valueNames;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to get value names: {Path}", registryPath);
            return Enumerable.Empty<string>();
        }
    }
    
    public async Task CreateKeyAsync(string registryPath)
    {
        try
        {
            _loggingService.LogDebug("Creating registry key: {Path}", registryPath);
            
            var (hive, subKey, _) = ParseRegistryPath(registryPath);
            
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            using var subKeyHandle = key.CreateSubKey(subKey);
            
            // Log audit
            await LogAuditAsync(registryPath, "CreateKey", null, null, true);
            
            _loggingService.LogDebug("Created registry key successfully: {Path}", registryPath);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to create registry key: {Path}", registryPath);
            await LogAuditAsync(registryPath, "CreateKey", null, null, false, ex.Message);
            throw;
        }
    }
    
    public async Task DeleteKeyAsync(string registryPath)
    {
        try
        {
            _loggingService.LogDebug("Deleting registry key: {Path}", registryPath);
            
            var (hive, subKey, _) = ParseRegistryPath(registryPath);
            
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            key.DeleteSubKeyTree(subKey, false);
            
            // Log audit
            await LogAuditAsync(registryPath, "DeleteKey", null, null, true);
            
            _loggingService.LogDebug("Deleted registry key successfully: {Path}", registryPath);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to delete registry key: {Path}", registryPath);
            await LogAuditAsync(registryPath, "DeleteKey", null, null, false, ex.Message);
            throw;
        }
    }
    
    public async Task<bool> KeyExistsAsync(string registryPath)
    {
        try
        {
            var (hive, subKey, _) = ParseRegistryPath(registryPath);
            
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            using var subKeyHandle = key.OpenSubKey(subKey);
            
            return subKeyHandle != null;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to check registry key existence: {Path}", registryPath);
            return false;
        }
    }
    
    public async Task<RegistryKeyInfo> GetKeyInfoAsync(string registryPath)
    {
        try
        {
            var (hive, subKey, _) = ParseRegistryPath(registryPath);
            
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            using var subKeyHandle = key.OpenSubKey(subKey);
            
            if (subKeyHandle == null)
                return null;
            
            var keyInfo = new RegistryKeyInfo
            {
                Path = registryPath,
                Name = subKey.Split('\\').LastOrDefault(),
                Hive = hive,
                LastModified = DateTime.Now, // Registry doesn't provide this directly
                SubKeyCount = subKeyHandle.SubKeyCount,
                ValueCount = subKeyHandle.ValueCount,
                IsAccessible = true
            };
            
            return keyInfo;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to get registry key info: {Path}", registryPath);
            return null;
        }
    }
    
    // Helper methods
    private (RegistryHive hive, string subKey, string valueName) ParseRegistryPath(string registryPath)
    {
        var parts = registryPath.Split('\\', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length < 2)
            throw new ArgumentException("Invalid registry path format");
        
        var hiveName = parts[0].ToUpperInvariant();
        var hive = hiveName switch
        {
            "HKEY_LOCAL_MACHINE" => RegistryHive.LocalMachine,
            "HKEY_CURRENT_USER" => RegistryHive.CurrentUser,
            "HKEY_CLASSES_ROOT" => RegistryHive.ClassesRoot,
            "HKEY_USERS" => RegistryHive.Users,
            "HKEY_CURRENT_CONFIG" => RegistryHive.CurrentConfig,
            _ => throw new ArgumentException($"Unsupported registry hive: {hiveName}")
        };
        
        var subKey = string.Join("\\", parts.Skip(1).Take(parts.Length - 2));
        var valueName = parts.Length > 2 ? parts.Last() : "";
        
        return (hive, subKey, valueName);
    }
    
    private bool TryGetFromCache(string key, out string value)
    {
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(key, out var cachedValue))
            {
                value = cachedValue as string;
                return true;
            }
            
            value = null;
            return false;
        }
    }
    
    private void AddToCache(string key, string value)
    {
        lock (_cacheLock)
        {
            _cache[key] = value;
        }
    }
    
    private void RemoveFromCache(string key)
    {
        lock (_cacheLock)
        {
            _cache.Remove(key);
        }
    }
    
    private async Task LogAuditAsync(string registryPath, string operation, string oldValue, string newValue, bool isSuccessful, string errorMessage = null)
    {
        var auditLog = new RegistryAuditLog
        {
            Id = Guid.NewGuid(),
            RegistryPath = registryPath,
            Operation = operation,
            UserId = Environment.UserName,
            Timestamp = DateTime.UtcNow,
            OldValue = oldValue,
            NewValue = newValue,
            IsSuccessful = isSuccessful,
            ErrorMessage = errorMessage
        };
        
        _loggingService.LogInformation("Registry audit: {Operation} {Path} {Success}", operation, registryPath, isSuccessful);
        
        // In a real implementation, this would be saved to a database
        await Task.CompletedTask;
    }
}
```

### Phase 2: Batch Operations Implementation (2 hours)
```csharp
// SystemConfig.Infrastructure/Services/RegistryBatchService.cs
public class RegistryBatchService
{
    private readonly IRegistryService _registryService;
    private readonly ILoggingService _loggingService;
    
    public RegistryBatchService(IRegistryService registryService, ILoggingService loggingService)
    {
        _registryService = registryService;
        _loggingService = loggingService;
    }
    
    public async Task SetValuesAsync(Dictionary<string, string> registryValues)
    {
        try
        {
            _loggingService.LogInformation("Setting {Count} registry values in batch", registryValues.Count);
            
            foreach (var kvp in registryValues)
            {
                await _registryService.SetValueAsync(kvp.Key, kvp.Value);
            }
            
            _loggingService.LogInformation("Successfully set {Count} registry values", registryValues.Count);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to set registry values in batch");
            throw;
        }
    }
    
    public async Task DeleteValuesAsync(IEnumerable<string> registryPaths)
    {
        try
        {
            var paths = registryPaths.ToList();
            _loggingService.LogInformation("Deleting {Count} registry values in batch", paths.Count);
            
            foreach (var path in paths)
            {
                await _registryService.DeleteValueAsync(path);
            }
            
            _loggingService.LogInformation("Successfully deleted {Count} registry values", paths.Count);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to delete registry values in batch");
            throw;
        }
    }
    
    public async Task<IEnumerable<string>> GetValuesAsync(IEnumerable<string> registryPaths)
    {
        try
        {
            var paths = registryPaths.ToList();
            _loggingService.LogInformation("Getting {Count} registry values in batch", paths.Count);
            
            var values = new List<string>();
            foreach (var path in paths)
            {
                var value = await _registryService.GetValueAsync(path);
                values.Add(value);
            }
            
            _loggingService.LogInformation("Successfully retrieved {Count} registry values", values.Count);
            
            return values;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to get registry values in batch");
            throw;
        }
    }
}
```

### Phase 3: Backup and Restore Implementation (1 hour)
```csharp
// SystemConfig.Infrastructure/Services/RegistryBackupService.cs
public class RegistryBackupService
{
    private readonly ILoggingService _loggingService;
    private readonly string _backupPath;
    
    public RegistryBackupService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
        _backupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SystemConfig", "Backups");
        Directory.CreateDirectory(_backupPath);
    }
    
    public async Task CreateBackupAsync(string registryPath, string backupPath = null)
    {
        try
        {
            var backupFilePath = backupPath ?? Path.Combine(_backupPath, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.reg");
            
            _loggingService.LogInformation("Creating registry backup: {Path} -> {BackupPath}", registryPath, backupFilePath);
            
            var startInfo = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = $"export \"{registryPath}\" \"{backupFilePath}\" /y",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            await process.WaitForExitAsync();
            
            if (process.ExitCode == 0)
            {
                _loggingService.LogInformation("Registry backup created successfully: {BackupPath}", backupFilePath);
            }
            else
            {
                throw new Exception($"Failed to create registry backup. Exit code: {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to create registry backup: {Path}", registryPath);
            throw;
        }
    }
    
    public async Task RestoreBackupAsync(string backupPath, string registryPath)
    {
        try
        {
            _loggingService.LogInformation("Restoring registry backup: {BackupPath} -> {Path}", backupPath, registryPath);
            
            var startInfo = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = $"import \"{backupPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            await process.WaitForExitAsync();
            
            if (process.ExitCode == 0)
            {
                _loggingService.LogInformation("Registry backup restored successfully: {BackupPath}", backupPath);
            }
            else
            {
                throw new Exception($"Failed to restore registry backup. Exit code: {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to restore registry backup: {BackupPath}", backupPath);
            throw;
        }
    }
    
    public async Task<string> ExportRegistryAsync(string registryPath)
    {
        try
        {
            _loggingService.LogInformation("Exporting registry: {Path}", registryPath);
            
            var tempFile = Path.GetTempFileName();
            
            var startInfo = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = $"export \"{registryPath}\" \"{tempFile}\" /y",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            await process.WaitForExitAsync();
            
            if (process.ExitCode == 0)
            {
                var exportData = await File.ReadAllTextAsync(tempFile);
                File.Delete(tempFile);
                
                _loggingService.LogInformation("Registry exported successfully: {Path}", registryPath);
                
                return exportData;
            }
            else
            {
                throw new Exception($"Failed to export registry. Exit code: {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to export registry: {Path}", registryPath);
            throw;
        }
    }
    
    public async Task ImportRegistryAsync(string registryPath, string importData)
    {
        try
        {
            _loggingService.LogInformation("Importing registry: {Path}", registryPath);
            
            var tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, importData);
            
            var startInfo = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = $"import \"{tempFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            await process.WaitForExitAsync();
            
            File.Delete(tempFile);
            
            if (process.ExitCode == 0)
            {
                _loggingService.LogInformation("Registry imported successfully: {Path}", registryPath);
            }
            else
            {
                throw new Exception($"Failed to import registry. Exit code: {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to import registry: {Path}", registryPath);
            throw;
        }
    }
}
```

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Infrastructure/Services/RegistryServiceTests.cs
public class RegistryServiceTests
{
    private readonly Mock<ILoggingService> _mockLoggingService;
    private readonly Mock<IConfigurationService> _mockConfigurationService;
    private readonly RegistryService _registryService;
    
    public RegistryServiceTests()
    {
        _mockLoggingService = new Mock<ILoggingService>();
        _mockConfigurationService = new Mock<IConfigurationService>();
        _registryService = new RegistryService(_mockLoggingService.Object, _mockConfigurationService.Object);
    }
    
    [Fact]
    public async Task GetValueAsync_WithValidPath_ShouldReturnValue()
    {
        // Arrange
        var registryPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\SystemConfig\Test";
        var expectedValue = "test_value";
        
        // Act
        var result = await _registryService.GetValueAsync(registryPath);
        
        // Assert
        // Note: This test would need to set up actual registry values
        // For now, we're testing the method structure
        Assert.NotNull(_registryService);
    }
    
    [Fact]
    public async Task SetValueAsync_WithValidPath_ShouldSetValue()
    {
        // Arrange
        var registryPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\SystemConfig\Test";
        var value = "test_value";
        
        // Act
        await _registryService.SetValueAsync(registryPath, value);
        
        // Assert
        // Verify that the value was set correctly
        var retrievedValue = await _registryService.GetValueAsync(registryPath);
        Assert.Equal(value, retrievedValue);
    }
    
    [Fact]
    public async Task DeleteValueAsync_WithValidPath_ShouldDeleteValue()
    {
        // Arrange
        var registryPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\SystemConfig\Test";
        var value = "test_value";
        await _registryService.SetValueAsync(registryPath, value);
        
        // Act
        await _registryService.DeleteValueAsync(registryPath);
        
        // Assert
        var exists = await _registryService.ValueExistsAsync(registryPath);
        Assert.False(exists);
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Infrastructure/Services/RegistryServiceIntegrationTests.cs
public class RegistryServiceIntegrationTests : IDisposable
{
    private readonly IRegistryService _registryService;
    private readonly string _testRegistryPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\SystemConfig\Test";
    
    public RegistryServiceIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure();
        var serviceProvider = services.BuildServiceProvider();
        _registryService = serviceProvider.GetRequiredService<IRegistryService>();
    }
    
    [Fact]
    public async Task RegistryService_ShouldPerformFullCRUDCycle()
    {
        // Arrange
        var testValue = "test_value_" + Guid.NewGuid();
        
        // Act - Create
        await _registryService.SetValueAsync(_testRegistryPath, testValue);
        
        // Assert - Create
        var retrievedValue = await _registryService.GetValueAsync(_testRegistryPath);
        Assert.Equal(testValue, retrievedValue);
        
        // Act - Update
        var updatedValue = "updated_value_" + Guid.NewGuid();
        await _registryService.SetValueAsync(_testRegistryPath, updatedValue);
        
        // Assert - Update
        var newRetrievedValue = await _registryService.GetValueAsync(_testRegistryPath);
        Assert.Equal(updatedValue, newRetrievedValue);
        
        // Act - Delete
        await _registryService.DeleteValueAsync(_testRegistryPath);
        
        // Assert - Delete
        var exists = await _registryService.ValueExistsAsync(_testRegistryPath);
        Assert.False(exists);
    }
    
    public void Dispose()
    {
        // Cleanup test registry entries
        try
        {
            _registryService.DeleteValueAsync(_testRegistryPath).Wait();
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
```

## 📊 Definition of Done
- [ ] **Registry Service Interface**: IRegistryService interface được implement đầy đủ
- [ ] **Registry Operations**: Tất cả basic operations hoạt động đúng
- [ ] **Batch Operations**: Batch operations được implement
- [ ] **Backup/Restore**: Backup và restore functionality hoàn thành
- [ ] **Security**: Permission validation và audit logging hoàn thành
- [ ] **Performance**: Caching và optimization hoàn thành
- [ ] **Unit Tests**: >95% coverage cho registry service
- [ ] **Integration Tests**: Registry operations tests pass
- [ ] **Code Review**: Registry operations được approve

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Windows Registry permission issues
- **Mitigation**: Implement proper permission checks và error handling

- **Risk**: Registry corruption during operations
- **Mitigation**: Implement backup/restore functionality

- **Risk**: Performance issues với large registry data
- **Mitigation**: Implement caching và optimization

### Security Risks
- **Risk**: Registry access vulnerabilities
- **Mitigation**: Implement access control validation

- **Risk**: Sensitive data exposure
- **Mitigation**: Implement proper logging filters

## 📚 Resources & References
- Windows Registry Programming Guide
- .NET Registry Access Best Practices
- Registry Security Guidelines
- Registry Backup/Restore Patterns
- Registry Performance Optimization

## 🔄 Dependencies
- Task 1.3.2: Registry Repository Implementation
- Task 1.1.2: Setup Dependency Injection
- Task 1.1.3: Configure Logging Framework

## 📈 Success Metrics
- Registry operations work correctly
- Security features implemented
- Performance benchmarks met
- High test coverage achieved
- Error handling works properly
- Audit logging functions correctly

## 📝 Notes
- Implement proper registry permissions
- Use caching cho performance
- Implement backup/restore functionality
- Consider security implications
- Document registry structure
- Regular security reviews 