# Task 1.3.2: Registry Repository Implementation

## 📋 Task Overview
**Sprint**: 1  
**Story**: 1.3 - Basic Repository Pattern  
**Priority**: High  
**Estimated Hours**: 6  
**Assigned To**: Senior Developer  
**Dependencies**: Task 1.3.1 - Generic Repository Interface

## 🎯 Objective
Implement Windows Registry repository implementation với secure data access, encryption, và error handling cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Windows Registry Repository**:
  - Secure registry access với permission validation
  - Data encryption/decryption cho sensitive information
  - Error handling và logging
  - Transaction support
  - Backup/restore functionality

- [ ] **Registry Operations**:
  - Create/Read/Update/Delete registry keys
  - Query registry với filtering
  - Batch operations
  - Registry monitoring
  - Health checks

- [ ] **Security Features**:
  - Access control validation
  - Data encryption at rest
  - Audit logging
  - Secure key management
  - Permission checks

### Technical Requirements
- [ ] **Registry Repository Structure**:
  ```csharp
  // SystemConfig.Infrastructure/Repositories/RegistryRepository.cs
  public class RegistryRepository<T> : IRepository<T> where T : AggregateRoot
  {
      private readonly IRegistryService _registryService;
      private readonly IEncryptionService _encryptionService;
      private readonly ILoggingService _loggingService;
      private readonly string _baseRegistryPath;
      private readonly JsonSerializerOptions _jsonOptions;
      
      public RegistryRepository(
          IRegistryService registryService,
          IEncryptionService encryptionService,
          ILoggingService loggingService,
          string baseRegistryPath)
      {
          _registryService = registryService;
          _encryptionService = encryptionService;
          _loggingService = loggingService;
          _baseRegistryPath = baseRegistryPath;
          _jsonOptions = new JsonSerializerOptions
          {
              PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
              WriteIndented = true
          };
      }
      
      public async Task<T> GetByIdAsync(Guid id)
      {
          try
          {
              var registryPath = GetRegistryPath(id);
              var jsonData = await _registryService.GetValueAsync(registryPath);
              
              if (string.IsNullOrEmpty(jsonData))
                  return null;
              
              var decryptedData = _encryptionService.Decrypt(jsonData);
              return JsonSerializer.Deserialize<T>(decryptedData, _jsonOptions);
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to get entity by ID: {Id}", id);
              throw;
          }
      }
      
      public async Task<IEnumerable<T>> GetAllAsync()
      {
          try
          {
              var entities = new List<T>();
              var subKeys = await _registryService.GetSubKeysAsync(_baseRegistryPath);
              
              foreach (var subKey in subKeys)
              {
                  var id = Guid.Parse(subKey);
                  var entity = await GetByIdAsync(id);
                  if (entity != null)
                      entities.Add(entity);
              }
              
              return entities;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to get all entities");
              throw;
          }
      }
      
      public async Task<T> AddAsync(T entity)
      {
          try
          {
              var registryPath = GetRegistryPath(entity.Id);
              var jsonData = JsonSerializer.Serialize(entity, _jsonOptions);
              var encryptedData = _encryptionService.Encrypt(jsonData);
              
              await _registryService.SetValueAsync(registryPath, encryptedData);
              _loggingService.LogInformation("Entity added: {Id}", entity.Id);
              
              return entity;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to add entity: {Id}", entity.Id);
              throw;
          }
      }
      
      public async Task<T> UpdateAsync(T entity)
      {
          try
          {
              var registryPath = GetRegistryPath(entity.Id);
              var jsonData = JsonSerializer.Serialize(entity, _jsonOptions);
              var encryptedData = _encryptionService.Encrypt(jsonData);
              
              await _registryService.SetValueAsync(registryPath, encryptedData);
              _loggingService.LogInformation("Entity updated: {Id}", entity.Id);
              
              return entity;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to update entity: {Id}", entity.Id);
              throw;
          }
      }
      
      public async Task DeleteAsync(Guid id)
      {
          try
          {
              var registryPath = GetRegistryPath(id);
              await _registryService.DeleteValueAsync(registryPath);
              _loggingService.LogInformation("Entity deleted: {Id}", id);
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to delete entity: {Id}", id);
              throw;
          }
      }
      
      public async Task<bool> ExistsAsync(Guid id)
      {
          try
          {
              var registryPath = GetRegistryPath(id);
              var value = await _registryService.GetValueAsync(registryPath);
              return !string.IsNullOrEmpty(value);
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to check entity existence: {Id}", id);
              return false;
          }
      }
      
      public async Task<IEnumerable<T>> FindAsync(ISpecification<T> specification)
      {
          try
          {
              var allEntities = await GetAllAsync();
              var query = allEntities.AsQueryable();
              
              if (specification.Criteria != null)
              {
                  query = query.Where(specification.Criteria.Compile());
              }
              
              if (specification.OrderBy != null)
              {
                  query = query.OrderBy(specification.OrderBy.Compile());
              }
              else if (specification.OrderByDescending != null)
              {
                  query = query.OrderByDescending(specification.OrderByDescending.Compile());
              }
              
              if (specification.IsPagingEnabled)
              {
                  query = query.Skip(specification.Skip).Take(specification.Take);
              }
              
              return query.ToList();
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to find entities with specification");
              throw;
          }
      }
      
      public async Task<T> FindOneAsync(ISpecification<T> specification)
      {
          try
          {
              var results = await FindAsync(specification);
              return results.FirstOrDefault();
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to find one entity with specification");
              throw;
          }
      }
      
      public async Task<int> CountAsync(ISpecification<T> specification)
      {
          try
          {
              var allEntities = await GetAllAsync();
              var query = allEntities.AsQueryable();
              
              if (specification.Criteria != null)
              {
                  query = query.Where(specification.Criteria.Compile());
              }
              
              return query.Count();
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to count entities with specification");
              throw;
          }
      }
      
      public async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize)
      {
          try
          {
              var allEntities = await GetAllAsync();
              var totalCount = allEntities.Count();
              var items = allEntities.Skip((pageNumber - 1) * pageSize).Take(pageSize);
              
              return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to get paged entities");
              throw;
          }
      }
      
      public async Task<PagedResult<T>> GetPagedAsync(ISpecification<T> specification, int pageNumber, int pageSize)
      {
          try
          {
              var allEntities = await GetAllAsync();
              var query = allEntities.AsQueryable();
              
              if (specification.Criteria != null)
              {
                  query = query.Where(specification.Criteria.Compile());
              }
              
              var totalCount = query.Count();
              var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
              
              return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to get paged entities with specification");
              throw;
          }
      }
      
      public async Task AddRangeAsync(IEnumerable<T> entities)
      {
          try
          {
              foreach (var entity in entities)
              {
                  await AddAsync(entity);
              }
              _loggingService.LogInformation("Added {Count} entities", entities.Count());
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to add range of entities");
              throw;
          }
      }
      
      public async Task UpdateRangeAsync(IEnumerable<T> entities)
      {
          try
          {
              foreach (var entity in entities)
              {
                  await UpdateAsync(entity);
              }
              _loggingService.LogInformation("Updated {Count} entities", entities.Count());
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to update range of entities");
              throw;
          }
      }
      
      public async Task DeleteRangeAsync(IEnumerable<Guid> ids)
      {
          try
          {
              foreach (var id in ids)
              {
                  await DeleteAsync(id);
              }
              _loggingService.LogInformation("Deleted {Count} entities", ids.Count());
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to delete range of entities");
              throw;
          }
      }
      
      public async Task BeginTransactionAsync()
      {
          // Registry doesn't support transactions, but we can implement a simple backup mechanism
          try
          {
              await _registryService.CreateBackupAsync(_baseRegistryPath);
              _loggingService.LogInformation("Transaction backup created");
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to begin transaction");
              throw;
          }
      }
      
      public async Task CommitTransactionAsync()
      {
          try
          {
              // Commit is automatic for registry operations
              _loggingService.LogInformation("Transaction committed");
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to commit transaction");
              throw;
          }
      }
      
      public async Task RollbackTransactionAsync()
      {
          try
          {
              await _registryService.RestoreBackupAsync(_baseRegistryPath);
              _loggingService.LogInformation("Transaction rolled back");
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to rollback transaction");
              throw;
          }
      }
      
      private string GetRegistryPath(Guid id)
      {
          return $"{_baseRegistryPath}\\{id}";
      }
  }
  ```

