# Task 1.3.4: Basic CRUD Operations

## 📋 Task Overview
**Sprint**: 1  
**Story**: 1.3 - Basic Repository Pattern  
**Priority**: High  
**Estimated Hours**: 4  
**Assigned To**: Senior Developer  
**Dependencies**: Task 1.3.1 - Generic Repository Interface, Task 1.3.2 - Registry Repository Implementation, Task 1.3.3 - Unit of Work Pattern

## 🎯 Objective
Implement basic CRUD operations với validation, error handling, và business logic cho tất cả domain entities trong POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **CRUD Operations**:
  - Create operations với validation
  - Read operations với filtering
  - Update operations với business rules
  - Delete operations với constraints
  - Bulk operations support

- [ ] **Business Logic Integration**:
  - Domain validation rules
  - Business constraint enforcement
  - Audit trail generation
  - Change tracking
  - Event publishing

- [ ] **Error Handling**:
  - Validation errors
  - Business rule violations
  - Concurrency conflicts
  - Data integrity issues
  - System errors

### Technical Requirements
- [ ] **CRUD Service Interface**:
  ```csharp
  // SystemConfig.Application/Services/ICrudService.cs
  public interface ICrudService<T> where T : AggregateRoot
  {
      // Basic CRUD operations
      Task<T> CreateAsync(T entity, string userId);
      Task<T> GetByIdAsync(Guid id);
      Task<IEnumerable<T>> GetAllAsync();
      Task<T> UpdateAsync(T entity, string userId);
      Task DeleteAsync(Guid id, string userId);
      Task<bool> ExistsAsync(Guid id);
      
      // Query operations
      Task<IEnumerable<T>> FindAsync(ISpecification<T> specification);
      Task<T> FindOneAsync(ISpecification<T> specification);
      Task<int> CountAsync(ISpecification<T> specification);
      
      // Pagination
      Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize);
      Task<PagedResult<T>> GetPagedAsync(ISpecification<T> specification, int pageNumber, int pageSize);
      
      // Bulk operations
      Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities, string userId);
      Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, string userId);
      Task DeleteRangeAsync(IEnumerable<Guid> ids, string userId);
  }
  ```

