# Task 1.2.1: DatabaseConfiguration Entity

## 📋 Task Information

| **Attribute** | **Value** |
|---------------|-----------|
| **Task ID** | 1.2.1 |
| **Sprint** | Sprint 1 (Weeks 1-2) |
| **Story** | 1.2 - Core Domain Models |
| **Priority** | Critical |
| **Story Points** | 10 |
| **Estimated Hours** | 10 |
| **Assigned To** | Domain Expert |
| **Status** | Not Started |
| **Dependencies** | Task 1.1.1 (Create Solution Structure) |

## 🎯 Objective

Implement the DatabaseConfiguration domain entity with rich business logic, validation rules, connection management, health monitoring, and comprehensive audit capabilities following Domain-Driven Design principles.

## 📝 Detailed Requirements

### Functional Requirements

#### **Core Entity Properties**
- [ ] Unique identifier and metadata (ID, Name, Description, Created/Modified dates)
- [ ] Database connection parameters (Server, Database, Authentication)
- [ ] Database provider type (SQL Server, MySQL, PostgreSQL, Oracle)
- [ ] Connection pooling and performance settings
- [ ] Security configuration (Encryption, SSL, Certificates)
- [ ] Health monitoring settings (Status, Last Check, Retry Policies)
- [ ] Backup and recovery settings

#### **Business Rules Implementation**
- [ ] **Uniqueness**: Database configuration names must be unique
- [ ] **Default Configuration**: Only one configuration can be marked as default
- [ ] **Connection Validation**: All connections must be validated before saving
- [ ] **Security Requirements**: Sensitive data must be encrypted
- [ ] **Health Monitoring**: Automatic health checks with configurable intervals
- [ ] **Version Control**: Track configuration changes with versioning
- [ ] **Audit Trail**: Complete history of all modifications

#### **Domain Events**
- [ ] DatabaseConfigurationCreated
- [ ] DatabaseConfigurationUpdated
- [ ] DatabaseConfigurationDeleted
- [ ] DefaultConfigurationChanged
- [ ] ConnectionTestSucceeded/Failed
- [ ] HealthStatusChanged

### Technical Requirements

