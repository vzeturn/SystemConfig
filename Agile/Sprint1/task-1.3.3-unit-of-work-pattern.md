# Task 1.3.3: Unit of Work Pattern

## 📋 Task Overview
**Sprint**: 1  
**Story**: 1.3 - Basic Repository Pattern  
**Priority**: High  
**Estimated Hours**: 4  
**Assigned To**: Senior Developer  
**Dependencies**: Task 1.3.1 - Generic Repository Interface, Task 1.3.2 - Registry Repository Implementation

## 🎯 Objective
Implement Unit of Work pattern để quản lý transactions, ensure data consistency, và coordinate multiple repository operations cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Unit of Work Interface**:
  - Transaction management
  - Repository coordination
  - Change tracking
  - Commit/rollback operations
  - Disposal pattern

- [ ] **Transaction Features**:
  - Begin transaction
  - Commit transaction
  - Rollback transaction
  - Nested transactions
  - Transaction isolation

- [ ] **Repository Management**:
  - Dynamic repository creation
  - Repository caching
  - Change tracking
  - Batch operations
  - Audit trail

### Technical Requirements
- [ ] **Unit of Work Interface**:
  ```csharp
  // SystemConfig.Infrastructure/Repositories/IUnitOfWork.cs
  public interface IUnitOfWork : IDisposable
  {
      // Repository access
      IRepository<T> Repository<T>() where T : AggregateRoot;
      
      // Transaction management
      Task BeginTransactionAsync();
      Task CommitTransactionAsync();
      Task RollbackTransactionAsync();
      Task<bool> HasActiveTransactionAsync();
      
      // Change tracking
      Task<int> SaveChangesAsync();
      Task DiscardChangesAsync();
      Task<bool> HasChangesAsync();
      
      // Audit and logging
      Task<IEnumerable<ChangeLog>> GetChangeLogAsync();
      Task ClearChangeLogAsync();
  }
  ```

- [ ] **Change Tracking**:
  ```csharp
  // SystemConfig.Infrastructure/Repositories/ChangeLog.cs
  public class ChangeLog
  {
      public Guid Id { get; set; }
      public string EntityType { get; set; }
      public Guid EntityId { get; set; }
      public ChangeType ChangeType { get; set; }
      public DateTime Timestamp { get; set; }
      public string UserId { get; set; }
      public string Description { get; set; }
      public string OldValue { get; set; }
      public string NewValue { get; set; }
  }
  
  public enum ChangeType
  {
      Created,
      Updated,
      Deleted
  }
  ```