- [ ] **Registry Service Interface**:
  ```csharp
  // SystemConfig.Infrastructure/Services/IRegistryService.cs
  public interface IRegistryService
  {
      Task<string> GetValueAsync(string registryPath);
      Task SetValueAsync(string registryPath, string value);
      Task DeleteValueAsync(string registryPath);
      Task<bool> ValueExistsAsync(string registryPath);
      Task<IEnumerable<string>> GetSubKeysAsync(string registryPath);
      Task CreateBackupAsync(string registryPath);
      Task RestoreBackupAsync(string registryPath);
      Task<bool> HasPermissionAsync(string registryPath);
  }
  ```

### Quality Requirements
- [ ] **Security**: Secure registry access với encryption
- [ ] **Performance**: Efficient registry operations
- [ ] **Error Handling**: Comprehensive error handling
- [ ] **Logging**: Detailed audit logging
- [ ] **Testing**: Comprehensive unit và integration tests

## 🏗️ Implementation Plan

### Phase 1: Registry Service Implementation (3 hours)
```csharp
// SystemConfig.Infrastructure/Services/RegistryService.cs
public class RegistryService : IRegistryService
{
    private readonly ILoggingService _loggingService;
    private readonly string _backupPath;
    
    public RegistryService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
        _backupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SystemConfig", "Backups");
        Directory.CreateDirectory(_backupPath);
    }
    
    public async Task<string> GetValueAsync(string registryPath)
    {
        try
        {
            var (hive, subKey, valueName) = ParseRegistryPath(registryPath);
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            using var subKeyHandle = key.OpenSubKey(subKey);
            
            if (subKeyHandle == null)
                return null;
            
            var value = subKeyHandle.GetValue(valueName);
            return value?.ToString();
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
            var (hive, subKey, valueName) = ParseRegistryPath(registryPath);
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            using var subKeyHandle = key.CreateSubKey(subKey);
            
            subKeyHandle.SetValue(valueName, value);
            _loggingService.LogInformation("Registry value set: {Path}", registryPath);
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to set registry value: {Path}", registryPath);
            throw;
        }
    }
    
    public async Task DeleteValueAsync(string registryPath)
    {
        try
        {
            var (hive, subKey, valueName) = ParseRegistryPath(registryPath);
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            using var subKeyHandle = key.OpenSubKey(subKey, true);
            
            if (subKeyHandle != null)
            {
                subKeyHandle.DeleteValue(valueName, false);
                _loggingService.LogInformation("Registry value deleted: {Path}", registryPath);
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to delete registry value: {Path}", registryPath);
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
            var (hive, subKey, _) = ParseRegistryPath(registryPath);
            using var key = Registry.OpenBaseKey(hive, RegistryView.Default);
            using var subKeyHandle = key.OpenSubKey(subKey);
            
            if (subKeyHandle == null)
                return Enumerable.Empty<string>();
            
            return subKeyHandle.GetSubKeyNames();
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to get registry sub keys: {Path}", registryPath);
            return Enumerable.Empty<string>();
        }
    }
    
    public async Task CreateBackupAsync(string registryPath)
    {
        try
        {
            var backupFileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.reg";
            var backupFilePath = Path.Combine(_backupPath, backupFileName);
            
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
                _loggingService.LogInformation("Registry backup created: {Path}", backupFilePath);
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
    
    public async Task RestoreBackupAsync(string registryPath)
    {
        try
        {
            var backupFiles = Directory.GetFiles(_backupPath, "backup_*.reg")
                .OrderByDescending(f => f)
                .FirstOrDefault();
            
            if (string.IsNullOrEmpty(backupFiles))
                throw new FileNotFoundException("No backup files found");
            
            var startInfo = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = $"import \"{backupFiles}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            await process.WaitForExitAsync();
            
            if (process.ExitCode == 0)
            {
                _loggingService.LogInformation("Registry backup restored: {Path}", backupFiles);
            }
            else
            {
                throw new Exception($"Failed to restore registry backup. Exit code: {process.ExitCode}");
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to restore registry backup: {Path}", registryPath);
            throw;
        }
    }
    
    public async Task<bool> HasPermissionAsync(string registryPath)
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
            _loggingService.LogError(ex, "Failed to check registry permissions: {Path}", registryPath);
            return false;
        }
    }
    
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
}
```

