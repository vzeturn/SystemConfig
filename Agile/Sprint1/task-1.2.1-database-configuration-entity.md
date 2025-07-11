# Task 1.2.1: Database Configuration Entity

## 📋 Task Overview
**Sprint**: 1  
**Story**: 1.2 - Domain Models Development  
**Priority**: High  
**Estimated Hours**: 10  
**Assigned To**: Domain Expert  
**Dependencies**: Task 1.1.1 - Create Solution Structure, Task 1.1.2 - Setup Dependency Injection

## 🎯 Objective
Implement DatabaseConfiguration aggregate root với business rules, validation logic, và domain events theo Domain-Driven Design principles cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **DatabaseConfiguration Aggregate Root**:
  - Unique identifier với DatabaseConfigurationId
  - Name và description fields
  - Connection settings với encryption
  - Database type (SQL Server, MySQL, PostgreSQL)
  - Default configuration management
  - Health status tracking
  - Creation và modification timestamps

- [ ] **Business Rules Implementation**:
  - Chỉ có 1 configuration được set làm default
  - Connection string phải được validate trước khi lưu
  - Automatic backup trước khi update
  - Health check định kỳ
  - Configuration changes trigger notifications

- [ ] **Value Objects**:
  - ConnectionSettings với validation
  - DatabaseConfigurationId với uniqueness
  - HealthStatus enum
  - DatabaseType enum

### Technical Requirements
- [ ] **Entity Structure**:
  ```csharp
  // SystemConfig.Domain/Entities/DatabaseConfiguration.cs
  public class DatabaseConfiguration : AggregateRoot
  {
      public DatabaseConfigurationId Id { get; private set; }
      public string Name { get; private set; }
      public string Description { get; private set; }
      public ConnectionSettings ConnectionSettings { get; private set; }
      public DatabaseType DatabaseType { get; private set; }
      public bool IsDefault { get; private set; }
      public HealthStatus HealthStatus { get; private set; }
      public DateTime CreatedAt { get; private set; }
      public DateTime? LastModifiedAt { get; private set; }
      public string CreatedBy { get; private set; }
      public string LastModifiedBy { get; private set; }
      
      // Business methods
      public void UpdateConnectionSettings(ConnectionSettings settings, string modifiedBy)
      public void SetAsDefault(string modifiedBy)
      public void UpdateHealthStatus(HealthStatus status)
      public void ValidateConnection()
      public void UpdateName(string name, string modifiedBy)
      public void UpdateDescription(string description, string modifiedBy)
      public void MarkAsInactive(string modifiedBy)
  }
  ```

- [ ] **Value Objects**:
  ```csharp
  // SystemConfig.Domain/ValueObjects/DatabaseConfigurationId.cs
  public class DatabaseConfigurationId : ValueObject
  {
      public Guid Value { get; }
      
      public DatabaseConfigurationId(Guid value)
      {
          if (value == Guid.Empty)
              throw new DomainException("Database configuration ID cannot be empty");
          
          Value = value;
      }
      
      public static DatabaseConfigurationId New() => new DatabaseConfigurationId(Guid.NewGuid());
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Value;
      }
      
      public static implicit operator Guid(DatabaseConfigurationId id) => id.Value;
      public static implicit operator DatabaseConfigurationId(Guid value) => new DatabaseConfigurationId(value);
  }
  
  // SystemConfig.Domain/ValueObjects/ConnectionSettings.cs
  public class ConnectionSettings : ValueObject
  {
      public string Server { get; }
      public string Database { get; }
      public string Username { get; }
      public string Password { get; }
      public int Port { get; }
      public bool UseSSL { get; }
      public int ConnectionTimeout { get; }
      public int CommandTimeout { get; }
      public int MaxPoolSize { get; }
      public int MinPoolSize { get; }
      
      public ConnectionSettings(
          string server, 
          string database, 
          string username, 
          string password,
          int port = 0,
          bool useSSL = false,
          int connectionTimeout = 30,
          int commandTimeout = 60,
          int maxPoolSize = 100,
          int minPoolSize = 5)
      {
          if (string.IsNullOrWhiteSpace(server))
              throw new DomainException("Server cannot be empty");
          
          if (string.IsNullOrWhiteSpace(database))
              throw new DomainException("Database cannot be empty");
          
          if (string.IsNullOrWhiteSpace(username))
              throw new DomainException("Username cannot be empty");
          
          if (string.IsNullOrWhiteSpace(password))
              throw new DomainException("Password cannot be empty");
          
          if (connectionTimeout <= 0)
              throw new DomainException("Connection timeout must be greater than 0");
          
          if (commandTimeout <= 0)
              throw new DomainException("Command timeout must be greater than 0");
          
          Server = server;
          Database = database;
          Username = username;
          Password = password;
          Port = port;
          UseSSL = useSSL;
          ConnectionTimeout = connectionTimeout;
          CommandTimeout = commandTimeout;
          MaxPoolSize = maxPoolSize;
          MinPoolSize = minPoolSize;
      }
      
      public string GetConnectionString()
      {
          var builder = new SqlConnectionStringBuilder
          {
              DataSource = Server,
              InitialCatalog = Database,
              UserID = Username,
              Password = Password,
              ConnectTimeout = ConnectionTimeout,
              CommandTimeout = CommandTimeout,
              MaxPoolSize = MaxPoolSize,
              MinPoolSize = MinPoolSize,
              IntegratedSecurity = false
          };
          
          if (Port > 0)
              builder.DataSource = $"{Server},{Port}";
          
          return builder.ConnectionString;
      }
      
      public bool IsValid()
      {
          return !string.IsNullOrWhiteSpace(Server) &&
                 !string.IsNullOrWhiteSpace(Database) &&
                 !string.IsNullOrWhiteSpace(Username) &&
                 !string.IsNullOrWhiteSpace(Password) &&
                 ConnectionTimeout > 0 &&
                 CommandTimeout > 0;
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Server;
          yield return Database;
          yield return Username;
          yield return Password;
          yield return Port;
          yield return UseSSL;
          yield return ConnectionTimeout;
          yield return CommandTimeout;
          yield return MaxPoolSize;
          yield return MinPoolSize;
      }
  }
  ```

