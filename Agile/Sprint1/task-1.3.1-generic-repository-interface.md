# Task 1.3.1: Generic Repository Interface

## 📋 Task Overview
**Sprint**: 1  
**Story**: 1.3 - Basic Repository Pattern  
**Priority**: High  
**Estimated Hours**: 4  
**Assigned To**: Senior Developer  
**Dependencies**: Task 1.1.1 - Create Solution Structure, Task 1.1.2 - Setup Dependency Injection

## 🎯 Objective
Implement generic repository interface với CRUD operations, query specifications, và async support cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Generic Repository Interface**:
  - CRUD operations (Create, Read, Update, Delete)
  - Async operations support
  - Query specifications pattern
  - Pagination support
  - Bulk operations
  - Transaction support

- [ ] **Query Specifications**:
  - Filter specifications
  - Sort specifications
  - Include specifications
  - Composite specifications
  - Reusable query patterns

- [ ] **Repository Features**:
  - Type-safe operations
  - Error handling
  - Performance optimization
  - Caching support
  - Audit trail integration

### Technical Requirements
- [ ] **Repository Interface Structure**:
  ```csharp
  // SystemConfig.Infrastructure/Repositories/IRepository.cs
  public interface IRepository<T> where T : AggregateRoot
  {
      // Basic CRUD operations
      Task<T> GetByIdAsync(Guid id);
      Task<IEnumerable<T>> GetAllAsync();
      Task<T> AddAsync(T entity);
      Task<T> UpdateAsync(T entity);
      Task DeleteAsync(Guid id);
      Task<bool> ExistsAsync(Guid id);
      
      // Query operations
      Task<IEnumerable<T>> FindAsync(ISpecification<T> specification);
      Task<T> FindOneAsync(ISpecification<T> specification);
      Task<int> CountAsync(ISpecification<T> specification);
      
      // Pagination
      Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize);
      Task<PagedResult<T>> GetPagedAsync(ISpecification<T> specification, int pageNumber, int pageSize);
      
      // Bulk operations
      Task AddRangeAsync(IEnumerable<T> entities);
      Task UpdateRangeAsync(IEnumerable<T> entities);
      Task DeleteRangeAsync(IEnumerable<Guid> ids);
      
      // Transaction support
      Task BeginTransactionAsync();
      Task CommitTransactionAsync();
      Task RollbackTransactionAsync();
  }
  ```

- [ ] **Specification Pattern**:
  ```csharp
  // SystemConfig.Infrastructure/Repositories/ISpecification.cs
  public interface ISpecification<T>
  {
      Expression<Func<T, bool>> Criteria { get; }
      List<Expression<Func<T, object>>> Includes { get; }
      List<string> IncludeStrings { get; }
      Expression<Func<T, object>> OrderBy { get; }
      Expression<Func<T, object>> OrderByDescending { get; }
      int Take { get; }
      int Skip { get; }
      bool IsPagingEnabled { get; }
  }
  
  // SystemConfig.Infrastructure/Repositories/BaseSpecification.cs
  public abstract class BaseSpecification<T> : ISpecification<T>
  {
      public Expression<Func<T, bool>> Criteria { get; private set; }
      public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();
      public List<string> IncludeStrings { get; } = new List<string>();
      public Expression<Func<T, object>> OrderBy { get; private set; }
      public Expression<Func<T, object>> OrderByDescending { get; private set; }
      public int Take { get; private set; }
      public int Skip { get; private set; }
      public bool IsPagingEnabled { get; private set; }
      
      protected void AddCriteria(Expression<Func<T, bool>> criteria)
      {
          Criteria = criteria;
      }
      
      protected void AddInclude(Expression<Func<T, object>> includeExpression)
      {
          Includes.Add(includeExpression);
      }
      
      protected void AddInclude(string includeString)
      {
          IncludeStrings.Add(includeString);
      }
      
      protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
      {
          OrderBy = orderByExpression;
      }
      
      protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
      {
          OrderByDescending = orderByDescExpression;
      }
      
      protected void ApplyPaging(int skip, int take)
      {
          Skip = skip;
          Take = take;
          IsPagingEnabled = true;
      }
  }
  ```

