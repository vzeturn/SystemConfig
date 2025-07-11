# Task 1.2.3: System Configuration Entity

## 📋 Task Overview
**Sprint**: 1  
**Story**: 1.2 - Domain Models Development  
**Priority**: High  
**Estimated Hours**: 8  
**Assigned To**: Domain Expert  
**Dependencies**: Task 1.1.1 - Create Solution Structure, Task 1.1.2 - Setup Dependency Injection

## 🎯 Objective
Implement SystemConfiguration aggregate root với business rules, validation logic, và domain events theo Domain-Driven Design principles cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **SystemConfiguration Aggregate Root**:
  - Unique identifier với SystemConfigurationId
  - Name và description fields
  - Configuration schema với type-safe values
  - Validation rules cho configuration values
  - Version control cho configuration changes
  - Access control (ReadOnly, ReadWrite, Admin)
  - Configuration categories (System, User, Application)
  - Creation và modification timestamps

- [ ] **Business Rules Implementation**:
  - Required configurations không được delete
  - Read-only configurations chỉ admin mới edit được
  - Validation rules phải pass trước khi save
  - Configuration changes trigger notifications
  - Version history tracking

- [ ] **Value Objects**:
  - ConfigurationSchema với type definitions
  - SystemConfigurationId với uniqueness
  - ConfigurationValue với type safety
  - ValidationRule với custom rules
  - AccessLevel enum

### Technical Requirements
- [ ] **Entity Structure**:
  ```csharp
  // SystemConfig.Domain/Entities/SystemConfiguration.cs
  public class SystemConfiguration : AggregateRoot
  {
      public SystemConfigurationId Id { get; private set; }
      public string Name { get; private set; }
      public string Description { get; private set; }
      public ConfigurationSchema Schema { get; private set; }
      public Dictionary<string, ConfigurationValue> Values { get; private set; }
      public List<ValidationRule> ValidationRules { get; private set; }
      public AccessLevel AccessLevel { get; private set; }
      public ConfigurationCategory Category { get; private set; }
      public int Version { get; private set; }
      public DateTime CreatedAt { get; private set; }
      public DateTime? LastModifiedAt { get; private set; }
      public string CreatedBy { get; private set; }
      public string LastModifiedBy { get; private set; }
      
      // Business methods
      public void UpdateValue(string key, ConfigurationValue value, string modifiedBy)
      public void UpdateSchema(ConfigurationSchema schema, string modifiedBy)
      public void AddValidationRule(ValidationRule rule, string modifiedBy)
      public void RemoveValidationRule(string ruleName, string modifiedBy)
      public void ValidateConfiguration()
      public void IncrementVersion(string modifiedBy)
      public void SetAccessLevel(AccessLevel accessLevel, string modifiedBy)
  }
  ```