### Phase 2: Repository Factory Implementation (2 hours)
```csharp
// SystemConfig.Infrastructure/Repositories/RegistryRepositoryFactory.cs
public class RegistryRepositoryFactory : IRepositoryFactory
{
    private readonly IRegistryService _registryService;
    private readonly IEncryptionService _encryptionService;
    private readonly ILoggingService _loggingService;
    private readonly Dictionary<Type, string> _registryPaths;
    
    public RegistryRepositoryFactory(
        IRegistryService registryService,
        IEncryptionService encryptionService,
        ILoggingService loggingService)
    {
        _registryService = registryService;
        _encryptionService = encryptionService;
        _loggingService = loggingService;
        _registryPaths = new Dictionary<Type, string>
        {
            { typeof(DatabaseConfiguration), @"HKEY_LOCAL_MACHINE\SOFTWARE\SystemConfig\Database" },
            { typeof(PrinterConfiguration), @"HKEY_LOCAL_MACHINE\SOFTWARE\SystemConfig\Printer" },
            { typeof(SystemConfiguration), @"HKEY_LOCAL_MACHINE\SOFTWARE\SystemConfig\System" }
        };
    }
    
    public IRepository<T> CreateRepository<T>() where T : AggregateRoot
    {
        var entityType = typeof(T);
        
        if (!_registryPaths.ContainsKey(entityType))
        {
            throw new ArgumentException($"No registry path configured for entity type: {entityType.Name}");
        }
        
        var registryPath = _registryPaths[entityType];
        
        return new RegistryRepository<T>(_registryService, _encryptionService, _loggingService, registryPath);
    }
}
```