- [ ] **Paged Result**:
  ```csharp
  // SystemConfig.Infrastructure/Repositories/PagedResult.cs
  public class PagedResult<T>
  {
      public IEnumerable<T> Items { get; set; }
      public int TotalCount { get; set; }
      public int PageNumber { get; set; }
      public int PageSize { get; set; }
      public int TotalPages { get; set; }
      public bool HasPreviousPage { get; set; }
      public bool HasNextPage { get; set; }
      
      public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
      {
          Items = items;
          TotalCount = totalCount;
          PageNumber = pageNumber;
          PageSize = pageSize;
          TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
          HasPreviousPage = pageNumber > 1;
          HasNextPage = pageNumber < TotalPages;
      }
  }
  ```

### Quality Requirements
- [ ] **Type Safety**: Strongly-typed operations
- [ ] **Performance**: Efficient query execution
- [ ] **Flexibility**: Easy to extend và customize
- [ ] **Testability**: Easy to mock và test
- [ ] **Error Handling**: Proper exception handling

## 🏗️ Implementation Plan

### Phase 1: Core Repository Interface (2 hours)
```csharp
// SystemConfig.Infrastructure/Repositories/IRepository.cs
public interface IRepository<T> where T : AggregateRoot
{
    // Basic CRUD operations
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    
    // Query operations
    Task<IEnumerable<T>> FindAsync(ISpecification<T> specification);
    Task<T> FindOneAsync(ISpecification<T> specification);
    Task<int> CountAsync(ISpecification<T> specification);
    
    // Pagination
    Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize);
    Task<PagedResult<T>> GetPagedAsync(ISpecification<T> specification, int pageNumber, int pageSize);
    
    // Bulk operations
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateRangeAsync(IEnumerable<T> entities);
    Task DeleteRangeAsync(IEnumerable<Guid> ids);
    
    // Transaction support
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

// SystemConfig.Infrastructure/Repositories/IRepositoryFactory.cs
public interface IRepositoryFactory
{
    IRepository<T> CreateRepository<T>() where T : AggregateRoot;
}
```

### Phase 2: Specification Pattern Implementation (1 hour)
```csharp
// SystemConfig.Infrastructure/Repositories/ISpecification.cs
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>> OrderBy { get; }
    Expression<Func<T, object>> OrderByDescending { get; }
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}

// SystemConfig.Infrastructure/Repositories/BaseSpecification.cs
public abstract class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>> Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();
    public List<string> IncludeStrings { get; } = new List<string>();
    public Expression<Func<T, object>> OrderBy { get; private set; }
    public Expression<Func<T, object>> OrderByDescending { get; private set; }
    public int Take { get; private set; }
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    
    protected void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }
    
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }
    
    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }
    
    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }
    
    protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }
    
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}

// SystemConfig.Infrastructure/Repositories/SpecificationEvaluator.cs
public class SpecificationEvaluator<T> where T : AggregateRoot
{
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
    {
        var query = inputQuery;
        
        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }
        
        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }
        
        if (spec.IsPagingEnabled)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }
        
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));
        query = spec.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));
        
        return query;
    }
}
```

### Phase 3: Common Specifications (1 hour)
```csharp
// SystemConfig.Infrastructure/Repositories/Specifications/ByIdSpecification.cs
public class ByIdSpecification<T> : BaseSpecification<T> where T : AggregateRoot
{
    public ByIdSpecification(Guid id)
    {
        AddCriteria(x => x.Id == id);
    }
}

// SystemConfig.Infrastructure/Repositories/Specifications/ByNameSpecification.cs
public class ByNameSpecification<T> : BaseSpecification<T> where T : AggregateRoot
{
    public ByNameSpecification(string name)
    {
        AddCriteria(x => x.Name.Contains(name));
    }
}

// SystemConfig.Infrastructure/Repositories/Specifications/ByDateRangeSpecification.cs
public class ByDateRangeSpecification<T> : BaseSpecification<T> where T : AggregateRoot
{
    public ByDateRangeSpecification(DateTime fromDate, DateTime toDate)
    {
        AddCriteria(x => x.CreatedAt >= fromDate && x.CreatedAt <= toDate);
    }
}

// SystemConfig.Infrastructure/Repositories/Specifications/PagedSpecification.cs
public class PagedSpecification<T> : BaseSpecification<T> where T : AggregateRoot
{
    public PagedSpecification(int pageNumber, int pageSize)
    {
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
    }
}
```

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Infrastructure/Repositories/IRepositoryTests.cs
public class IRepositoryTests
{
    [Fact]
    public void Repository_ShouldSupportBasicCRUD()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<DatabaseConfiguration>>();
        var config = DatabaseConfiguration.Create("Test", "Description", 
            new ConnectionSettings("localhost", "testdb", "user", "pass"), 
            DatabaseType.SqlServer, "testuser");
        