- [ ] **CRUD Service Implementation**:
  ```csharp
  // SystemConfig.Application/Services/CrudService.cs
  public class CrudService<T> : ICrudService<T> where T : AggregateRoot
  {
      private readonly IRepository<T> _repository;
      private readonly IUnitOfWork _unitOfWork;
      private readonly IValidationService _validationService;
      private readonly ILoggingService _loggingService;
      private readonly IEventPublisher _eventPublisher;
      
      public CrudService(
          IRepository<T> repository,
          IUnitOfWork unitOfWork,
          IValidationService validationService,
          ILoggingService loggingService,
          IEventPublisher eventPublisher)
      {
          _repository = repository;
          _unitOfWork = unitOfWork;
          _validationService = validationService;
          _loggingService = loggingService;
          _eventPublisher = eventPublisher;
      }
      
      public async Task<T> CreateAsync(T entity, string userId)
      {
          try
          {
              _loggingService.LogInformation("Creating entity: {Type} {Id}", typeof(T).Name, entity.Id);
              
              // Validate entity
              var validationResult = await _validationService.ValidateAsync(entity);
              if (!validationResult.IsValid)
              {
                  throw new ValidationException(validationResult.Errors);
              }
              
              // Check business rules
              await ValidateBusinessRulesAsync(entity, userId);
              
              // Save entity
              var savedEntity = await _repository.AddAsync(entity);
              await _unitOfWork.SaveChangesAsync();
              
              // Publish events
              await PublishDomainEventsAsync(entity);
              
              _loggingService.LogInformation("Entity created successfully: {Type} {Id}", typeof(T).Name, entity.Id);
              
              return savedEntity;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to create entity: {Type} {Id}", typeof(T).Name, entity.Id);
              throw;
          }
      }
      
      public async Task<T> GetByIdAsync(Guid id)
      {
          try
          {
              _loggingService.LogInformation("Getting entity by ID: {Type} {Id}", typeof(T).Name, id);
              
              var entity = await _repository.GetByIdAsync(id);
              
              if (entity == null)
              {
                  _loggingService.LogWarning("Entity not found: {Type} {Id}", typeof(T).Name, id);
                  return null;
              }
              
              _loggingService.LogInformation("Entity retrieved successfully: {Type} {Id}", typeof(T).Name, id);
              
              return entity;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to get entity by ID: {Type} {Id}", typeof(T).Name, id);
              throw;
          }
      }
      
      public async Task<IEnumerable<T>> GetAllAsync()
      {
          try
          {
              _loggingService.LogInformation("Getting all entities: {Type}", typeof(T).Name);
              
              var entities = await _repository.GetAllAsync();
              
              _loggingService.LogInformation("Retrieved {Count} entities: {Type}", entities.Count(), typeof(T).Name);
              
              return entities;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to get all entities: {Type}", typeof(T).Name);
              throw;
          }
      }
      
      public async Task<T> UpdateAsync(T entity, string userId)
      {
          try
          {
              _loggingService.LogInformation("Updating entity: {Type} {Id}", typeof(T).Name, entity.Id);
              
              // Validate entity
              var validationResult = await _validationService.ValidateAsync(entity);
              if (!validationResult.IsValid)
              {
                  throw new ValidationException(validationResult.Errors);
              }
              
              // Check business rules
              await ValidateBusinessRulesAsync(entity, userId);
              
              // Update entity
              var updatedEntity = await _repository.UpdateAsync(entity);
              await _unitOfWork.SaveChangesAsync();
              
              // Publish events
              await PublishDomainEventsAsync(entity);
              
              _loggingService.LogInformation("Entity updated successfully: {Type} {Id}", typeof(T).Name, entity.Id);
              
              return updatedEntity;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to update entity: {Type} {Id}", typeof(T).Name, entity.Id);
              throw;
          }
      }
      
      public async Task DeleteAsync(Guid id, string userId)
      {
          try
          {
              _loggingService.LogInformation("Deleting entity: {Type} {Id}", typeof(T).Name, id);
              
              // Check if entity exists
              var entity = await _repository.GetByIdAsync(id);
              if (entity == null)
              {
                  throw new EntityNotFoundException($"Entity not found: {typeof(T).Name} {id}");
              }
              
              // Check business rules for deletion
              await ValidateDeletionRulesAsync(entity, userId);
              
              // Delete entity
              await _repository.DeleteAsync(id);
              await _unitOfWork.SaveChangesAsync();
              
              // Publish events
              await PublishDomainEventsAsync(entity);
              
              _loggingService.LogInformation("Entity deleted successfully: {Type} {Id}", typeof(T).Name, id);
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to delete entity: {Type} {Id}", typeof(T).Name, id);
              throw;
          }
      }
      
      public async Task<bool> ExistsAsync(Guid id)
      {
          try
          {
              return await _repository.ExistsAsync(id);
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to check entity existence: {Type} {Id}", typeof(T).Name, id);
              return false;
          }
      }
      
      public async Task<IEnumerable<T>> FindAsync(ISpecification<T> specification)
      {
          try
          {
              _loggingService.LogInformation("Finding entities with specification: {Type}", typeof(T).Name);
              
              var entities = await _repository.FindAsync(specification);
              
              _loggingService.LogInformation("Found {Count} entities with specification: {Type}", entities.Count(), typeof(T).Name);
              
              return entities;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to find entities with specification: {Type}", typeof(T).Name);
              throw;
          }
      }
      
      public async Task<T> FindOneAsync(ISpecification<T> specification)
      {
          try
          {
              _loggingService.LogInformation("Finding one entity with specification: {Type}", typeof(T).Name);
              
              var entity = await _repository.FindOneAsync(specification);
              
              if (entity != null)
              {
                  _loggingService.LogInformation("Found entity with specification: {Type} {Id}", typeof(T).Name, entity.Id);
              }
              else
              {
                  _loggingService.LogInformation("No entity found with specification: {Type}", typeof(T).Name);
              }
              
              return entity;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to find one entity with specification: {Type}", typeof(T).Name);
              throw;
          }
      }
      
      public async Task<int> CountAsync(ISpecification<T> specification)
      {
          try
          {
              var count = await _repository.CountAsync(specification);
              
              _loggingService.LogInformation("Counted {Count} entities with specification: {Type}", count, typeof(T).Name);
              
              return count;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to count entities with specification: {Type}", typeof(T).Name);
              throw;
          }
      }
      
      public async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize)
      {
          try
          {
              _loggingService.LogInformation("Getting paged entities: {Type} Page {Page} Size {Size}", typeof(T).Name, pageNumber, pageSize);
              
              var result = await _repository.GetPagedAsync(pageNumber, pageSize);
              
              _loggingService.LogInformation("Retrieved paged entities: {Type} {Count} of {Total}", typeof(T).Name, result.Items.Count(), result.TotalCount);
              
              return result;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to get paged entities: {Type}", typeof(T).Name);
              throw;
          }
      }
      
      public async Task<PagedResult<T>> GetPagedAsync(ISpecification<T> specification, int pageNumber, int pageSize)
      {
          try
          {
              _loggingService.LogInformation("Getting paged entities with specification: {Type} Page {Page} Size {Size}", typeof(T).Name, pageNumber, pageSize);
              
              var result = await _repository.GetPagedAsync(specification, pageNumber, pageSize);
              
              _loggingService.LogInformation("Retrieved paged entities with specification: {Type} {Count} of {Total}", typeof(T).Name, result.Items.Count(), result.TotalCount);
              
              return result;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to get paged entities with specification: {Type}", typeof(T).Name);
              throw;
          }
      }
      
      public async Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities, string userId)
      {
          try
          {
              _loggingService.LogInformation("Creating {Count} entities: {Type}", entities.Count(), typeof(T).Name);
              
              var createdEntities = new List<T>();
              
              foreach (var entity in entities)
              {
                  var createdEntity = await CreateAsync(entity, userId);
                  createdEntities.Add(createdEntity);
              }
              
              _loggingService.LogInformation("Created {Count} entities successfully: {Type}", createdEntities.Count, typeof(T).Name);
              
              return createdEntities;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to create range of entities: {Type}", typeof(T).Name);
              throw;
          }
      }
      
      public async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, string userId)
      {
          try
          {
              _loggingService.LogInformation("Updating {Count} entities: {Type}", entities.Count(), typeof(T).Name);
              
              var updatedEntities = new List<T>();
              
              foreach (var entity in entities)
              {
                  var updatedEntity = await UpdateAsync(entity, userId);
                  updatedEntities.Add(updatedEntity);
              }
              
              _loggingService.LogInformation("Updated {Count} entities successfully: {Type}", updatedEntities.Count, typeof(T).Name);
              
              return updatedEntities;
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to update range of entities: {Type}", typeof(T).Name);
              throw;
          }
      }
      
      public async Task DeleteRangeAsync(IEnumerable<Guid> ids, string userId)
      {
          try
          {
              _loggingService.LogInformation("Deleting {Count} entities: {Type}", ids.Count(), typeof(T).Name);
              
              foreach (var id in ids)
              {
                  await DeleteAsync(id, userId);
              }
              
              _loggingService.LogInformation("Deleted {Count} entities successfully: {Type}", ids.Count(), typeof(T).Name);
          }
          catch (Exception ex)
          {
              _loggingService.LogError(ex, "Failed to delete range of entities: {Type}", typeof(T).Name);
              throw;
          }
      }
      
      private async Task ValidateBusinessRulesAsync(T entity, string userId)
      {
          // Implement business rule validation based on entity type
          if (entity is DatabaseConfiguration dbConfig)
          {
              await ValidateDatabaseConfigurationRulesAsync(dbConfig, userId);
          }
          else if (entity is PrinterConfiguration printerConfig)
          {
              await ValidatePrinterConfigurationRulesAsync(printerConfig, userId);
          }
          else if (entity is SystemConfiguration sysConfig)
          {
              await ValidateSystemConfigurationRulesAsync(sysConfig, userId);
          }
      }
      
      private async Task ValidateDeletionRulesAsync(T entity, string userId)
      {
          // Implement deletion rule validation based on entity type
          if (entity is DatabaseConfiguration dbConfig)
          {
              await ValidateDatabaseConfigurationDeletionRulesAsync(dbConfig, userId);
          }
          else if (entity is PrinterConfiguration printerConfig)
          {
              await ValidatePrinterConfigurationDeletionRulesAsync(printerConfig, userId);
          }
          else if (entity is SystemConfiguration sysConfig)
          {
              await ValidateSystemConfigurationDeletionRulesAsync(sysConfig, userId);
          }
      }
      
      private async Task PublishDomainEventsAsync(T entity)
      {
          foreach (var domainEvent in entity.DomainEvents)
          {
              await _eventPublisher.PublishAsync(domainEvent);
          }
          
          entity.ClearDomainEvents();
      }
      
      // Business rule validation methods for specific entity types
      private async Task ValidateDatabaseConfigurationRulesAsync(DatabaseConfiguration config, string userId)
      {
          // Implement database-specific business rules
          if (config.IsDefault)
          {
              // Check if another default configuration exists
              var existingDefault = await FindOneAsync(new ByDefaultSpecification<DatabaseConfiguration>());
              if (existingDefault != null && existingDefault.Id != config.Id)
              {
                  throw new BusinessRuleViolationException("Only one default database configuration is allowed");
              }
          }
      }
      
      private async Task ValidatePrinterConfigurationRulesAsync(PrinterConfiguration config, string userId)
      {
          // Implement printer-specific business rules
          if (config.Status == PrinterStatus.Offline)
          {
              throw new BusinessRuleViolationException("Cannot modify offline printer configuration");
          }
      }
      
      private async Task ValidateSystemConfigurationRulesAsync(SystemConfiguration config, string userId)
      {
          // Implement system-specific business rules
          if (config.AccessLevel == AccessLevel.ReadOnly && userId != "admin")
          {
              throw new BusinessRuleViolationException("Only admin can modify read-only configurations");
          }
      }
      
      private async Task ValidateDatabaseConfigurationDeletionRulesAsync(DatabaseConfiguration config, string userId)
      {
          if (config.IsDefault)
          {
              throw new BusinessRuleViolationException("Cannot delete default database configuration");
          }
      }
      
      private async Task ValidatePrinterConfigurationDeletionRulesAsync(PrinterConfiguration config, string userId)
      {
          if (config.AssignedCategories.Any())
          {
              throw new BusinessRuleViolationException("Cannot delete printer with assigned categories");
          }
      }
      
      private async Task ValidateSystemConfigurationDeletionRulesAsync(SystemConfiguration config, string userId)
      {
          if (config.Category == ConfigurationCategory.System)
          {
              throw new BusinessRuleViolationException("Cannot delete system configurations");
          }
      }
  }
  ```