- [ ] **Enums**:
  ```csharp
  // SystemConfig.Domain/Enums/DatabaseType.cs
  public enum DatabaseType
  {
      SqlServer,
      MySQL,
      PostgreSQL,
      Oracle,
      SQLite
  }
  
  // SystemConfig.Domain/Enums/HealthStatus.cs
  public enum HealthStatus
  {
      Unknown,
      Healthy,
      Warning,
      Error,
      Disconnected
  }
  ```

### Quality Requirements
- [ ] **Domain Logic**: Business rules được implement đúng
- [ ] **Validation**: Comprehensive validation cho tất cả inputs
- [ ] **Immutability**: Value objects immutable
- [ ] **Domain Events**: Events được publish cho important changes
- [ ] **Testability**: Easy to test và mock

## 🏗️ Implementation Plan

### Phase 1: Core Entity Implementation (4 hours)
```csharp
// SystemConfig.Domain/Entities/DatabaseConfiguration.cs
public class DatabaseConfiguration : AggregateRoot
{
    public DatabaseConfigurationId Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ConnectionSettings ConnectionSettings { get; private set; }
    public DatabaseType DatabaseType { get; private set; }
    public bool IsDefault { get; private set; }
    public HealthStatus HealthStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public string CreatedBy { get; private set; }
    public string LastModifiedBy { get; private set; }
    
    // Private constructor for EF Core
    private DatabaseConfiguration() { }
    
    // Factory method
    public static DatabaseConfiguration Create(
        string name,
        string description,
        ConnectionSettings connectionSettings,
        DatabaseType databaseType,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new DomainException("Created by cannot be empty");
        
        var configuration = new DatabaseConfiguration
        {
            Id = DatabaseConfigurationId.New(),
            Name = name,
            Description = description ?? string.Empty,
            ConnectionSettings = connectionSettings ?? throw new DomainException("Connection settings cannot be null"),
            DatabaseType = databaseType,
            IsDefault = false,
            HealthStatus = HealthStatus.Unknown,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
        
        configuration.AddDomainEvent(new DatabaseConfigurationCreatedEvent(configuration.Id, configuration.Name));
        
        return configuration;
    }
    
    public void UpdateConnectionSettings(ConnectionSettings settings, string modifiedBy)
    {
        if (settings == null)
            throw new DomainException("Connection settings cannot be null");
        
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        var oldSettings = ConnectionSettings;
        ConnectionSettings = settings;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new DatabaseConfigurationConnectionSettingsUpdatedEvent(Id, oldSettings, settings));
    }
    
    public void SetAsDefault(string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        if (IsDefault)
            return; // Already default
        
        IsDefault = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new DatabaseConfigurationSetAsDefaultEvent(Id, Name));
    }
    
    public void RemoveDefaultStatus(string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        if (!IsDefault)
            return; // Not default
        
        IsDefault = false;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new DatabaseConfigurationRemovedDefaultEvent(Id, Name));
    }
    
    public void UpdateHealthStatus(HealthStatus status)
    {
        var oldStatus = HealthStatus;
        HealthStatus = status;
        
        if (oldStatus != status)
        {
            AddDomainEvent(new DatabaseConfigurationHealthStatusChangedEvent(Id, oldStatus, status));
        }
    }
    
    public void UpdateName(string name, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        var oldName = Name;
        Name = name;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new DatabaseConfigurationNameUpdatedEvent(Id, oldName, name));
    }
    
    public void UpdateDescription(string description, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        var oldDescription = Description;
        Description = description ?? string.Empty;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new DatabaseConfigurationDescriptionUpdatedEvent(Id, oldDescription, Description));
    }
    
    public void ValidateConnection()
    {
        if (!ConnectionSettings.IsValid())
        {
            UpdateHealthStatus(HealthStatus.Error);
            throw new DomainException("Connection settings are invalid");
        }
        
        // Additional validation logic can be added here
        UpdateHealthStatus(HealthStatus.Healthy);
    }
}
```