#### **Domain Entity Implementation**
```csharp
// Domain/Entities/DatabaseConfiguration.cs
public class DatabaseConfiguration : AggregateRoot<Guid>
{
    private readonly List<DatabaseConnectionHistory> _connectionHistory;
    private readonly List<DatabaseConfigurationVersion> _versions;

    public DatabaseConfiguration(
        string name,
        string description,
        DatabaseProvider provider,
        ConnectionSettings connectionSettings,
        string createdBy) : base(Guid.NewGuid())
    {
        Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Description = description ?? string.Empty;
        Provider = Guard.Against.Null(provider, nameof(provider));
        ConnectionSettings = Guard.Against.Null(connectionSettings, nameof(connectionSettings));
        CreatedBy = Guard.Against.NullOrWhiteSpace(createdBy, nameof(createdBy));
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
        Version = 1;
        IsDefault = false;
        IsActive = true;
        HealthStatus = DatabaseHealthStatus.Unknown;
        
        _connectionHistory = new List<DatabaseConnectionHistory>();
        _versions = new List<DatabaseConfigurationVersion>();
        
        // Add domain event
        AddDomainEvent(new DatabaseConfigurationCreatedEvent(this));
        
        // Create initial version
        CreateVersion("Initial configuration", createdBy);
    }

    // Properties
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DatabaseProvider Provider { get; private set; }
    public ConnectionSettings ConnectionSettings { get; private set; }
    public PerformanceSettings PerformanceSettings { get; private set; }
    public SecuritySettings SecuritySettings { get; private set; }
    public HealthMonitoringSettings HealthSettings { get; private set; }
    public BackupSettings BackupSettings { get; private set; }
    
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public DatabaseHealthStatus HealthStatus { get; private set; }
    public DateTime? LastHealthCheck { get; private set; }
    public string LastHealthMessage { get; private set; }
    
    public string CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string ModifiedBy { get; private set; }
    public DateTime ModifiedAt { get; private set; }
    public int Version { get; private set; }
    
    // Read-only collections
    public IReadOnlyList<DatabaseConnectionHistory> ConnectionHistory => _connectionHistory.AsReadOnly();
    public IReadOnlyList<DatabaseConfigurationVersion> Versions => _versions.AsReadOnly();

    // Business Methods
    public void UpdateBasicInfo(string name, string description, string modifiedBy)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));
        Guard.Against.NullOrWhiteSpace(modifiedBy, nameof(modifiedBy));
        
        if (Name != name || Description != description)
        {
            var oldName = Name;
            var oldDescription = Description;
            
            Name = name;
            Description = description;
            UpdateModificationInfo(modifiedBy);
            
            AddDomainEvent(new DatabaseConfigurationUpdatedEvent(this, nameof(UpdateBasicInfo), 
                new { OldName = oldName, NewName = name, OldDescription = oldDescription, NewDescription = description }));
        }
    }

    public void UpdateConnectionSettings(ConnectionSettings newSettings, string modifiedBy)
    {
        Guard.Against.Null(newSettings, nameof(newSettings));
        Guard.Against.NullOrWhiteSpace(modifiedBy, nameof(modifiedBy));
        
        if (!ConnectionSettings.Equals(newSettings))
        {
            var oldSettings = ConnectionSettings;
            ConnectionSettings = newSettings;
            UpdateModificationInfo(modifiedBy);
            
            // Reset health status when connection changes
            HealthStatus = DatabaseHealthStatus.Unknown;
            LastHealthCheck = null;
            LastHealthMessage = "Connection settings updated - health check required";
            
            AddDomainEvent(new DatabaseConfigurationUpdatedEvent(this, nameof(UpdateConnectionSettings), 
                new { OldSettings = oldSettings, NewSettings = newSettings }));
        }
    }

    public void UpdatePerformanceSettings(PerformanceSettings newSettings, string modifiedBy)
    {
        Guard.Against.Null(newSettings, nameof(newSettings));
        Guard.Against.NullOrWhiteSpace(modifiedBy, nameof(modifiedBy));
        
        if (!Equals(PerformanceSettings, newSettings))
        {
            var oldSettings = PerformanceSettings;
            PerformanceSettings = newSettings;
            UpdateModificationInfo(modifiedBy);
            
            AddDomainEvent(new DatabaseConfigurationUpdatedEvent(this, nameof(UpdatePerformanceSettings), 
                new { OldSettings = oldSettings, NewSettings = newSettings }));
        }
    }

    public void UpdateSecuritySettings(SecuritySettings newSettings, string modifiedBy)
    {
        Guard.Against.Null(newSettings, nameof(newSettings));
        Guard.Against.NullOrWhiteSpace(modifiedBy, nameof(modifiedBy));
        
        if (!Equals(SecuritySettings, newSettings))
        {
            var oldSettings = SecuritySettings;
            SecuritySettings = newSettings;
            UpdateModificationInfo(modifiedBy);
            
            AddDomainEvent(new DatabaseConfigurationUpdatedEvent(this, nameof(UpdateSecuritySettings), 
                new { OldSettings = oldSettings, NewSettings = newSettings }));
        }
    }

    public void SetAsDefault(string modifiedBy)
    {
        Guard.Against.NullOrWhiteSpace(modifiedBy, nameof(modifiedBy));
        
        if (!IsDefault)
        {
            IsDefault = true;
            UpdateModificationInfo(modifiedBy);
            
            AddDomainEvent(new DefaultConfigurationChangedEvent(this, modifiedBy));
        }
    }

    public void RemoveAsDefault(string modifiedBy)
    {
        Guard.Against.NullOrWhiteSpace(modifiedBy, nameof(modifiedBy));
        
        if (IsDefault)
        {
            IsDefault = false;
            UpdateModificationInfo(modifiedBy);
            
            AddDomainEvent(new DatabaseConfigurationUpdatedEvent(this, nameof(RemoveAsDefault), 
                new { PreviousDefault = true }));
        }
    }

    public void Activate(string modifiedBy)
    {
        Guard.Against.NullOrWhiteSpace(modifiedBy, nameof(modifiedBy));
        
        if (!IsActive)
        {
            IsActive = true;
            UpdateModificationInfo(modifiedBy);
            
            AddDomainEvent(new DatabaseConfigurationUpdatedEvent(this, nameof(Activate), 
                new { StatusChanged = "Activated" }));
        }
    }

    public void Deactivate(string modifiedBy)
    {
        Guard.Against.NullOrWhiteSpace(modifiedBy, nameof(modifiedBy));
        
        // Cannot deactivate default configuration
        if (IsDefault)
        {
            throw new BusinessRuleViolationException("Cannot deactivate the default database configuration");
        }
        
        if (IsActive)
        {
            IsActive = false;
            UpdateModificationInfo(modifiedBy);
            
            AddDomainEvent(new DatabaseConfigurationUpdatedEvent(this, nameof(Deactivate), 
                new { StatusChanged = "Deactivated" }));
        }
    }

    public ConnectionTestResult TestConnection()
    {
        var testResult = new ConnectionTestResult
        {
            ConfigurationId = Id,
            TestTime = DateTime.UtcNow,
            ConnectionString = ConnectionSettings.ToConnectionString(SecuritySettings.UseEncryption)
        };
        
        try
        {
            // This would be implemented by infrastructure layer
            // Domain layer defines the contract
            var connectionTester = DomainServices.GetService<IConnectionTester>();
            var result = connectionTester.TestConnection(ConnectionSettings, SecuritySettings);
            
            testResult.IsSuccessful = result.IsSuccessful;
            testResult.ResponseTimeMs = result.ResponseTimeMs;
            testResult.ErrorMessage = result.ErrorMessage;
            testResult.DatabaseVersion = result.DatabaseVersion;
            
            // Record the test in history
            _connectionHistory.Add(new DatabaseConnectionHistory(
                Id, testResult.IsSuccessful, testResult.ResponseTimeMs, 
                testResult.ErrorMessage, DateTime.UtcNow));
            
            // Update health status based on test result
            UpdateHealthStatus(
                testResult.IsSuccessful ? DatabaseHealthStatus.Healthy : DatabaseHealthStatus.Unhealthy,
                testResult.ErrorMessage ?? "Connection test completed successfully");
            
            // Add domain event
            if (testResult.IsSuccessful)
            {
                AddDomainEvent(new ConnectionTestSucceededEvent(this, testResult));
            }
            else
            {
                AddDomainEvent(new ConnectionTestFailedEvent(this, testResult));
            }
        }
        catch (Exception ex)
        {
            testResult.IsSuccessful = false;
            testResult.ErrorMessage = ex.Message;
            
            UpdateHealthStatus(DatabaseHealthStatus.Error, ex.Message);
            AddDomainEvent(new ConnectionTestFailedEvent(this, testResult));
        }
        
        return testResult;
    }

    public void UpdateHealthStatus(DatabaseHealthStatus status, string message)
    {
        if (HealthStatus != status)
        {
            var oldStatus = HealthStatus;
            HealthStatus = status;
            LastHealthCheck = DateTime.UtcNow;
            LastHealthMessage = message;
            
            AddDomainEvent(new HealthStatusChangedEvent(this, oldStatus, status, message));
        }
    }

    public void CreateVersion(string changeDescription, string createdBy)
    {
        Guard.Against.NullOrWhiteSpace(changeDescription, nameof(changeDescription));
        Guard.Against.NullOrWhiteSpace(createdBy, nameof(createdBy));
        
        var version = new DatabaseConfigurationVersion(
            Version, changeDescription, this.ToSnapshot(), createdBy, DateTime.UtcNow);
        
        _versions.Add(version);
        Version++;
    }

    public bool CanBeDeleted()
    {
        // Cannot delete default configuration
        if (IsDefault)
        {
            return false;
        }
        
        // Cannot delete if actively being used (this would be checked by domain service)
        return true;
    }

    public void MarkForDeletion(string deletedBy)
    {
        Guard.Against.NullOrWhiteSpace(deletedBy, nameof(deletedBy));
        
        if (!CanBeDeleted())
        {
            throw new BusinessRuleViolationException("This database configuration cannot be deleted");
        }
        
        IsActive = false;
        UpdateModificationInfo(deletedBy);
        
        AddDomainEvent(new DatabaseConfigurationDeletedEvent(this, deletedBy));
    }

    // Helper methods
    private void UpdateModificationInfo(string modifiedBy)
    {
        ModifiedBy = modifiedBy;
        ModifiedAt = DateTime.UtcNow;
    }

    private DatabaseConfigurationSnapshot ToSnapshot()
    {
        return new DatabaseConfigurationSnapshot
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Provider = Provider,
            ConnectionSettings = ConnectionSettings,
            PerformanceSettings = PerformanceSettings,
            SecuritySettings = SecuritySettings,
            HealthSettings = HealthSettings,
            BackupSettings = BackupSettings,
            IsDefault = IsDefault,
            IsActive = IsActive,
            Version = Version,
            SnapshotTime = DateTime.UtcNow
        };
    }

    // Validation methods
    public ValidationResult Validate()
    {
        var validator = new DatabaseConfigurationValidator();
        return validator.Validate(this);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
    }
}
```