- [ ] **Unit of Work Implementation**:
  ```csharp
  // SystemConfig.Infrastructure/Repositories/UnitOfWork.cs
  public class UnitOfWork : IUnitOfWork
  {
      private readonly IRepositoryFactory _repositoryFactory;
      private readonly ILoggingService _loggingService;
      private readonly Dictionary<Type, object> _repositories;
      private readonly List<ChangeLog> _changeLog;
      private bool _disposed;
      private bool _hasActiveTransaction;
      
      public UnitOfWork(IRepositoryFactory repositoryFactory, ILoggingService loggingService)
      {
          _repositoryFactory = repositoryFactory;
          _loggingService = loggingService;
          _repositories = new Dictionary<Type, object>();
          _changeLog = new List<ChangeLog>();
      }
      
      public IRepository<T> Repository<T>() where T : AggregateRoot
      {
          var type = typeof(T);
          
          if (!_repositories.ContainsKey(type))
          {
              var repository = _repositoryFactory.CreateRepository<T>();
              _repositories[type] = repository;
          }
          
          return (IRepository<T>)_repositories[type];
      }
      
      public async Task BeginTransactionAsync()
      {
          if (_hasActiveTransaction)
          {
              throw new InvalidOperationException("Transaction already active");
          }
          
          try
          {
              // Begin transaction on all repositories
              foreach (var repository in _repositories.Values)
              {
                  var repo = repository as dynamic;
                  await repo.BeginTransactionAsync();
              }
              
              _hasActiveTransaction = true;
              _loggingService.LogInformation("Transaction begun");
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to begin transaction");
              throw;
          }
      }
      
      public async Task CommitTransactionAsync()
      {
          if (!_hasActiveTransaction)
          {
              throw new InvalidOperationException("No active transaction");
          }
          
          try
          {
              // Commit transaction on all repositories
              foreach (var repository in _repositories.Values)
              {
                  var repo = repository as dynamic;
                  await repo.CommitTransactionAsync();
              }
              
              _hasActiveTransaction = false;
              _loggingService.LogInformation("Transaction committed");
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to commit transaction");
              await RollbackTransactionAsync();
              throw;
          }
      }
      
      public async Task RollbackTransactionAsync()
      {
          if (!_hasActiveTransaction)
          {
              return; // No active transaction to rollback
          }
          
          try
          {
              // Rollback transaction on all repositories
              foreach (var repository in _repositories.Values)
              {
                  var repo = repository as dynamic;
                  await repo.RollbackTransactionAsync();
              }
              
              _hasActiveTransaction = false;
              _loggingService.LogInformation("Transaction rolled back");
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to rollback transaction");
              throw;
          }
      }
      
      public async Task<bool> HasActiveTransactionAsync()
      {
          return _hasActiveTransaction;
      }
      
      public async Task<int> SaveChangesAsync()
      {
          try
          {
              var changeCount = 0;
              
              // Save changes on all repositories
              foreach (var repository in _repositories.Values)
              {
                  var repo = repository as dynamic;
                  // Note: Registry repositories don't have SaveChangesAsync
                  // This would be more relevant for database repositories
                  changeCount++;
              }
              
              _loggingService.LogInformation("Changes saved: {Count}", changeCount);
              return changeCount;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to save changes");
              throw;
          }
      }
      
      public async Task DiscardChangesAsync()
      {
          try
          {
              _changeLog.Clear();
              _repositories.Clear();
              _loggingService.LogInformation("Changes discarded");
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to discard changes");
              throw;
          }
      }
      
      public async Task<bool> HasChangesAsync()
      {
          return _changeLog.Any();
      }
      
      public async Task<IEnumerable<ChangeLog>> GetChangeLogAsync()
      {
          return _changeLog.AsReadOnly();
      }
      
      public async Task ClearChangeLogAsync()
      {
          _changeLog.Clear();
      }
      
      public void Dispose()
      {
          Dispose(true);
          GC.SuppressFinalize(this);
      }
      
      protected virtual void Dispose(bool disposing)
      {
          if (!_disposed && disposing)
          {
              // Cleanup resources
              _repositories.Clear();
              _changeLog.Clear();
          }
          
          _disposed = true;
      }
  }
  ```

### Quality Requirements
- [ ] **Transaction Safety**: Proper transaction management
- [ ] **Resource Management**: Proper disposal pattern
- [ ] **Error Handling**: Comprehensive error handling
- [ ] **Logging**: Detailed audit logging
- [ ] **Thread Safety**: Thread-safe operations

## 🏗️ Implementation Plan