### Quality Requirements
- [ ] **Validation**: Comprehensive input validation
- [ ] **Business Rules**: Business logic enforcement
- [ ] **Error Handling**: Proper exception handling
- [ ] **Logging**: Detailed audit logging
- [ ] **Performance**: Efficient operations

## 🏗️ Implementation Plan

### Phase 1: CRUD Service Implementation (2 hours)
```csharp
// SystemConfig.Application/Services/CrudService.cs
public class CrudService<T> : ICrudService<T> where T : AggregateRoot
{
    private readonly IRepository<T> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidationService _validationService;
    private readonly ILoggingService _loggingService;
    private readonly IEventPublisher _eventPublisher;
    
    public CrudService(
        IRepository<T> repository,
        IUnitOfWork unitOfWork,
        IValidationService validationService,
        ILoggingService loggingService,
        IEventPublisher eventPublisher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _validationService = validationService;
        _loggingService = loggingService;
        _eventPublisher = eventPublisher;
    }
    
    // Implementation methods as shown above
}
```

### Phase 2: Business Rule Validation (1 hour)
```csharp
// SystemConfig.Application/Services/BusinessRuleValidator.cs
public class BusinessRuleValidator
{
    private readonly ILoggingService _loggingService;
    
    public BusinessRuleValidator(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }
    
    public async Task ValidateDatabaseConfigurationRulesAsync(DatabaseConfiguration config, string userId)
    {
        // Implement database-specific business rules
        if (config.IsDefault)
        {
            // Check if another default configuration exists
            // This would require access to repository
            _loggingService.LogInformation("Validating default database configuration: {Id}", config.Id);
        }
    }
    
    public async Task ValidatePrinterConfigurationRulesAsync(PrinterConfiguration config, string userId)
    {
        // Implement printer-specific business rules
        if (config.Status == PrinterStatus.Offline)
        {
            throw new BusinessRuleViolationException("Cannot modify offline printer configuration");
        }
    }
    
    public async Task ValidateSystemConfigurationRulesAsync(SystemConfiguration config, string userId)
    {
        // Implement system-specific business rules
        if (config.AccessLevel == AccessLevel.ReadOnly && userId != "admin")
        {
            throw new BusinessRuleViolationException("Only admin can modify read-only configurations");
        }
    }
}
```