        // Act & Assert
        mockRepository.Setup(x => x.AddAsync(config)).ReturnsAsync(config);
        mockRepository.Setup(x => x.GetByIdAsync(config.Id)).ReturnsAsync(config);
        mockRepository.Setup(x => x.UpdateAsync(config)).ReturnsAsync(config);
        mockRepository.Setup(x => x.DeleteAsync(config.Id)).Returns(Task.CompletedTask);
        
        Assert.NotNull(mockRepository.Object);
    }
}

// SystemConfig.UnitTests/Infrastructure/Repositories/SpecificationTests.cs
public class SpecificationTests
{
    [Fact]
    public void ByIdSpecification_ShouldCreateCorrectCriteria()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Act
        var spec = new ByIdSpecification<DatabaseConfiguration>(id);
        
        // Assert
        Assert.NotNull(spec.Criteria);
    }
    
    [Fact]
    public void PagedSpecification_ShouldSetPagingCorrectly()
    {
        // Arrange
        var pageNumber = 2;
        var pageSize = 10;
        
        // Act
        var spec = new PagedSpecification<DatabaseConfiguration>(pageNumber, pageSize);
        
        // Assert
        Assert.True(spec.IsPagingEnabled);
        Assert.Equal(10, spec.Skip);
        Assert.Equal(10, spec.Take);
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Infrastructure/Repositories/RepositoryIntegrationTests.cs
public class RepositoryIntegrationTests : IDisposable
{
    private readonly IRepository<DatabaseConfiguration> _repository;
    
    public RepositoryIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure();
        var serviceProvider = services.BuildServiceProvider();
        _repository = serviceProvider.GetRequiredService<IRepository<DatabaseConfiguration>>();
    }
    
    [Fact]
    public async Task Repository_ShouldSaveAndRetrieveEntity()
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
        // Cleanup
    }
}
```

## 📊 Definition of Done
- [ ] **Repository Interface**: IRepository interface được implement đầy đủ
- [ ] **Specification Pattern**: ISpecification và BaseSpecification được implement
- [ ] **Common Specifications**: ById, ByName, Paged specifications được implement
- [ ] **Paged Result**: PagedResult class được implement
- [ ] **Unit Tests**: >95% coverage cho repository interfaces
- [ ] **Integration Tests**: Repository integration tests pass
- [ ] **Code Review**: Repository pattern được approve
- [ ] **Documentation**: Repository usage documentation hoàn thành

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Complex specification pattern implementation
- **Mitigation**: Start với simple specifications, add complexity gradually

- **Risk**: Performance issues với complex queries
- **Mitigation**: Implement query optimization và caching

- **Risk**: Transaction management complexity
- **Mitigation**: Use simple transaction patterns first

### Quality Risks
- **Risk**: Repository pattern over-engineering
- **Mitigation**: Focus on essential functionality

- **Risk**: Specification pattern learning curve
- **Mitigation**: Provide clear documentation và examples

## 📚 Resources & References
- Repository Pattern Best Practices
- Specification Pattern Implementation
- .NET 8 Generic Repository Patterns
- Entity Framework Core Repository Pattern
- Clean Architecture Repository Design

## 🔄 Dependencies
- Task 1.1.1: Create Solution Structure
- Task 1.1.2: Setup Dependency Injection
- Task 1.2.1: Database Configuration Entity
- Task 1.2.2: Printer Configuration Entity
- Task 1.2.3: System Configuration Entity

## 📈 Success Metrics
- Repository interface follows best practices
- Specification pattern works correctly
- All CRUD operations functional
- Paging works properly
- High test coverage achieved
- Performance benchmarks met

## 📝 Notes
- Keep repository interface simple và focused
- Use specification pattern for complex queries
- Implement proper error handling
- Consider performance implications
- Document usage patterns clearly
- Regular code reviews cho repository implementations 