- [ ] **Value Objects**:
  ```csharp
  // SystemConfig.Domain/ValueObjects/SystemConfigurationId.cs
  public class SystemConfigurationId : ValueObject
  {
      public Guid Value { get; }
      
      public SystemConfigurationId(Guid value)
      {
          if (value == Guid.Empty)
              throw new DomainException("System configuration ID cannot be empty");
          
          Value = value;
      }
      
      public static SystemConfigurationId New() => new SystemConfigurationId(Guid.NewGuid());
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Value;
      }
      
      public static implicit operator Guid(SystemConfigurationId id) => id.Value;
      public static implicit operator SystemConfigurationId(Guid value) => new SystemConfigurationId(value);
  }
  
  // SystemConfig.Domain/ValueObjects/ConfigurationSchema.cs
  public class ConfigurationSchema : ValueObject
  {
      public Dictionary<string, ConfigurationFieldDefinition> Fields { get; }
      public string Version { get; }
      public DateTime CreatedAt { get; }
      
      public ConfigurationSchema(Dictionary<string, ConfigurationFieldDefinition> fields, string version)
      {
          if (fields == null || !fields.Any())
              throw new DomainException("Schema must have at least one field");
          
          if (string.IsNullOrWhiteSpace(version))
              throw new DomainException("Schema version cannot be empty");
          
          Fields = fields;
          Version = version;
          CreatedAt = DateTime.UtcNow;
      }
      
      public bool IsValid()
      {
          return Fields != null && Fields.Any() && !string.IsNullOrWhiteSpace(Version);
      }
      
      public ConfigurationFieldDefinition GetFieldDefinition(string fieldName)
      {
          return Fields.TryGetValue(fieldName, out var definition) ? definition : null;
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Version;
          yield return CreatedAt;
          foreach (var field in Fields.OrderBy(f => f.Key))
          {
              yield return field.Key;
              yield return field.Value;
          }
      }
  }
  
  // SystemConfig.Domain/ValueObjects/ConfigurationValue.cs
  public class ConfigurationValue : ValueObject
  {
      public object Value { get; }
      public ConfigurationValueType Type { get; }
      public DateTime CreatedAt { get; }
      
      public ConfigurationValue(object value, ConfigurationValueType type)
      {
          if (value == null)
              throw new DomainException("Configuration value cannot be null");
          
          Value = value;
          Type = type;
          CreatedAt = DateTime.UtcNow;
      }
      
      public T GetValue<T>()
      {
          if (Value is T typedValue)
              return typedValue;
          
          throw new DomainException($"Cannot convert value of type {Value.GetType()} to {typeof(T)}");
      }
      
      public bool IsValidForType(ConfigurationValueType expectedType)
      {
          return Type == expectedType;
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Value;
          yield return Type;
          yield return CreatedAt;
      }
  }
  
  // SystemConfig.Domain/ValueObjects/ValidationRule.cs
  public class ValidationRule : ValueObject
  {
      public string Name { get; }
      public string Expression { get; }
      public string ErrorMessage { get; }
      public ValidationRuleType Type { get; }
      public DateTime CreatedAt { get; }
      
      public ValidationRule(string name, string expression, string errorMessage, ValidationRuleType type)
      {
          if (string.IsNullOrWhiteSpace(name))
              throw new DomainException("Validation rule name cannot be empty");
          
          if (string.IsNullOrWhiteSpace(expression))
              throw new DomainException("Validation rule expression cannot be empty");
          
          if (string.IsNullOrWhiteSpace(errorMessage))
              throw new DomainException("Validation rule error message cannot be empty");
          
          Name = name;
          Expression = expression;
          ErrorMessage = errorMessage;
          Type = type;
          CreatedAt = DateTime.UtcNow;
      }
      
      public bool IsValid()
      {
          return !string.IsNullOrWhiteSpace(Name) &&
                 !string.IsNullOrWhiteSpace(Expression) &&
                 !string.IsNullOrWhiteSpace(ErrorMessage);
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Name;
          yield return Expression;
          yield return ErrorMessage;
          yield return Type;
          yield return CreatedAt;
      }
  }
  ```

- [ ] **Enums**:
  ```csharp
  // SystemConfig.Domain/Enums/AccessLevel.cs
  public enum AccessLevel
  {
      ReadOnly,
      ReadWrite,
      Admin
  }
  
  // SystemConfig.Domain/Enums/ConfigurationCategory.cs
  public enum ConfigurationCategory
  {
      System,
      User,
      Application,
      Security,
      Performance
  }
  
  // SystemConfig.Domain/Enums/ConfigurationValueType.cs
  public enum ConfigurationValueType
  {
      String,
      Integer,
      Decimal,
      Boolean,
      DateTime,
      Json
  }
  
  // SystemConfig.Domain/Enums/ValidationRuleType.cs
  public enum ValidationRuleType
  {
      Required,
      Range,
      Format,
      Custom
  }
  ```

### Quality Requirements
- [ ] **Domain Logic**: Business rules được implement đúng
- [ ] **Validation**: Comprehensive validation cho tất cả inputs
- [ ] **Immutability**: Value objects immutable
- [ ] **Domain Events**: Events được publish cho important changes
- [ ] **Testability**: Easy to test và mock

