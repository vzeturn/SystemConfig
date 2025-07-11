# Task 1.1.1: Create Solution Structure

## 📋 Task Overview
**Sprint**: 1  
**Story**: 1.1 - Project Structure Setup  
**Priority**: High  
**Estimated Hours**: 8  
**Assigned To**: Senior Developer  
**Dependencies**: .NET 8 SDK, Visual Studio 2022

## 🎯 Objective
Thiết lập cấu trúc solution theo Clean Architecture pattern với 4 layer chính và project structure chuẩn cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Presentation Layer** (SystemConfig.Presentation):
  - Windows Forms UI project
  - Main dashboard form
  - Configuration forms (Database, Printer, System)
  - Backup manager form
  - Monitoring dashboard form
  - Common UI components library

- [ ] **Application Layer** (SystemConfig.Application):
  - CQRS implementation với MediatR
  - Command handlers cho write operations
  - Query handlers cho read operations
  - Event handlers cho notifications
  - Validation services
  - Form controllers cho UI business logic

- [ ] **Domain Layer** (SystemConfig.Domain):
  - Core domain entities (DatabaseConfiguration, PrinterConfiguration, SystemConfiguration)
  - Business rules và validation logic
  - Domain events cho audit trail
  - Value objects cho configuration settings
  - Aggregate roots và domain services

- [ ] **Infrastructure Layer** (SystemConfig.Infrastructure):
  - Windows Registry repository implementation
  - Database connection factory
  - Encryption services (AES-256)
  - Logging services (Serilog)
  - External service integrations

### Technical Requirements
- [ ] **Project Structure**:
  ```
  SystemConfig/
  ├── src/
  │   ├── SystemConfig.Presentation/          # Windows Forms UI
  │   │   ├── Forms/
  │   │   │   ├── MainDashboard.cs
  │   │   │   ├── DatabaseConfigForm.cs
  │   │   │   ├── PrinterConfigForm.cs
  │   │   │   ├── SystemConfigForm.cs
  │   │   │   └── BackupManagerForm.cs
  │   │   ├── Controls/
  │   │   │   ├── ConfigurationGrid.cs
  │   │   │   ├── HealthMonitor.cs
  │   │   │   └── NotificationPanel.cs
  │   │   └── Program.cs
  │   ├── SystemConfig.Application/           # Business logic, CQRS
  │   │   ├── Commands/
  │   │   │   ├── DatabaseConfiguration/
  │   │   │   ├── PrinterConfiguration/
  │   │   │   └── SystemConfiguration/
  │   │   ├── Queries/
  │   │   │   ├── DatabaseConfiguration/
  │   │   │   ├── PrinterConfiguration/
  │   │   │   └── SystemConfiguration/
  │   │   ├── Events/
  │   │   │   ├── ConfigurationChangedEvent.cs
  │   │   │   └── HealthStatusChangedEvent.cs
  │   │   ├── Services/
  │   │   │   ├── IDatabaseConfigurationService.cs
  │   │   │   ├── IPrinterConfigurationService.cs
  │   │   │   └── ISystemConfigurationService.cs
  │   │   └── Validation/
  │   │       ├── IValidationService.cs
  │   │       └── ValidationRules/
  │   ├── SystemConfig.Domain/               # Core entities, business rules
  │   │   ├── Entities/
  │   │   │   ├── DatabaseConfiguration.cs
  │   │   │   ├── PrinterConfiguration.cs
  │   │   │   └── SystemConfiguration.cs
  │   │   ├── ValueObjects/
  │   │   │   ├── ConnectionSettings.cs
  │   │   │   ├── PrinterSettings.cs
  │   │   │   └── ValidationRules.cs
  │   │   ├── Events/
  │   │   │   ├── DomainEvent.cs
  │   │   │   └── ConfigurationChangedEvent.cs
  │   │   ├── Exceptions/
  │   │   │   ├── DomainException.cs
  │   │   │   └── ValidationException.cs
  │   │   └── Services/
  │   │       ├── IDomainService.cs
  │   │       └── IConfigurationValidator.cs
  │   └── SystemConfig.Infrastructure/       # Data access, external services
  │       ├── Repositories/
  │       │   ├── IRepository.cs
  │       │   ├── IUnitOfWork.cs
  │       │   ├── RegistryRepository.cs
  │       │   └── WindowsRegistryRepository.cs
  │       ├── Services/
  │       │   ├── IRegistryService.cs
  │       │   ├── IEncryptionService.cs
  │       │   ├── ILoggingService.cs
  │       │   └── IDatabaseConnectionService.cs
  │       ├── Factories/
  │       │   ├── IDatabaseConnectionFactory.cs
  │       │   └── IPrinterServiceFactory.cs
  │       └── Configuration/
  │           ├── AppSettings.cs
  │           └── RegistrySettings.cs
  ├── tests/
  │   ├── SystemConfig.UnitTests/
  │   │   ├── Domain/
  │   │   ├── Application/
  │   │   └── Infrastructure/
  │   ├── SystemConfig.IntegrationTests/
  │   │   ├── Repository/
  │   │   ├── Services/
  │   │   └── EndToEnd/
  │   └── SystemConfig.UITests/
  │       ├── Forms/
  │       └── Controls/
  └── docs/
      ├── Architecture/
      │   ├── CleanArchitecture.md
      │   ├── DomainModel.md
      │   └── ServiceLayer.md
      ├── API/
      │   ├── ApplicationServices.md
      │   └── DomainServices.md
      └── UserGuide/
          ├── Installation.md
          └── UserManual.md
  ```