### Phase 3: Dependency Injection Setup (1 hour)
```csharp
// SystemConfig.Infrastructure/DependencyInjection/RepositoryServiceCollectionExtensions.cs
public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddRegistryRepositories(this IServiceCollection services)
    {
        services.AddScoped<IRegistryService, RegistryService>();
        services.AddScoped<IRepositoryFactory, RegistryRepositoryFactory>();
        
        // Register specific repositories
        services.AddScoped<IRepository<DatabaseConfiguration>>(provider =>
        {
            var factory = provider.GetRequiredService<IRepositoryFactory>();
            return factory.CreateRepository<DatabaseConfiguration>();
        });
        
        services.AddScoped<IRepository<PrinterConfiguration>>(provider =>
        {
            var factory = provider.GetRequiredService<IRepositoryFactory>();
            return factory.CreateRepository<PrinterConfiguration>();
        });
        
        services.AddScoped<IRepository<SystemConfiguration>>(provider =>
        {
            var factory = provider.GetRequiredService<IRepositoryFactory>();
            return factory.CreateRepository<SystemConfiguration>();
        });
        
        return services;
    }
}
```

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Infrastructure/Repositories/RegistryRepositoryTests.cs
public class RegistryRepositoryTests
{
    private readonly Mock<IRegistryService> _mockRegistryService;
    private readonly Mock<IEncryptionService> _mockEncryptionService;
    private readonly Mock<ILoggingService> _mockLoggingService;
    private readonly RegistryRepository<DatabaseConfiguration> _repository;
    
    public RegistryRepositoryTests()
    {
        _mockRegistryService = new Mock<IRegistryService>();
        _mockEncryptionService = new Mock<IEncryptionService>();
        _mockLoggingService = new Mock<ILoggingService>();
        
        _repository = new RegistryRepository<DatabaseConfiguration>(
            _mockRegistryService.Object,
            _mockEncryptionService.Object,
            _mockLoggingService.Object,
            @"HKEY_LOCAL_MACHINE\SOFTWARE\SystemConfig\Database");
    }
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var config = DatabaseConfiguration.Create("Test", "Description", 
            new ConnectionSettings("localhost", "testdb", "user", "pass"), 
            DatabaseType.SqlServer, "testuser");
        
        var jsonData = JsonSerializer.Serialize(config);
        var encryptedData = "encrypted_data";
        