## 🏗️ Implementation Plan

### Phase 1: Core Entity Implementation (3 hours)
```csharp
// SystemConfig.Domain/Entities/SystemConfiguration.cs
public class SystemConfiguration : AggregateRoot
{
    public SystemConfigurationId Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public ConfigurationSchema Schema { get; private set; }
    public Dictionary<string, ConfigurationValue> Values { get; private set; }
    public List<ValidationRule> ValidationRules { get; private set; }
    public AccessLevel AccessLevel { get; private set; }
    public ConfigurationCategory Category { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public string CreatedBy { get; private set; }
    public string LastModifiedBy { get; private set; }
    
    // Private constructor for EF Core
    private SystemConfiguration()
    {
        Values = new Dictionary<string, ConfigurationValue>();
        ValidationRules = new List<ValidationRule>();
    }
    
    // Factory method
    public static SystemConfiguration Create(
        string name,
        string description,
        ConfigurationSchema schema,
        ConfigurationCategory category,
        AccessLevel accessLevel,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new DomainException("Created by cannot be empty");
        
        var configuration = new SystemConfiguration
        {
            Id = SystemConfigurationId.New(),
            Name = name,
            Description = description ?? string.Empty,
            Schema = schema ?? throw new DomainException("Schema cannot be null"),
            Category = category,
            AccessLevel = accessLevel,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
        
        configuration.AddDomainEvent(new SystemConfigurationCreatedEvent(configuration.Id, configuration.Name));
        
        return configuration;
    }
    
    public void UpdateValue(string key, ConfigurationValue value, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("Configuration key cannot be empty");
        
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        if (AccessLevel == AccessLevel.ReadOnly)
            throw new DomainException("Cannot update read-only configuration");
        
        // Validate value against schema
        var fieldDefinition = Schema.GetFieldDefinition(key);
        if (fieldDefinition != null && !value.IsValidForType(fieldDefinition.Type))
        {
            throw new DomainException($"Value type {value.Type} does not match schema type {fieldDefinition.Type}");
        }
        
        var oldValue = Values.ContainsKey(key) ? Values[key] : null;
        Values[key] = value;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new SystemConfigurationValueUpdatedEvent(Id, key, oldValue, value));
    }
    
    public void UpdateSchema(ConfigurationSchema schema, string modifiedBy)
    {
        if (schema == null)
            throw new DomainException("Schema cannot be null");
        
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        if (AccessLevel == AccessLevel.ReadOnly)
            throw new DomainException("Cannot update read-only configuration");
        
        var oldSchema = Schema;
        Schema = schema;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new SystemConfigurationSchemaUpdatedEvent(Id, oldSchema, schema));
    }
    
    public void AddValidationRule(ValidationRule rule, string modifiedBy)
    {
        if (rule == null)
            throw new DomainException("Validation rule cannot be null");
        
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        if (AccessLevel == AccessLevel.ReadOnly)
            throw new DomainException("Cannot update read-only configuration");
        
        if (ValidationRules.Any(r => r.Name == rule.Name))
            throw new DomainException($"Validation rule '{rule.Name}' already exists");
        
        ValidationRules.Add(rule);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new SystemConfigurationValidationRuleAddedEvent(Id, rule));
    }
    
    public void RemoveValidationRule(string ruleName, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(ruleName))
            throw new DomainException("Rule name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        if (AccessLevel == AccessLevel.ReadOnly)
            throw new DomainException("Cannot update read-only configuration");
        
        var rule = ValidationRules.FirstOrDefault(r => r.Name == ruleName);
        if (rule == null)
            throw new DomainException($"Validation rule '{ruleName}' not found");
        
        ValidationRules.Remove(rule);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new SystemConfigurationValidationRuleRemovedEvent(Id, rule));
    }
    
    public void ValidateConfiguration()
    {
        var errors = new List<string>();
        
        // Validate required fields
        foreach (var field in Schema.Fields.Where(f => f.Value.IsRequired))
        {
            if (!Values.ContainsKey(field.Key))
            {
                errors.Add($"Required field '{field.Key}' is missing");
            }
        }
        
        // Validate values against schema
        foreach (var value in Values)
        {
            var fieldDefinition = Schema.GetFieldDefinition(value.Key);
            if (fieldDefinition != null && !value.Value.IsValidForType(fieldDefinition.Type))
            {
                errors.Add($"Value for field '{value.Key}' has invalid type");
            }
        }
        
        // Run validation rules
        foreach (var rule in ValidationRules)
        {
            // Custom validation logic here
            if (!ValidateRule(rule))
            {
                errors.Add(rule.ErrorMessage);
            }
        }
        
        if (errors.Any())
        {
            throw new DomainException($"Configuration validation failed: {string.Join(", ", errors)}");
        }
    }
    
    public void IncrementVersion(string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        Version++;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new SystemConfigurationVersionIncrementedEvent(Id, Version));
    }
    
    public void SetAccessLevel(AccessLevel accessLevel, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        var oldAccessLevel = AccessLevel;
        AccessLevel = accessLevel;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new SystemConfigurationAccessLevelChangedEvent(Id, oldAccessLevel, accessLevel));
    }
    
    private bool ValidateRule(ValidationRule rule)
    {
        // Simple validation logic - can be extended
        switch (rule.Type)
        {
            case ValidationRuleType.Required:
                return Values.Any();
            case ValidationRuleType.Range:
                // Implement range validation
                return true;
            case ValidationRuleType.Format:
                // Implement format validation
                return true;
            case ValidationRuleType.Custom:
                // Implement custom validation
                return true;
            default:
                return true;
        }
    }
}
```