- [ ] **Dependencies Configuration**:
  - SystemConfig.Presentation → SystemConfig.Application
  - SystemConfig.Application → SystemConfig.Domain
  - SystemConfig.Infrastructure → SystemConfig.Domain
  - SystemConfig.Presentation → SystemConfig.Infrastructure (for UI services)

### Quality Requirements
- [ ] **Code Standards**:
  - Clean Code principles
  - SOLID principles compliance
  - Design patterns implementation
  - Consistent naming conventions
  - Proper separation of concerns

- [ ] **Documentation**:
  - Architecture decision records (ADRs)
  - API documentation
  - Code comments cho complex logic
  - README files cho mỗi project

## 🏗️ Implementation Plan

### Phase 1: Solution Creation (2 hours)
```bash
# Create solution structure
dotnet new sln -n SystemConfig
dotnet new winforms -n SystemConfig.Presentation
dotnet new classlib -n SystemConfig.Application
dotnet new classlib -n SystemConfig.Domain
dotnet new classlib -n SystemConfig.Infrastructure
dotnet new xunit -n SystemConfig.UnitTests
dotnet new xunit -n SystemConfig.IntegrationTests
dotnet new xunit -n SystemConfig.UITests

# Add projects to solution
dotnet sln add src/SystemConfig.Presentation/SystemConfig.Presentation.csproj
dotnet sln add src/SystemConfig.Application/SystemConfig.Application.csproj
dotnet sln add src/SystemConfig.Domain/SystemConfig.Domain.csproj
dotnet sln add src/SystemConfig.Infrastructure/SystemConfig.Infrastructure.csproj
dotnet sln add tests/SystemConfig.UnitTests/SystemConfig.UnitTests.csproj
dotnet sln add tests/SystemConfig.IntegrationTests/SystemConfig.IntegrationTests.csproj
dotnet sln add tests/SystemConfig.UITests/SystemConfig.UITests.csproj
```

### Phase 2: Project References Setup (2 hours)
```xml
<!-- SystemConfig.Presentation.csproj -->
<ItemGroup>
  <ProjectReference Include="..\SystemConfig.Application\SystemConfig.Application.csproj" />
  <ProjectReference Include="..\SystemConfig.Infrastructure\SystemConfig.Infrastructure.csproj" />
</ItemGroup>

<!-- SystemConfig.Application.csproj -->
<ItemGroup>
  <ProjectReference Include="..\SystemConfig.Domain\SystemConfig.Domain.csproj" />
</ItemGroup>

<!-- SystemConfig.Infrastructure.csproj -->
<ItemGroup>
  <ProjectReference Include="..\SystemConfig.Domain\SystemConfig.Domain.csproj" />
</ItemGroup>
```