### Phase 1: Core Unit of Work Implementation (2 hours)
```csharp
// SystemConfig.Infrastructure/Repositories/UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly ILoggingService _loggingService;
    private readonly Dictionary<Type, object> _repositories;
    private readonly List<ChangeLog> _changeLog;
    private bool _disposed;
    private bool _hasActiveTransaction;
    
    public UnitOfWork(IRepositoryFactory repositoryFactory, ILoggingService loggingService)
    {
        _repositoryFactory = repositoryFactory;
        _loggingService = loggingService;
        _repositories = new Dictionary<Type, object>();
        _changeLog = new List<ChangeLog>();
    }
    
    public IRepository<T> Repository<T>() where T : AggregateRoot
    {
        var type = typeof(T);
        
        if (!_repositories.ContainsKey(type))
        {
            var repository = _repositoryFactory.CreateRepository<T>();
            _repositories[type] = repository;
        }
        
        return (IRepository<T>)_repositories[type];
    }
    
    public async Task BeginTransactionAsync()
    {
        if (_hasActiveTransaction)
        {
            throw new InvalidOperationException("Transaction already active");
        }
        
        try
        {
            // Begin transaction on all repositories
            foreach (var repository in _repositories.Values)
            {
                var repo = repository as dynamic;
                await repo.BeginTransactionAsync();
            }
            
            _hasActiveTransaction = true;
            _loggingService.LogInformation("Transaction begun");
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to begin transaction");
            throw;
        }
    }
    
    public async Task CommitTransactionAsync()
    {
        if (!_hasActiveTransaction)
        {
            throw new InvalidOperationException("No active transaction");
        }
        
        try
        {
            // Commit transaction on all repositories
            foreach (var repository in _repositories.Values)
            {
                var repo = repository as dynamic;
                await repo.CommitTransactionAsync();
            }
            
            _hasActiveTransaction = false;
            _loggingService.LogInformation("Transaction committed");
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to commit transaction");
            await RollbackTransactionAsync();
            throw;
        }
    }
    
    public async Task RollbackTransactionAsync()
    {
        if (!_hasActiveTransaction)
        {
            return; // No active transaction to rollback
        }
        
        try
        {
            // Rollback transaction on all repositories
            foreach (var repository in _repositories.Values)
            {
                var repo = repository as dynamic;
                await repo.RollbackTransactionAsync();
            }
            
            _hasActiveTransaction = false;
            _loggingService.LogInformation("Transaction rolled back");
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to rollback transaction");
            throw;
        }
    }
    
    public async Task<bool> HasActiveTransactionAsync()
    {
        return _hasActiveTransaction;
    }
    
    public async Task<int> SaveChangesAsync()
    {
        try
        {
            var changeCount = 0;
            
            // Save changes on all repositories
            foreach (var repository in _repositories.Values)
            {
                var repo = repository as dynamic;
                // Note: Registry repositories don't have SaveChangesAsync
                // This would be more relevant for database repositories
                changeCount++;
            }
            
            _loggingService.LogInformation("Changes saved: {Count}", changeCount);
            return changeCount;
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to save changes");
            throw;
        }
    }
    
    public async Task DiscardChangesAsync()
    {
        try
        {
            _changeLog.Clear();
            _repositories.Clear();
            _loggingService.LogInformation("Changes discarded");
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to discard changes");
            throw;
        }
    }
    
    public async Task<bool> HasChangesAsync()
    {
        return _changeLog.Any();
    }
    
    public async Task<IEnumerable<ChangeLog>> GetChangeLogAsync()
    {
        return _changeLog.AsReadOnly();
    }
    
    public async Task ClearChangeLogAsync()
    {
        _changeLog.Clear();
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Cleanup resources
            _repositories.Clear();
            _changeLog.Clear();
        }
        
        _disposed = true;
    }
}
```

### Phase 2: Change Tracking Implementation (1 hour)
```csharp
// SystemConfig.Infrastructure/Repositories/ChangeLog.cs
public class ChangeLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; }
    public Guid EntityId { get; set; }
    public ChangeType ChangeType { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public string Description { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    
    public ChangeLog()
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }
    
    public ChangeLog(string entityType, Guid entityId, ChangeType changeType, string userId, string description = null)
        : this()
    {
        EntityType = entityType;
        EntityId = entityId;
        ChangeType = changeType;
        UserId = userId;
        Description = description;
    }
}

public enum ChangeType
{
    Created,
    Updated,
    Deleted
}

// SystemConfig.Infrastructure/Repositories/ChangeTracker.cs
public class ChangeTracker
{
    private readonly List<ChangeLog> _changes;
    private readonly ILoggingService _loggingService;
    
    public ChangeTracker(ILoggingService loggingService)
    {
        _changes = new List<ChangeLog>();
        _loggingService = loggingService;
    }
    
    public void TrackChange(string entityType, Guid entityId, ChangeType changeType, string userId, string description = null)
    {
        var change = new ChangeLog(entityType, entityId, changeType, userId, description);
        _changes.Add(change);
        
        _loggingService.LogInformation("Change tracked: {EntityType} {EntityId} {ChangeType}", 
            entityType, entityId, changeType);
    }
    
    public void TrackChange(string entityType, Guid entityId, ChangeType changeType, string userId, string oldValue, string newValue)
    {
        var change = new ChangeLog(entityType, entityId, changeType, userId)
        {
            OldValue = oldValue,
            NewValue = newValue
        };
        _changes.Add(change);
        
        _loggingService.LogInformation("Change tracked: {EntityType} {EntityId} {ChangeType} - {OldValue} -> {NewValue}", 
            entityType, entityId, changeType, oldValue, newValue);
    }
    
    public IEnumerable<ChangeLog> GetChanges()
    {
        return _changes.AsReadOnly();
    }
    
    public void ClearChanges()
    {
        _changes.Clear();
    }
}
```