### Phase 2: Domain Events Implementation (2 hours)
```csharp
// SystemConfig.Domain/Events/SystemConfigurationEvents.cs
public class SystemConfigurationCreatedEvent : DomainEvent
{
    public SystemConfigurationId ConfigurationId { get; }
    public string Name { get; }
    
    public SystemConfigurationCreatedEvent(SystemConfigurationId configurationId, string name)
    {
        ConfigurationId = configurationId;
        Name = name;
    }
}

public class SystemConfigurationValueUpdatedEvent : DomainEvent
{
    public SystemConfigurationId ConfigurationId { get; }
    public string Key { get; }
    public ConfigurationValue OldValue { get; }
    public ConfigurationValue NewValue { get; }
    
    public SystemConfigurationValueUpdatedEvent(
        SystemConfigurationId configurationId,
        string key,
        ConfigurationValue oldValue,
        ConfigurationValue newValue)
    {
        ConfigurationId = configurationId;
        Key = key;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

public class SystemConfigurationSchemaUpdatedEvent : DomainEvent
{
    public SystemConfigurationId ConfigurationId { get; }
    public ConfigurationSchema OldSchema { get; }
    public ConfigurationSchema NewSchema { get; }
    
    public SystemConfigurationSchemaUpdatedEvent(
        SystemConfigurationId configurationId,
        ConfigurationSchema oldSchema,
        ConfigurationSchema newSchema)
    {
        ConfigurationId = configurationId;
        OldSchema = oldSchema;
        NewSchema = newSchema;
    }
}

public class SystemConfigurationValidationRuleAddedEvent : DomainEvent
{
    public SystemConfigurationId ConfigurationId { get; }
    public ValidationRule Rule { get; }
    
    public SystemConfigurationValidationRuleAddedEvent(SystemConfigurationId configurationId, ValidationRule rule)
    {
        ConfigurationId = configurationId;
        Rule = rule;
    }
}

public class SystemConfigurationVersionIncrementedEvent : DomainEvent
{
    public SystemConfigurationId ConfigurationId { get; }
    public int NewVersion { get; }
    
    public SystemConfigurationVersionIncrementedEvent(SystemConfigurationId configurationId, int newVersion)
    {
        ConfigurationId = configurationId;
        NewVersion = newVersion;
    }
}
```