### Phase 3: Base Classes & Interfaces (2 hours)
```csharp
// SystemConfig.Domain/Entities/AggregateRoot.cs
public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }
    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

// SystemConfig.Domain/ValueObjects/ValueObject.cs
public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;
            
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }
    
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }
}

// SystemConfig.Infrastructure/Repositories/IRepository.cs
public interface IRepository<T> where T : AggregateRoot
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

// SystemConfig.Infrastructure/Repositories/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : AggregateRoot;
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### Phase 4: Configuration & Logging Setup (2 hours)
```csharp
// SystemConfig.Infrastructure/Configuration/AppSettings.cs
public class AppSettings
{
    public DatabaseSettings Database { get; set; } = new();
    public SecuritySettings Security { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}

public class DatabaseSettings
{
    public string DefaultConnectionString { get; set; } = string.Empty;
    public int ConnectionTimeout { get; set; } = 30;
    public int CommandTimeout { get; set; } = 60;
}

public class SecuritySettings
{
    public string EncryptionKey { get; set; } = string.Empty;
    public bool EnableAuditLogging { get; set; } = true;
}

public class LoggingSettings
{
    public string LogLevel { get; set; } = "Information";
    public string LogFilePath { get; set; } = "logs/systemconfig.log";
}
```

## 🧪 Testing Strategy

### Unit Tests Structure
```csharp
// SystemConfig.UnitTests/Domain/DatabaseConfigurationTests.cs
public class DatabaseConfigurationTests
{
    [Fact]
    public void Create_WithValidSettings_ShouldCreateConfiguration()
    {
        // Arrange
        var connectionSettings = new ConnectionSettings("localhost", "testdb", "user", "pass");
        
        // Act
        var config = DatabaseConfiguration.Create("Test Config", connectionSettings, DatabaseType.SqlServer);
        
        // Assert
        Assert.NotNull(config);
        Assert.Equal("Test Config", config.Name);
        Assert.Equal(DatabaseType.SqlServer, config.DatabaseType);
    }
    
    [Fact]
    public void SetAsDefault_WhenAnotherDefaultExists_ShouldThrowException()
    {
        // Arrange
        var config1 = DatabaseConfiguration.Create("Config 1", connectionSettings, DatabaseType.SqlServer);
        var config2 = DatabaseConfiguration.Create("Config 2", connectionSettings, DatabaseType.SqlServer);
        config1.SetAsDefault();
        
        // Act & Assert
        Assert.Throws<DomainException>(() => config2.SetAsDefault());
    }
}
```

### Integration Tests Structure
```csharp
// SystemConfig.IntegrationTests/Repository/RegistryRepositoryTests.cs
public class RegistryRepositoryTests : IDisposable
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<DatabaseConfiguration> _repository;
    
    public RegistryRepositoryTests()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure();
        var serviceProvider = services.BuildServiceProvider();
        
        _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        _repository = _unitOfWork.Repository<DatabaseConfiguration>();
    }
    
    [Fact]
    public async Task SaveAndRetrieve_ShouldPersistConfiguration()
    {
        // Arrange
        var config = DatabaseConfiguration.Create("Test", connectionSettings, DatabaseType.SqlServer);
        
        // Act
        await _repository.AddAsync(config);
        await _unitOfWork.SaveChangesAsync();
        
        var retrieved = await _repository.GetByIdAsync(config.Id);
        
        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(config.Name, retrieved.Name);
    }
}
```

## 📊 Definition of Done
- [ ] **Solution Structure**: Tất cả projects được tạo đúng Clean Architecture
- [ ] **Project References**: Dependencies được setup đúng
- [ ] **Base Classes**: AggregateRoot, ValueObject, IRepository được implement
- [ ] **Configuration**: AppSettings và configuration loading hoạt động
- [ ] **Logging**: Serilog được configure đúng
- [ ] **Unit Tests**: >95% coverage cho domain layer
- [ ] **Integration Tests**: Repository tests pass 100%
- [ ] **Code Review**: Architecture được approve
- [ ] **Documentation**: README và architecture docs hoàn thành

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Clean Architecture complexity cho team mới
- **Mitigation**: Provide detailed documentation và training

- **Risk**: Project reference circular dependencies
- **Mitigation**: Strict dependency direction enforcement

- **Risk**: Performance issues với Windows Registry access
- **Mitigation**: Implement caching và async operations

### Timeline Risks
- **Risk**: Over-engineering initial structure
- **Mitigation**: Focus on MVP, optimize later

- **Risk**: Team learning curve với Clean Architecture
- **Mitigation**: Pair programming và code reviews

## 📚 Resources & References
- Clean Architecture by Robert C. Martin
- Domain-Driven Design by Eric Evans
- .NET 8 Documentation
- Serilog Documentation
- Windows Registry Programming Guide

## 🔄 Dependencies
- .NET 8 SDK installed
- Visual Studio 2022 or VS Code
- Git repository setup
- Team understanding of Clean Architecture

## 📈 Success Metrics
- Solution builds successfully without errors
- All unit tests pass với >95% coverage
- Integration tests pass 100%
- No circular dependencies detected
- Architecture review approved
- Team can navigate và understand structure

## 📝 Notes
- Prioritize simplicity over perfection in initial setup
- Document architectural decisions for team reference
- Consider future scalability requirements
- Ensure team understands Clean Architecture principles
- Regular architecture reviews during development 