        _mockRegistryService.Setup(x => x.GetValueAsync(It.IsAny<string>()))
            .ReturnsAsync(encryptedData);
        _mockEncryptionService.Setup(x => x.Decrypt(encryptedData))
            .Returns(jsonData);
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(config.Name, result.Name);
    }
    
    [Fact]
    public async Task AddAsync_WithValidEntity_ShouldSaveToRegistry()
    {
        // Arrange
        var config = DatabaseConfiguration.Create("Test", "Description", 
            new ConnectionSettings("localhost", "testdb", "user", "pass"), 
            DatabaseType.SqlServer, "testuser");
        
        var jsonData = JsonSerializer.Serialize(config);
        var encryptedData = "encrypted_data";
        
        _mockEncryptionService.Setup(x => x.Encrypt(jsonData))
            .Returns(encryptedData);
        _mockRegistryService.Setup(x => x.SetValueAsync(It.IsAny<string>(), encryptedData))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _repository.AddAsync(config);
        
        // Assert
        Assert.Equal(config, result);
        _mockRegistryService.Verify(x => x.SetValueAsync(It.IsAny<string>(), encryptedData), Times.Once);
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Infrastructure/Repositories/RegistryRepositoryIntegrationTests.cs
public class RegistryRepositoryIntegrationTests : IDisposable
{
    private readonly IRepository<DatabaseConfiguration> _repository;
    private readonly string _testRegistryPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\SystemConfig\Test";
    
    public RegistryRepositoryIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddRegistryRepositories();
        var serviceProvider = services.BuildServiceProvider();
        _repository = serviceProvider.GetRequiredService<IRepository<DatabaseConfiguration>>();
    }
    
    [Fact]
    public async Task RegistryRepository_ShouldSaveAndRetrieveEntity()
    {
        // Arrange
        var config = DatabaseConfiguration.Create("Test", "Description", 
            new ConnectionSettings("localhost", "testdb", "user", "pass"), 
            DatabaseType.SqlServer, "testuser");
        
        // Act
        var saved = await _repository.AddAsync(config);
        var retrieved = await _repository.GetByIdAsync(config.Id);
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(config.Name, retrieved.Name);
    }
    
    public void Dispose()
    {
        // Cleanup test registry entries
    }
}
```

## 📊 Definition of Done
- [ ] **Registry Repository**: RegistryRepository implementation hoàn thành
- [ ] **Registry Service**: IRegistryService implementation hoàn thành
- [ ] **Repository Factory**: RegistryRepositoryFactory implementation hoàn thành
- [ ] **Dependency Injection**: Repository DI setup hoàn thành
- [ ] **Security**: Encryption và permission validation hoàn thành
- [ ] **Error Handling**: Comprehensive error handling hoàn thành
- [ ] **Unit Tests**: >95% coverage cho registry repository
- [ ] **Integration Tests**: Registry operations tests pass
- [ ] **Code Review**: Registry repository được approve

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Windows Registry permission issues
- **Mitigation**: Implement proper permission checks và error handling

- **Risk**: Registry corruption during operations
- **Mitigation**: Implement backup/restore functionality

- **Risk**: Performance issues với large registry data
- **Mitigation**: Implement caching và optimization

### Security Risks
- **Risk**: Sensitive data exposure trong registry
- **Mitigation**: Implement proper encryption

- **Risk**: Registry access vulnerabilities
- **Mitigation**: Implement access control validation

## 📚 Resources & References
- Windows Registry Programming Guide
- .NET Registry Access Best Practices
- Registry Security Guidelines
- Registry Backup/Restore Patterns
- Clean Architecture Repository Patterns

## 🔄 Dependencies
- Task 1.3.1: Generic Repository Interface
- Task 1.1.2: Setup Dependency Injection
- Task 1.1.3: Configure Logging Framework

## 📈 Success Metrics
- Registry repository works correctly
- All CRUD operations functional
- Security features implemented
- Performance benchmarks met
- High test coverage achieved
- Error handling works properly

## 📝 Notes
- Implement proper registry permissions
- Use encryption cho sensitive data
- Implement backup/restore functionality
- Consider performance implications
- Document registry structure
- Regular security reviews 