### Phase 3: Business Rules & Validation (3 hours)
```csharp
// SystemConfig.Domain/Services/ISystemConfigurationValidator.cs
public interface ISystemConfigurationValidator
{
    Task<ValidationResult> ValidateAsync(SystemConfiguration configuration);
    Task<ValidationResult> ValidateValueAsync(string key, ConfigurationValue value, ConfigurationSchema schema);
    Task<ValidationResult> ValidateSchemaAsync(ConfigurationSchema schema);
    Task<ValidationResult> ValidateAccessLevelAsync(SystemConfiguration configuration, string userId, AccessLevel requiredLevel);
}

// SystemConfig.Domain/Services/SystemConfigurationValidator.cs
public class SystemConfigurationValidator : ISystemConfigurationValidator
{
    public async Task<ValidationResult> ValidateAsync(SystemConfiguration configuration)
    {
        var errors = new List<ValidationError>();
        
        // Validate name
        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            errors.Add(new ValidationError("Name", "Name cannot be empty"));
        }
        
        // Validate schema
        if (configuration.Schema == null)
        {
            errors.Add(new ValidationError("Schema", "Schema cannot be null"));
        }
        else if (!configuration.Schema.IsValid())
        {
            errors.Add(new ValidationError("Schema", "Schema is invalid"));
        }
        
        // Validate values against schema
        foreach (var value in configuration.Values)
        {
            var fieldDefinition = configuration.Schema.GetFieldDefinition(value.Key);
            if (fieldDefinition != null && !value.Value.IsValidForType(fieldDefinition.Type))
            {
                errors.Add(new ValidationError(value.Key, $"Value type does not match schema type"));
            }
        }
        
        // Validate required fields
        foreach (var field in configuration.Schema.Fields.Where(f => f.Value.IsRequired))
        {
            if (!configuration.Values.ContainsKey(field.Key))
            {
                errors.Add(new ValidationError(field.Key, "Required field is missing"));
            }
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors) 
            : ValidationResult.Success();
    }
    
    public async Task<ValidationResult> ValidateValueAsync(string key, ConfigurationValue value, ConfigurationSchema schema)
    {
        var errors = new List<ValidationError>();
        
        if (string.IsNullOrWhiteSpace(key))
        {
            errors.Add(new ValidationError("Key", "Configuration key cannot be empty"));
        }
        
        if (value == null)
        {
            errors.Add(new ValidationError("Value", "Configuration value cannot be null"));
        }
        else
        {
            var fieldDefinition = schema.GetFieldDefinition(key);
            if (fieldDefinition != null && !value.IsValidForType(fieldDefinition.Type))
            {
                errors.Add(new ValidationError("Value", $"Value type {value.Type} does not match schema type {fieldDefinition.Type}"));
            }
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors) 
            : ValidationResult.Success();
    }
    
    public async Task<ValidationResult> ValidateSchemaAsync(ConfigurationSchema schema)
    {
        var errors = new List<ValidationError>();
        
        if (schema == null)
        {
            errors.Add(new ValidationError("Schema", "Schema cannot be null"));
        }
        else if (!schema.IsValid())
        {
            errors.Add(new ValidationError("Schema", "Schema is invalid"));
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors) 
            : ValidationResult.Success();
    }
    
    public async Task<ValidationResult> ValidateAccessLevelAsync(SystemConfiguration configuration, string userId, AccessLevel requiredLevel)
    {
        var errors = new List<ValidationError>();
        
        if (string.IsNullOrWhiteSpace(userId))
        {
            errors.Add(new ValidationError("UserId", "User ID cannot be empty"));
        }
        
        if (configuration.AccessLevel < requiredLevel)
        {
            errors.Add(new ValidationError("AccessLevel", $"Insufficient access level. Required: {requiredLevel}, Current: {configuration.AccessLevel}"));
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
// SystemConfig.UnitTests/Domain/SystemConfigurationTests.cs
public class SystemConfigurationTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateConfiguration()
    {
        // Arrange
        var name = "Test Configuration";
        var description = "Test description";
        var schema = CreateTestSchema();
        var category = ConfigurationCategory.System;
        var accessLevel = AccessLevel.ReadWrite;
        var createdBy = "testuser";
        
        // Act
        var configuration = SystemConfiguration.Create(name, description, schema, category, accessLevel, createdBy);
        
        // Assert
        Assert.NotNull(configuration);
        Assert.Equal(name, configuration.Name);
        Assert.Equal(description, configuration.Description);
        Assert.Equal(schema, configuration.Schema);
        Assert.Equal(category, configuration.Category);
        Assert.Equal(accessLevel, configuration.AccessLevel);
        Assert.Equal(1, configuration.Version);
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
        var schema = CreateTestSchema();
        var category = ConfigurationCategory.System;
        var accessLevel = AccessLevel.ReadWrite;
        
        // Act & Assert
        Assert.Throws<DomainException>(() => 
            SystemConfiguration.Create(name, description, schema, category, accessLevel, createdBy));
    }
    
    [Fact]
    public void UpdateValue_WithValidValue_ShouldUpdateValue()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var key = "testKey";
        var value = new ConfigurationValue("testValue", ConfigurationValueType.String);
        var modifiedBy = "testuser";
        
        // Act
        configuration.UpdateValue(key, value, modifiedBy);
        
        // Assert
        Assert.Equal(value, configuration.Values[key]);
        Assert.Equal(modifiedBy, configuration.LastModifiedBy);
        Assert.True(configuration.LastModifiedAt > DateTime.UtcNow.AddMinutes(-1));
    }
    
    [Fact]
    public void UpdateValue_WithReadOnlyAccess_ShouldThrowException()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        configuration.SetAccessLevel(AccessLevel.ReadOnly, "admin");
        var key = "testKey";
        var value = new ConfigurationValue("testValue", ConfigurationValueType.String);
        
        // Act & Assert
        Assert.Throws<DomainException>(() => configuration.UpdateValue(key, value, "user"));
    }
    
    [Fact]
    public void AddValidationRule_WithValidRule_ShouldAddRule()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var rule = new ValidationRule("TestRule", "expression", "error message", ValidationRuleType.Required);
        var modifiedBy = "testuser";
        
        // Act
        configuration.AddValidationRule(rule, modifiedBy);
        
        // Assert
        Assert.Contains(rule, configuration.ValidationRules);
        Assert.Equal(modifiedBy, configuration.LastModifiedBy);
    }
    
    [Fact]
    public void IncrementVersion_ShouldIncrementVersion()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var modifiedBy = "testuser";
        var originalVersion = configuration.Version;
        
        // Act
        configuration.IncrementVersion(modifiedBy);
        
        // Assert
        Assert.Equal(originalVersion + 1, configuration.Version);
        Assert.Equal(modifiedBy, configuration.LastModifiedBy);
    }
    
    private SystemConfiguration CreateTestConfiguration()
    {
        var schema = CreateTestSchema();
        return SystemConfiguration.Create("Test", "Description", schema, ConfigurationCategory.System, AccessLevel.ReadWrite, "testuser");
    }
    
    private ConfigurationSchema CreateTestSchema()
    {
        var fields = new Dictionary<string, ConfigurationFieldDefinition>
        {
            ["testKey"] = new ConfigurationFieldDefinition("testKey", ConfigurationValueType.String, true)
        };
        return new ConfigurationSchema(fields, "1.0");
    }
}

// SystemConfig.UnitTests/Domain/ValueObjects/ConfigurationValueTests.cs
public class ConfigurationValueTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateValue()
    {
        // Arrange
        var value = "testValue";
        var type = ConfigurationValueType.String;
        
        // Act
        var configValue = new ConfigurationValue(value, type);
        
        // Assert
        Assert.Equal(value, configValue.Value);
        Assert.Equal(type, configValue.Type);
        Assert.True(configValue.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
    }
    
    [Fact]
    public void Create_WithNullValue_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new ConfigurationValue(null, ConfigurationValueType.String));
    }
    
    [Fact]
    public void GetValue_WithCorrectType_ShouldReturnValue()
    {
        // Arrange
        var configValue = new ConfigurationValue("testValue", ConfigurationValueType.String);
        
        // Act
        var result = configValue.GetValue<string>();
        
        // Assert
        Assert.Equal("testValue", result);
    }
    
    [Fact]
    public void GetValue_WithIncorrectType_ShouldThrowException()
    {
        // Arrange
        var configValue = new ConfigurationValue("testValue", ConfigurationValueType.String);
        
        // Act & Assert
        Assert.Throws<DomainException>(() => configValue.GetValue<int>());
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Domain/SystemConfigurationIntegrationTests.cs
public class SystemConfigurationIntegrationTests
{
    [Fact]
    public async Task SystemConfiguration_WithDomainEvents_ShouldPublishEvents()
    {
        // Arrange
        var schema = CreateTestSchema();
        var configuration = SystemConfiguration.Create("Test", "Description", schema, ConfigurationCategory.System, AccessLevel.ReadWrite, "testuser");
        
        // Act
        // Domain events should be published when configuration is created
        
        // Assert
        Assert.Single(configuration.DomainEvents);
        Assert.IsType<SystemConfigurationCreatedEvent>(configuration.DomainEvents.First());
    }
    
    [Fact]
    public async Task SystemConfiguration_WithValueUpdate_ShouldPublishEvents()
    {
        // Arrange
        var schema = CreateTestSchema();
        var configuration = SystemConfiguration.Create("Test", "Description", schema, ConfigurationCategory.System, AccessLevel.ReadWrite, "testuser");
        
        // Act
        configuration.UpdateValue("testKey", new ConfigurationValue("testValue", ConfigurationValueType.String), "testuser");
        
        // Assert
        Assert.Contains(configuration.DomainEvents, e => e is SystemConfigurationValueUpdatedEvent);
    }
    
    private ConfigurationSchema CreateTestSchema()
    {
        var fields = new Dictionary<string, ConfigurationFieldDefinition>
        {
            ["testKey"] = new ConfigurationFieldDefinition("testKey", ConfigurationValueType.String, true)
        };
        return new ConfigurationSchema(fields, "1.0");
    }
}
```