#### **Value Objects Implementation**
```csharp
// Domain/ValueObjects/DatabaseProvider.cs
public class DatabaseProvider : ValueObject
{
    public static readonly DatabaseProvider SqlServer = new("SqlServer", "Microsoft SQL Server", "System.Data.SqlClient");
    public static readonly DatabaseProvider MySQL = new("MySQL", "MySQL Database", "MySql.Data.MySqlClient");
    public static readonly DatabaseProvider PostgreSQL = new("PostgreSQL", "PostgreSQL Database", "Npgsql");
    public static readonly DatabaseProvider Oracle = new("Oracle", "Oracle Database", "Oracle.ManagedDataAccess.Client");
    public static readonly DatabaseProvider SQLite = new("SQLite", "SQLite Database", "Microsoft.Data.Sqlite");

    public string Name { get; }
    public string DisplayName { get; }
    public string ProviderName { get; }

    private DatabaseProvider(string name, string displayName, string providerName)
    {
        Name = name;
        DisplayName = displayName;
        ProviderName = providerName;
    }

    public static DatabaseProvider FromName(string name)
    {
        return name switch
        {
            "SqlServer" => SqlServer,
            "MySQL" => MySQL,
            "PostgreSQL" => PostgreSQL,
            "Oracle" => Oracle,
            "SQLite" => SQLite,
            _ => throw new ArgumentException($"Unknown database provider: {name}")
        };
    }

    public static IEnumerable<DatabaseProvider> GetSupportedProviders()
    {
        yield return SqlServer;
        yield return MySQL;
        yield return PostgreSQL;
        yield return Oracle;
        yield return SQLite;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}

// Domain/ValueObjects/ConnectionSettings.cs
public class ConnectionSettings : ValueObject
{
    public string Server { get; }
    public string Database { get; }
    public string Username { get; }
    public string Password { get; }
    public int Port { get; }
    public AuthenticationType AuthenticationType { get; }
    public bool IntegratedSecurity { get; }
    public Dictionary<string, string> AdditionalParameters { get; }

    public ConnectionSettings(
        string server,
        string database,
        string username = null,
        string password = null,
        int port = 0,
        AuthenticationType authenticationType = AuthenticationType.SqlServer,
        bool integratedSecurity = false,
        Dictionary<string, string> additionalParameters = null)
    {
        Server = Guard.Against.NullOrWhiteSpace(server, nameof(server));
        Database = Guard.Against.NullOrWhiteSpace(database, nameof(database));
        Username = username;
        Password = password;
        Port = port;
        AuthenticationType = authenticationType;
        IntegratedSecurity = integratedSecurity;
        AdditionalParameters = additionalParameters ?? new Dictionary<string, string>();

        ValidateAuthenticationSettings();
    }

    public string ToConnectionString(bool encryptConnection = true)
    {
        var builder = new StringBuilder();

        // Server and database
        builder.Append($"Server={Server}");
        if (Port > 0) builder.Append($",{Port}");
        builder.Append($";Database={Database}");

        // Authentication
        if (IntegratedSecurity)
        {
            builder.Append(";Integrated Security=true");
        }
        else if (!string.IsNullOrEmpty(Username))
        {
            builder.Append($";User Id={Username}");
            if (!string.IsNullOrEmpty(Password))
            {
                builder.Append($";Password={Password}");
            }
        }

        // Security
        if (encryptConnection)
        {
            builder.Append(";Encrypt=true;TrustServerCertificate=false");
        }

        // Additional parameters
        foreach (var param in AdditionalParameters)
        {
            builder.Append($";{param.Key}={param.Value}");
        }

        return builder.ToString();
    }

    public ConnectionSettings WithEncryptedPassword(string encryptedPassword)
    {
        return new ConnectionSettings(
            Server, Database, Username, encryptedPassword, Port, 
            AuthenticationType, IntegratedSecurity, AdditionalParameters);
    }

    private void ValidateAuthenticationSettings()
    {
        if (!IntegratedSecurity && string.IsNullOrEmpty(Username))
        {
            throw new ArgumentException("Username is required when not using integrated security");
        }

        if (AuthenticationType == AuthenticationType.Windows && !IntegratedSecurity)
        {
            throw new ArgumentException("Windows authentication requires integrated security");
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Server;
        yield return Database;
        yield return Username;
        yield return Port;
        yield return AuthenticationType;
        yield return IntegratedSecurity;
        yield return string.Join(";", AdditionalParameters.Select(kv => $"{kv.Key}={kv.Value}"));
    }
}

// Domain/ValueObjects/PerformanceSettings.cs
public class PerformanceSettings : ValueObject
{
    public int MinPoolSize { get; }
    public int MaxPoolSize { get; }
    public TimeSpan ConnectionTimeout { get; }
    public TimeSpan CommandTimeout { get; }
    public bool EnableConnectionPooling { get; }
    public int ConnectionLifetime { get; }
    public bool EnableRetryOnFailure { get; }
    public int MaxRetryCount { get; }
    public TimeSpan MaxRetryDelay { get; }

    public PerformanceSettings(
        int minPoolSize = 5,
        int maxPoolSize = 100,
        TimeSpan? connectionTimeout = null,
        TimeSpan? commandTimeout = null,
        bool enableConnectionPooling = true,
        int connectionLifetime = 0,
        bool enableRetryOnFailure = true,
        int maxRetryCount = 3,
        TimeSpan? maxRetryDelay = null)
    {
        MinPoolSize = Guard.Against.NegativeOrZero(minPoolSize, nameof(minPoolSize));
        MaxPoolSize = Guard.Against.NegativeOrZero(maxPoolSize, nameof(maxPoolSize));
        ConnectionTimeout = connectionTimeout ?? TimeSpan.FromSeconds(30);
        CommandTimeout = commandTimeout ?? TimeSpan.FromSeconds(60);
        EnableConnectionPooling = enableConnectionPooling;
        ConnectionLifetime = Math.Max(0, connectionLifetime);
        EnableRetryOnFailure = enableRetryOnFailure;
        MaxRetryCount = Math.Max(0, maxRetryCount);
        MaxRetryDelay = maxRetryDelay ?? TimeSpan.FromSeconds(30);

        if (MinPoolSize > MaxPoolSize)
        {
            throw new ArgumentException("MinPoolSize cannot be greater than MaxPoolSize");
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MinPoolSize;
        yield return MaxPoolSize;
        yield return ConnectionTimeout;
        yield return CommandTimeout;
        yield return EnableConnectionPooling;
        yield return ConnectionLifetime;
        yield return EnableRetryOnFailure;
        yield return MaxRetryCount;
        yield return MaxRetryDelay;
    }
}
```