### Phase 3: Dependency Injection Setup (1 hour)
```csharp
// SystemConfig.Application/DependencyInjection/CrudServiceCollectionExtensions.cs
public static class CrudServiceCollectionExtensions
{
    public static IServiceCollection AddCrudServices(this IServiceCollection services)
    {
        // Register generic CRUD service
        services.AddScoped(typeof(ICrudService<>), typeof(CrudService<>));
        
        // Register business rule validator
        services.AddScoped<BusinessRuleValidator>();
        
        // Register event publisher
        services.AddScoped<IEventPublisher, EventPublisher>();
        
        return services;
    }
}

// SystemConfig.Application/Services/IEventPublisher.cs
public interface IEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent);
}

// SystemConfig.Application/Services/EventPublisher.cs
public class EventPublisher : IEventPublisher
{
    private readonly ILoggingService _loggingService;
    
    public EventPublisher(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }
    
    public async Task PublishAsync(DomainEvent domainEvent)
    {
        _loggingService.LogInformation("Publishing domain event: {EventType}", domainEvent.GetType().Name);
        
        // In a real implementation, this would publish to an event bus
        // For now, we just log the event
        await Task.CompletedTask;
    }
}
```

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Application/Services/CrudServiceTests.cs
public class CrudServiceTests
{
    private readonly Mock<IRepository<DatabaseConfiguration>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IValidationService> _mockValidationService;
    private readonly Mock<ILoggingService> _mockLoggingService;
    private readonly Mock<IEventPublisher> _mockEventPublisher;
    private readonly CrudService<DatabaseConfiguration> _crudService;
    