### Phase 2: Domain Events Implementation (3 hours)
```csharp
// SystemConfig.Domain/Events/DatabaseConfigurationEvents.cs
public class DatabaseConfigurationCreatedEvent : DomainEvent
{
    public DatabaseConfigurationId ConfigurationId { get; }
    public string Name { get; }
    
    public DatabaseConfigurationCreatedEvent(DatabaseConfigurationId configurationId, string name)
    {
        ConfigurationId = configurationId;
        Name = name;
    }
}

public class DatabaseConfigurationConnectionSettingsUpdatedEvent : DomainEvent
{
    public DatabaseConfigurationId ConfigurationId { get; }
    public ConnectionSettings OldSettings { get; }
    public ConnectionSettings NewSettings { get; }
    
    public DatabaseConfigurationConnectionSettingsUpdatedEvent(
        DatabaseConfigurationId configurationId,
        ConnectionSettings oldSettings,
        ConnectionSettings newSettings)
    {
        ConfigurationId = configurationId;
        OldSettings = oldSettings;
        NewSettings = newSettings;
    }
}

public class DatabaseConfigurationSetAsDefaultEvent : DomainEvent
{
    public DatabaseConfigurationId ConfigurationId { get; }
    public string Name { get; }
    
    public DatabaseConfigurationSetAsDefaultEvent(DatabaseConfigurationId configurationId, string name)
    {
        ConfigurationId = configurationId;
        Name = name;
    }
}

public class DatabaseConfigurationHealthStatusChangedEvent : DomainEvent
{
    public DatabaseConfigurationId ConfigurationId { get; }
    public HealthStatus OldStatus { get; }
    public HealthStatus NewStatus { get; }
    
    public DatabaseConfigurationHealthStatusChangedEvent(
        DatabaseConfigurationId configurationId,
        HealthStatus oldStatus,
        HealthStatus newStatus)
    {
        ConfigurationId = configurationId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}
```