## 🧪 Testing Strategy

### Unit Tests

#### **Test 1: Entity Business Logic Tests**
```csharp
[TestFixture]
public class DatabaseConfigurationTests
{
    private DatabaseConfiguration _configuration;
    private const string TestUser = "testuser";

    [SetUp]
    public void Setup()
    {
        var connectionSettings = new ConnectionSettings("localhost", "testdb", "user", "password");
        _configuration = new DatabaseConfiguration(
            "Test Config", "Test Description", DatabaseProvider.SqlServer, 
            connectionSettings, TestUser);
    }

    [Test]
    public void CreateDatabaseConfiguration_ShouldSetInitialValues()
    {
        Assert.That(_configuration.Name, Is.EqualTo("Test Config"));
        Assert.That(_configuration.Description, Is.EqualTo("Test Description"));
        Assert.That(_configuration.Provider, Is.EqualTo(DatabaseProvider.SqlServer));
        Assert.That(_configuration.CreatedBy, Is.EqualTo(TestUser));
        Assert.That(_configuration.Version, Is.EqualTo(1));
        Assert.That(_configuration.IsDefault, Is.False);
        Assert.That(_configuration.IsActive, Is.True);
        Assert.That(_configuration.HealthStatus, Is.EqualTo(DatabaseHealthStatus.Unknown));
    }

    [Test]
    public void SetAsDefault_ShouldMarkAsDefault()
    {
        _configuration.SetAsDefault(TestUser);

        Assert.That(_configuration.IsDefault, Is.True);
        Assert.That(_configuration.ModifiedBy, Is.EqualTo(TestUser));
        Assert.That(_configuration.ModifiedAt, Is.GreaterThan(_configuration.CreatedAt));
    }

    [Test]
    public void Deactivate_WhenIsDefault_ShouldThrowException()
    {
        _configuration.SetAsDefault(TestUser);

        var ex = Assert.Throws<BusinessRuleViolationException>(() => 
            _configuration.Deactivate(TestUser));
        
        Assert.That(ex.Message, Does.Contain("Cannot deactivate the default database configuration"));
    }

    [Test]
    public void UpdateBasicInfo_ShouldUpdatePropertiesAndVersion()
    {
        var originalVersion = _configuration.Version;
        
        _configuration.UpdateBasicInfo("New Name", "New Description", TestUser);

        Assert.That(_configuration.Name, Is.EqualTo("New Name"));
        Assert.That(_configuration.Description, Is.EqualTo("New Description"));
        Assert.That(_configuration.ModifiedBy, Is.EqualTo(TestUser));
        Assert.That(_configuration.Version, Is.GreaterThan(originalVersion));
    }

    [Test]
    public void UpdateConnectionSettings_ShouldResetHealthStatus()
    {
        // First set a health status
        _configuration.UpdateHealthStatus(DatabaseHealthStatus.Healthy, "Test");
        
        var newSettings = new ConnectionSettings("newserver", "newdb", "newuser", "newpass");
        _configuration.UpdateConnectionSettings(newSettings, TestUser);

        Assert.That(_configuration.HealthStatus, Is.EqualTo(DatabaseHealthStatus.Unknown));
        Assert.That(_configuration.LastHealthMessage, Does.Contain("health check required"));
    }

    [Test]
    public void CanBeDeleted_WhenIsDefault_ShouldReturnFalse()
    {
        _configuration.SetAsDefault(TestUser);

        Assert.That(_configuration.CanBeDeleted(), Is.False);
    }

    [Test]
    public void CanBeDeleted_WhenNotDefault_ShouldReturnTrue()
    {
        Assert.That(_configuration.CanBeDeleted(), Is.True);
    }

    [Test]
    public void Validate_ShouldReturnValidationResult()
    {
        var result = _configuration.Validate();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsValid, Is.True);
    }
}
```