### Phase 3: Dependency Injection Setup (1 hour)
```csharp
// SystemConfig.Infrastructure/DependencyInjection/UnitOfWorkServiceCollectionExtensions.cs
public static class UnitOfWorkServiceCollectionExtensions
{
    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ChangeTracker>();
        
        return services;
    }
}

// SystemConfig.Infrastructure/Repositories/UnitOfWorkScope.cs
public class UnitOfWorkScope : IDisposable
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _loggingService;
    private bool _disposed;
    
    public UnitOfWorkScope(IUnitOfWork unitOfWork, ILoggingService loggingService)
    {
        _unitOfWork = unitOfWork;
        _loggingService = loggingService;
    }
    
    public IUnitOfWork UnitOfWork => _unitOfWork;
    
    public async Task CommitAsync()
    {
        try
        {
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            _loggingService.LogInformation("Unit of work committed successfully");
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to commit unit of work");
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    
    public async Task RollbackAsync()
    {
        try
        {
            await _unitOfWork.RollbackTransactionAsync();
            _loggingService.LogInformation("Unit of work rolled back");
        }
        catch (Exception ex)
        {
            _loggingService.LogError(ex, "Failed to rollback unit of work");
            throw;
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _unitOfWork?.Dispose();
        }
        
        _disposed = true;
    }
}
```

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Infrastructure/Repositories/UnitOfWorkTests.cs
public class UnitOfWorkTests
{
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<ILoggingService> _mockLoggingService;
    private readonly UnitOfWork _unitOfWork;
    
    public UnitOfWorkTests()
    {
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockLoggingService = new Mock<ILoggingService>();
        _unitOfWork = new UnitOfWork(_mockRepositoryFactory.Object, _mockLoggingService.Object);
    }
    
    [Fact]
    public void Repository_ShouldReturnCachedRepository()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<DatabaseConfiguration>>();
        _mockRepositoryFactory.Setup(x => x.CreateRepository<DatabaseConfiguration>())
            .Returns(mockRepository.Object);
        
        // Act
        var repo1 = _unitOfWork.Repository<DatabaseConfiguration>();
        var repo2 = _unitOfWork.Repository<DatabaseConfiguration>();
        
        // Assert
        Assert.Same(repo1, repo2);
        _mockRepositoryFactory.Verify(x => x.CreateRepository<DatabaseConfiguration>(), Times.Once);
    }
    
    [Fact]
    public async Task BeginTransactionAsync_ShouldBeginTransactionOnAllRepositories()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<DatabaseConfiguration>>();
        _mockRepositoryFactory.Setup(x => x.CreateRepository<DatabaseConfiguration>())
            .Returns(mockRepository.Object);
        
        var repo = _unitOfWork.Repository<DatabaseConfiguration>();
        
        // Act
        await _unitOfWork.BeginTransactionAsync();
        
        // Assert
        mockRepository.Verify(x => x.BeginTransactionAsync(), Times.Once);
    }
    
    [Fact]
    public async Task CommitTransactionAsync_WithActiveTransaction_ShouldCommit()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<DatabaseConfiguration>>();
        _mockRepositoryFactory.Setup(x => x.CreateRepository<DatabaseConfiguration>())
            .Returns(mockRepository.Object);
        
        var repo = _unitOfWork.Repository<DatabaseConfiguration>();
        await _unitOfWork.BeginTransactionAsync();
        
        // Act
        await _unitOfWork.CommitTransactionAsync();
        
        // Assert
        mockRepository.Verify(x => x.CommitTransactionAsync(), Times.Once);
    }
    
    [Fact]
    public async Task RollbackTransactionAsync_WithActiveTransaction_ShouldRollback()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<DatabaseConfiguration>>();
        _mockRepositoryFactory.Setup(x => x.CreateRepository<DatabaseConfiguration>())
            .Returns(mockRepository.Object);
        
        var repo = _unitOfWork.Repository<DatabaseConfiguration>();
        await _unitOfWork.BeginTransactionAsync();
        
        // Act
        await _unitOfWork.RollbackTransactionAsync();
        
        // Assert
        mockRepository.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Infrastructure/Repositories/UnitOfWorkIntegrationTests.cs