### Phase 3: Business Rules & Validation (3 hours)
```csharp
// SystemConfig.Domain/Services/IDatabaseConfigurationValidator.cs
public interface IDatabaseConfigurationValidator
{
    Task<ValidationResult> ValidateAsync(DatabaseConfiguration configuration);
    Task<ValidationResult> ValidateConnectionStringAsync(string connectionString);
    Task<ValidationResult> ValidateDefaultConfigurationAsync(DatabaseConfiguration configuration);
}

// SystemConfig.Domain/Services/DatabaseConfigurationValidator.cs
public class DatabaseConfigurationValidator : IDatabaseConfigurationValidator
{
    public async Task<ValidationResult> ValidateAsync(DatabaseConfiguration configuration)
    {
        var errors = new List<ValidationError>();
        
        // Validate name
        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            errors.Add(new ValidationError("Name", "Name cannot be empty"));
        }
        
        // Validate connection settings
        if (configuration.ConnectionSettings == null)
        {
            errors.Add(new ValidationError("ConnectionSettings", "Connection settings cannot be null"));
        }
        else if (!configuration.ConnectionSettings.IsValid())
        {
            errors.Add(new ValidationError("ConnectionSettings", "Connection settings are invalid"));
        }
        
        // Validate database type
        if (!Enum.IsDefined(typeof(DatabaseType), configuration.DatabaseType))
        {
            errors.Add(new ValidationError("DatabaseType", "Invalid database type"));
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors) 
            : ValidationResult.Success();
    }
    
    public async Task<ValidationResult> ValidateConnectionStringAsync(string connectionString)
    {
        var errors = new List<ValidationError>();
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            errors.Add(new ValidationError("ConnectionString", "Connection string cannot be empty"));
            return ValidationResult.Failure(errors);
        }
        
        try
        {
            // Test connection string format
            var builder = new SqlConnectionStringBuilder(connectionString);
            
            // Validate required fields
            if (string.IsNullOrWhiteSpace(builder.DataSource))
            {
                errors.Add(new ValidationError("Server", "Server cannot be empty"));
            }
            
            if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
            {
                errors.Add(new ValidationError("Database", "Database cannot be empty"));
            }
            
            if (string.IsNullOrWhiteSpace(builder.UserID))
            {
                errors.Add(new ValidationError("Username", "Username cannot be empty"));
            }
            
            if (string.IsNullOrWhiteSpace(builder.Password))
            {
                errors.Add(new ValidationError("Password", "Password cannot be empty"));
            }
        }
        catch (Exception ex)
        {
            errors.Add(new ValidationError("ConnectionString", $"Invalid connection string format: {ex.Message}"));
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors) 
            : ValidationResult.Success();
    }
    
    public async Task<ValidationResult> ValidateDefaultConfigurationAsync(DatabaseConfiguration configuration)
    {
        var errors = new List<ValidationError>();
        
        if (configuration.IsDefault)
        {
            // Additional validation for default configuration
            if (configuration.HealthStatus == HealthStatus.Error)
            {
                errors.Add(new ValidationError("HealthStatus", "Default configuration cannot have error status"));
            }
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors) 
            : ValidationResult.Success();
    }
}
```

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Domain/DatabaseConfigurationTests.cs
public class DatabaseConfigurationTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateConfiguration()
    {
        // Arrange
        var name = "Test Database";
        var description = "Test description";
        var connectionSettings = new ConnectionSettings("localhost", "testdb", "user", "pass");
        var databaseType = DatabaseType.SqlServer;
        var createdBy = "testuser";
        
        // Act
        var configuration = DatabaseConfiguration.Create(name, description, connectionSettings, databaseType, createdBy);
        
        // Assert
        Assert.NotNull(configuration);
        Assert.Equal(name, configuration.Name);
        Assert.Equal(description, configuration.Description);
        Assert.Equal(connectionSettings, configuration.ConnectionSettings);
        Assert.Equal(databaseType, configuration.DatabaseType);
        Assert.False(configuration.IsDefault);
        Assert.Equal(HealthStatus.Unknown, configuration.HealthStatus);
        Assert.Equal(createdBy, configuration.CreatedBy);
        Assert.True(configuration.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
    }
    
    [Theory]
    [InlineData("", "description", "user")]
    [InlineData(null, "description", "user")]
    [InlineData("name", "description", "")]
    [InlineData("name", "description", null)]
    public void Create_WithInvalidParameters_ShouldThrowException(string name, string description, string createdBy)
    {
        // Arrange
        var connectionSettings = new ConnectionSettings("localhost", "testdb", "user", "pass");
        var databaseType = DatabaseType.SqlServer;
        
        // Act & Assert
        Assert.Throws<DomainException>(() => 
            DatabaseConfiguration.Create(name, description, connectionSettings, databaseType, createdBy));
    }
    
    [Fact]
    public void SetAsDefault_ShouldSetDefaultStatus()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var modifiedBy = "testuser";
        
        // Act
        configuration.SetAsDefault(modifiedBy);
        
        // Assert
        Assert.True(configuration.IsDefault);
        Assert.Equal(modifiedBy, configuration.LastModifiedBy);
        Assert.True(configuration.LastModifiedAt > DateTime.UtcNow.AddMinutes(-1));
    }
    
    [Fact]
    public void UpdateConnectionSettings_WithValidSettings_ShouldUpdateSettings()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var newSettings = new ConnectionSettings("newserver", "newdb", "newuser", "newpass");
        var modifiedBy = "testuser";
        
        // Act
        configuration.UpdateConnectionSettings(newSettings, modifiedBy);
        
        // Assert
        Assert.Equal(newSettings, configuration.ConnectionSettings);
        Assert.Equal(modifiedBy, configuration.LastModifiedBy);
        Assert.True(configuration.LastModifiedAt > DateTime.UtcNow.AddMinutes(-1));
    }
    
    [Fact]
    public void UpdateHealthStatus_ShouldUpdateStatus()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var newStatus = HealthStatus.Healthy;
        
        // Act
        configuration.UpdateHealthStatus(newStatus);
        
        // Assert
        Assert.Equal(newStatus, configuration.HealthStatus);
    }
    
    private DatabaseConfiguration CreateTestConfiguration()
    {
        var connectionSettings = new ConnectionSettings("localhost", "testdb", "user", "pass");
        return DatabaseConfiguration.Create("Test", "Description", connectionSettings, DatabaseType.SqlServer, "testuser");
    }
}