#### **Test 2: Value Object Tests**
```csharp
[TestFixture]
public class ConnectionSettingsTests
{
    [Test]
    public void CreateConnectionSettings_WithValidParameters_ShouldSucceed()
    {
        var settings = new ConnectionSettings("localhost", "testdb", "user", "password");

        Assert.That(settings.Server, Is.EqualTo("localhost"));
        Assert.That(settings.Database, Is.EqualTo("testdb"));
        Assert.That(settings.Username, Is.EqualTo("user"));
        Assert.That(settings.Password, Is.EqualTo("password"));
    }

    [Test]
    public void CreateConnectionSettings_WithNullServer_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => 
            new ConnectionSettings(null, "testdb", "user", "password"));
    }

    [Test]
    public void ToConnectionString_ShouldGenerateCorrectFormat()
    {
        var settings = new ConnectionSettings("localhost", "testdb", "user", "password", 1433);
        
        var connectionString = settings.ToConnectionString(true);

        Assert.That(connectionString, Does.Contain("Server=localhost,1433"));
        Assert.That(connectionString, Does.Contain("Database=testdb"));
        Assert.That(connectionString, Does.Contain("User Id=user"));
        Assert.That(connectionString, Does.Contain("Password=password"));
        Assert.That(connectionString, Does.Contain("Encrypt=true"));
    }

    [Test]
    public void ToConnectionString_WithIntegratedSecurity_ShouldNotIncludeCredentials()
    {
        var settings = new ConnectionSettings("localhost", "testdb", 
            integratedSecurity: true);
        
        var connectionString = settings.ToConnectionString();

        Assert.That(connectionString, Does.Contain("Integrated Security=true"));
        Assert.That(connectionString, Does.Not.Contain("User Id"));
        Assert.That(connectionString, Does.Not.Contain("Password"));
    }

    [Test]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        var settings1 = new ConnectionSettings("localhost", "testdb", "user", "password");
        var settings2 = new ConnectionSettings("localhost", "testdb", "user", "password");

        Assert.That(settings1, Is.EqualTo(settings2));
        Assert.That(settings1.GetHashCode(), Is.EqualTo(settings2.GetHashCode()));
    }
}

[TestFixture]
public class DatabaseProviderTests
{
    [Test]
    public void GetSupportedProviders_ShouldReturnAllProviders()
    {
        var providers = DatabaseProvider.GetSupportedProviders().ToList();

        Assert.That(providers.Count, Is.GreaterThan(0));
        Assert.That(providers.Any(p => p.Name == "SqlServer"), Is.True);
        Assert.That(providers.Any(p => p.Name == "MySQL"), Is.True);
        Assert.That(providers.Any(p => p.Name == "PostgreSQL"), Is.True);
    }

    [Test]
    public void FromName_WithValidName_ShouldReturnCorrectProvider()
    {
        var provider = DatabaseProvider.FromName("SqlServer");

        Assert.That(provider, Is.EqualTo(DatabaseProvider.SqlServer));
        Assert.That(provider.DisplayName, Is.EqualTo("Microsoft SQL Server"));
    }

    [Test]
    public void FromName_WithInvalidName_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() => 
            DatabaseProvider.FromName("InvalidProvider"));
    }
}
```