    public CrudServiceTests()
    {
        _mockRepository = new Mock<IRepository<DatabaseConfiguration>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockValidationService = new Mock<IValidationService>();
        _mockLoggingService = new Mock<ILoggingService>();
        _mockEventPublisher = new Mock<IEventPublisher>();
        
        _crudService = new CrudService<DatabaseConfiguration>(
            _mockRepository.Object,
            _mockUnitOfWork.Object,
            _mockValidationService.Object,
            _mockLoggingService.Object,
            _mockEventPublisher.Object);
    }
    
    [Fact]
    public async Task CreateAsync_WithValidEntity_ShouldCreateSuccessfully()
    {
        // Arrange
        var config = DatabaseConfiguration.Create("Test", "Description", 
            new ConnectionSettings("localhost", "testdb", "user", "pass"), 
            DatabaseType.SqlServer, "testuser");
        
        _mockValidationService.Setup(x => x.ValidateAsync(config))
            .ReturnsAsync(ValidationResult.Success());
        
        _mockRepository.Setup(x => x.AddAsync(config))
            .ReturnsAsync(config);
        
        // Act
        var result = await _crudService.CreateAsync(config, "testuser");
        
        // Assert
        Assert.Equal(config, result);
        _mockRepository.Verify(x => x.AddAsync(config), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task CreateAsync_WithInvalidEntity_ShouldThrowValidationException()
    {
        // Arrange
        var config = DatabaseConfiguration.Create("Test", "Description", 
            new ConnectionSettings("localhost", "testdb", "user", "pass"), 
            DatabaseType.SqlServer, "testuser");
        
        var validationErrors = new List<ValidationError>
        {
            new ValidationError("Name", "Name is required")
        };
        
        _mockValidationService.Setup(x => x.ValidateAsync(config))
            .ReturnsAsync(ValidationResult.Failure(validationErrors));
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => 
            _crudService.CreateAsync(config, "testuser"));
    }
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var config = DatabaseConfiguration.Create("Test", "Description", 
            new ConnectionSettings("localhost", "testdb", "user", "pass"), 
            DatabaseType.SqlServer, "testuser");
        
        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(config);
        
        // Act
        var result = await _crudService.GetByIdAsync(id);
        
        // Assert
        Assert.Equal(config, result);
    }
    