// SystemConfig.UnitTests/Domain/ValueObjects/ConnectionSettingsTests.cs
public class ConnectionSettingsTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateSettings()
    {
        // Arrange
        var server = "localhost";
        var database = "testdb";
        var username = "user";
        var password = "pass";
        
        // Act
        var settings = new ConnectionSettings(server, database, username, password);
        
        // Assert
        Assert.Equal(server, settings.Server);
        Assert.Equal(database, settings.Database);
        Assert.Equal(username, settings.Username);
        Assert.Equal(password, settings.Password);
        Assert.True(settings.IsValid());
    }
    
    [Theory]
    [InlineData("", "db", "user", "pass")]
    [InlineData("server", "", "user", "pass")]
    [InlineData("server", "db", "", "pass")]
    [InlineData("server", "db", "user", "")]
    public void Create_WithInvalidParameters_ShouldThrowException(string server, string database, string username, string password)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => 
            new ConnectionSettings(server, database, username, password));
    }
    
    [Fact]
    public void GetConnectionString_ShouldReturnValidConnectionString()
    {
        // Arrange
        var settings = new ConnectionSettings("localhost", "testdb", "user", "pass");
        
        // Act
        var connectionString = settings.GetConnectionString();
        
        // Assert
        Assert.NotNull(connectionString);
        Assert.Contains("localhost", connectionString);
        Assert.Contains("testdb", connectionString);
        Assert.Contains("user", connectionString);
        Assert.Contains("pass", connectionString);
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Domain/DatabaseConfigurationIntegrationTests.cs
public class DatabaseConfigurationIntegrationTests
{
    [Fact]
    public async Task DatabaseConfiguration_WithDomainEvents_ShouldPublishEvents()
    {
        // Arrange
        var eventHandler = new Mock<IDomainEventHandler<DatabaseConfigurationCreatedEvent>>();
        var configuration = DatabaseConfiguration.Create("Test", "Description", 
            new ConnectionSettings("localhost", "testdb", "user", "pass"), 
            DatabaseType.SqlServer, "testuser");
        
        // Act
        // Domain events should be published when configuration is created
        
        // Assert
        Assert.Single(configuration.DomainEvents);
        Assert.IsType<DatabaseConfigurationCreatedEvent>(configuration.DomainEvents.First());
    }
}
```

## 📊 Definition of Done
- [ ] **Entity Implementation**: DatabaseConfiguration aggregate root được implement đầy đủ
- [ ] **Value Objects**: ConnectionSettings, DatabaseConfigurationId được implement
- [ ] **Business Rules**: Tất cả business rules được implement
- [ ] **Domain Events**: Events được publish cho important changes
- [ ] **Validation**: Comprehensive validation cho tất cả inputs
- [ ] **Unit Tests**: >95% coverage cho domain entities
- [ ] **Integration Tests**: Domain events tests pass
- [ ] **Code Review**: Domain model được approve
- [ ] **Documentation**: Business rules documentation hoàn thành

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Complex business rules implementation
- **Mitigation**: Start với simple rules, add complexity gradually

- **Risk**: Value object serialization issues
- **Mitigation**: Use proper serialization attributes

- **Risk**: Domain event handling complexity
- **Mitigation**: Implement simple event publishing

### Domain Risks
- **Risk**: Business rules không đầy đủ
- **Mitigation**: Regular stakeholder review

- **Risk**: Validation logic không chính xác
- **Mitigation**: Comprehensive unit tests

## 📚 Resources & References
- Domain-Driven Design by Eric Evans
- Clean Architecture by Robert C. Martin
- .NET 8 Value Objects Best Practices
- Entity Framework Core Domain Modeling
- Domain Events Patterns

## 🔄 Dependencies
- Task 1.1.1: Create Solution Structure
- Task 1.1.2: Setup Dependency Injection
- Domain expert consultation

## 📈 Success Metrics
- Entity follows DDD principles
- All business rules enforced
- Value objects immutable
- Domain events published correctly
- High test coverage achieved
- Validation works properly

## 📝 Notes
- Focus on domain logic, not infrastructure concerns
- Use value objects for complex data
- Implement proper encapsulation
- Consider future extensibility
- Document business rules clearly
- Regular domain model reviews 