## 📊 Acceptance Criteria

### Primary Acceptance Criteria
- [ ] DatabaseConfiguration entity with complete business logic implemented
- [ ] All value objects (ConnectionSettings, PerformanceSettings, SecuritySettings) created
- [ ] Business rules enforced (uniqueness, default configuration, validation)
- [ ] Domain events raised for all significant operations
- [ ] Connection testing capability with history tracking
- [ ] Health monitoring with status tracking
- [ ] Version control with change tracking
- [ ] Comprehensive validation with business rule checking

### Quality Acceptance Criteria
- [ ] All business rules properly implemented and tested
- [ ] Value objects are immutable and properly validated
- [ ] Domain events contain relevant information
- [ ] Entity supports audit trail requirements
- [ ] Performance optimized for common operations
- [ ] Memory usage optimized with proper encapsulation

### Technical Acceptance Criteria
- [ ] Follows DDD principles and Clean Architecture
- [ ] Proper separation of concerns
- [ ] Thread-safe operations where applicable
- [ ] Comprehensive unit test coverage >95%
- [ ] Integration with repository pattern ready
- [ ] Serialization/deserialization support

## 🚨 Risk Management

### Technical Risks
| **Risk** | **Probability** | **Impact** | **Mitigation** |
|----------|----------------|------------|----------------|
| Complex business rules | Medium | Medium | Comprehensive testing, clear documentation |
| Performance issues | Low | Medium | Optimized algorithms, performance testing |
| Thread safety issues | Low | High | Immutable value objects, careful state management |
| Validation complexity | Medium | Low | Well-defined validation rules, unit tests |