public class UnitOfWorkIntegrationTests : IDisposable
{
    private readonly IUnitOfWork _unitOfWork;
    
    public UnitOfWorkIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddRegistryRepositories();
        services.AddUnitOfWork();
        var serviceProvider = services.BuildServiceProvider();
        _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    }
    
    [Fact]
    public async Task UnitOfWork_ShouldCoordinateMultipleRepositories()
    {
        // Arrange
        var dbConfig = DatabaseConfiguration.Create("Test DB", "Description", 
            new ConnectionSettings("localhost", "testdb", "user", "pass"), 
            DatabaseType.SqlServer, "testuser");
        
        var printerConfig = PrinterConfiguration.Create("Test Printer", "Description", 
            new PrinterSettings("TestDevice", "COM1", "TestDriver"), "testuser");
        
        // Act
        await _unitOfWork.BeginTransactionAsync();
        
        var dbRepo = _unitOfWork.Repository<DatabaseConfiguration>();
        var printerRepo = _unitOfWork.Repository<PrinterConfiguration>();
        
        await dbRepo.AddAsync(dbConfig);
        await printerRepo.AddAsync(printerConfig);
        
        await _unitOfWork.CommitTransactionAsync();
        
        // Assert
        var retrievedDb = await dbRepo.GetByIdAsync(dbConfig.Id);
        var retrievedPrinter = await printerRepo.GetByIdAsync(printerConfig.Id);
        
        Assert.NotNull(retrievedDb);
        Assert.NotNull(retrievedPrinter);
    }
    
    public void Dispose()
    {
        _unitOfWork?.Dispose();
    }
}
```

## 📊 Definition of Done
- [ ] **Unit of Work Interface**: IUnitOfWork interface được implement đầy đủ
- [ ] **Unit of Work Implementation**: UnitOfWork class được implement
- [ ] **Change Tracking**: ChangeLog và ChangeTracker được implement
- [ ] **Transaction Management**: Transaction operations hoạt động đúng
- [ ] **Repository Coordination**: Multiple repositories được coordinate đúng
- [ ] **Unit Tests**: >95% coverage cho unit of work
- [ ] **Integration Tests**: Unit of work integration tests pass
- [ ] **Code Review**: Unit of work pattern được approve

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Complex transaction management
- **Mitigation**: Start với simple transactions, add complexity gradually

- **Risk**: Memory leaks với repository caching
- **Mitigation**: Implement proper disposal pattern

- **Risk**: Thread safety issues
- **Mitigation**: Use thread-safe collections và patterns

### Quality Risks
- **Risk**: Unit of work over-engineering
- **Mitigation**: Focus on essential functionality

- **Risk**: Transaction rollback complexity
- **Mitigation**: Implement simple rollback mechanisms

## 📚 Resources & References
- Unit of Work Pattern Best Practices
- Repository Pattern with Unit of Work
- .NET 8 Transaction Management
- Clean Architecture Unit of Work
- Change Tracking Patterns

## 🔄 Dependencies
- Task 1.3.1: Generic Repository Interface
- Task 1.3.2: Registry Repository Implementation
- Task 1.1.2: Setup Dependency Injection

## 📈 Success Metrics
- Unit of work coordinates repositories correctly
- Transactions work properly
- Change tracking functions correctly
- High test coverage achieved
- Performance benchmarks met
- Error handling works properly

## 📝 Notes
- Keep unit of work simple và focused
- Implement proper disposal pattern
- Use change tracking for audit trails
- Consider performance implications
- Document usage patterns clearly
- Regular code reviews cho unit of work implementations 