    [Fact]
    public async Task UpdateAsync_WithValidEntity_ShouldUpdateSuccessfully()
    {
        // Arrange
        var config = DatabaseConfiguration.Create("Test", "Description", 
            new ConnectionSettings("localhost", "testdb", "user", "pass"), 
            DatabaseType.SqlServer, "testuser");
        
        _mockValidationService.Setup(x => x.ValidateAsync(config))
            .ReturnsAsync(ValidationResult.Success());
        
        _mockRepository.Setup(x => x.UpdateAsync(config))
            .ReturnsAsync(config);
        
        // Act
        var result = await _crudService.UpdateAsync(config, "testuser");
        
        // Assert
        Assert.Equal(config, result);
        _mockRepository.Verify(x => x.UpdateAsync(config), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var config = DatabaseConfiguration.Create("Test", "Description", 
            new ConnectionSettings("localhost", "testdb", "user", "pass"), 
            DatabaseType.SqlServer, "testuser");
        
        _mockRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(config);
        
        _mockRepository.Setup(x => x.DeleteAsync(id))
            .Returns(Task.CompletedTask);
        
        // Act
        await _crudService.DeleteAsync(id, "testuser");
        
        // Assert
        _mockRepository.Verify(x => x.DeleteAsync(id), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Application/Services/CrudServiceIntegrationTests.cs
public class CrudServiceIntegrationTests : IDisposable
{
    private readonly ICrudService<DatabaseConfiguration> _crudService;
    
    public CrudServiceIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddRegistryRepositories();
        services.AddUnitOfWork();
        services.AddCrudServices();
        var serviceProvider = services.BuildServiceProvider();
        _crudService = serviceProvider.GetRequiredService<ICrudService<DatabaseConfiguration>>();
    }
    
    [Fact]
    public async Task CrudService_ShouldPerformFullCRUDCycle()
    {
        // Arrange
        var config = DatabaseConfiguration.Create("Test", "Description", 
            new ConnectionSettings("localhost", "testdb", "user", "pass"), 
            DatabaseType.SqlServer, "testuser");
        
        // Act - Create
        var created = await _crudService.CreateAsync(config, "testuser");
        
        // Assert - Create
        Assert.NotNull(created);
        Assert.Equal(config.Name, created.Name);
        
        // Act - Read
        var retrieved = await _crudService.GetByIdAsync(created.Id);
        
        // Assert - Read
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        
        // Act - Update
        retrieved.UpdateName("Updated Test", "testuser");
        var updated = await _crudService.UpdateAsync(retrieved, "testuser");
        
        // Assert - Update
        Assert.Equal("Updated Test", updated.Name);
        
        // Act - Delete
        await _crudService.DeleteAsync(updated.Id, "testuser");
        
        // Assert - Delete
        var deleted = await _crudService.GetByIdAsync(updated.Id);
        Assert.Null(deleted);
    }
    
    public void Dispose()
    {
        // Cleanup
    }
}
```

## 📊 Definition of Done
- [ ] **CRUD Service Interface**: ICrudService interface được implement đầy đủ
- [ ] **CRUD Service Implementation**: CrudService class được implement
- [ ] **Business Rule Validation**: Business rule validation được implement
- [ ] **Error Handling**: Comprehensive error handling hoàn thành
- [ ] **Event Publishing**: Domain events được publish đúng
- [ ] **Unit Tests**: >95% coverage cho CRUD services
- [ ] **Integration Tests**: CRUD operations tests pass
- [ ] **Code Review**: CRUD operations được approve

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Complex business rule validation
- **Mitigation**: Start với simple rules, add complexity gradually

- **Risk**: Performance issues với large datasets
- **Mitigation**: Implement pagination và optimization

- **Risk**: Concurrency conflicts
- **Mitigation**: Implement proper locking mechanisms

### Quality Risks
- **Risk**: Business rules không đầy đủ
- **Mitigation**: Regular stakeholder review

- **Risk**: Validation logic không chính xác
- **Mitigation**: Comprehensive unit tests

## 📚 Resources & References
- CRUD Operations Best Practices
- Business Rule Validation Patterns
- Domain Event Publishing
- .NET 8 Service Patterns
- Clean Architecture CRUD Design

## 🔄 Dependencies
- Task 1.3.1: Generic Repository Interface
- Task 1.3.2: Registry Repository Implementation
- Task 1.3.3: Unit of Work Pattern
- Task 1.1.2: Setup Dependency Injection

## 📈 Success Metrics
- CRUD operations work correctly
- Business rules enforced properly
- Validation works accurately
- High test coverage achieved
- Performance benchmarks met
- Error handling works properly

## 📝 Notes
- Keep CRUD operations simple và focused
- Implement proper business rule validation
- Use domain events for side effects
- Consider performance implications
- Document business rules clearly
- Regular code reviews cho CRUD implementations 