### Business Risks
| **Risk** | **Probability** | **Impact** | **Mitigation** |
|----------|----------------|------------|----------------|
| Business rule violations | Medium | High | Comprehensive validation, domain events |
| Data consistency issues | Low | High | Transaction boundaries, unit of work pattern |
| Audit trail gaps | Low | Medium | Complete event tracking, version control |

## 📚 Resources & References

### Technical Documentation
- [Domain-Driven Design Patterns](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Validation in .NET](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)

### Domain Design References
- Domain-Driven Design by Eric Evans
- Implementing Domain-Driven Design by Vaughn Vernon
- Clean Architecture patterns and practices

## 📈 Success Metrics

### Completion Metrics
- [ ] DatabaseConfiguration entity fully implemented with all business methods
- [ ] All value objects created and properly validated
- [ ] Domain events implemented for all business operations
- [ ] Business rules enforced with proper exception handling
- [ ] Version control and audit trail functional

### Quality Metrics
- [ ] Unit test coverage >95% for all entity logic
- [ ] All business scenarios covered by tests
- [ ] Performance benchmarks met for entity operations
- [ ] Memory usage optimized for typical workloads
- [ ] Code review passed with architecture compliance

### Business Metrics
- [ ] All business rules properly implemented
- [ ] Domain events capture all required information
- [ ] Audit trail provides complete change history
- [ ] Validation catches all invalid scenarios

## 📝 Definition of Done

### Code Complete
- [ ] DatabaseConfiguration entity with all business methods implemented
- [ ] Value objects (ConnectionSettings, PerformanceSettings, SecuritySettings, etc.)
- [ ] Domain events for all business operations
- [ ] Business rule validation throughout
- [ ] Version control and change tracking
- [ ] Connection testing with history
- [ ] Health monitoring capabilities

### Quality Complete
- [ ] Unit tests with >95% coverage for entity and value objects
- [ ] Integration tests for business workflows
- [ ] Performance tests for entity operations
- [ ] Business rule violation tests
- [ ] Domain event testing
- [ ] Code review completed and approved

### Documentation Complete
- [ ] Entity business rules documented
- [ ] Value object usage examples
- [ ] Domain events catalog
- [ ] Business workflow documentation
- [ ] API documentation for public methods

---

**Note**: This DatabaseConfiguration entity serves as the cornerstone of the domain model, encapsulating all business logic related to database configuration management. Ensure all business rules are properly implemented and tested before proceeding with repository implementation.