## 📊 Definition of Done
- [ ] **Entity Implementation**: SystemConfiguration aggregate root được implement đầy đủ
- [ ] **Value Objects**: ConfigurationSchema, ConfigurationValue, ValidationRule được implement
- [ ] **Business Rules**: Tất cả business rules được implement
- [ ] **Domain Events**: Events được publish cho important changes
- [ ] **Validation**: Comprehensive validation cho tất cả inputs
- [ ] **Unit Tests**: >95% coverage cho domain entities
- [ ] **Integration Tests**: Domain events tests pass
- [ ] **Code Review**: Domain model được approve
- [ ] **Documentation**: Business rules documentation hoàn thành

## 🚨 Risks & Mitigation

### Technical Risks
- [ ] **Risk**: Complex schema validation logic
- [ ] **Mitigation**: Start với simple validation, add complexity gradually

- [ ] **Risk**: Type safety issues với dynamic values
- [ ] **Mitigation**: Use strong typing và validation

- [ ] **Risk**: Version control complexity
- [ ] **Mitigation**: Implement simple versioning first

### Domain Risks
- [ ] **Risk**: Configuration schema evolution
- [ ] **Mitigation**: Design for extensibility

- [ ] **Risk**: Access control complexity
- [ ] **Mitigation**: Start với simple access levels

## 📚 Resources & References
- Domain-Driven Design by Eric Evans
- Clean Architecture by Robert C. Martin
- .NET 8 Value Objects Best Practices
- Configuration Management Patterns
- Schema Evolution Best Practices

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
- Focus on domain logic, not UI concerns
- Use value objects for complex configuration data
- Implement proper encapsulation
- Consider future extensibility
- Document business rules clearly
- Regular domain model reviews 