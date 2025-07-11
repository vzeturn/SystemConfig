# Task 1.2.2: Printer Configuration Entity

## 📋 Task Overview
**Sprint**: 1  
**Story**: 1.2 - Domain Models Development  
**Priority**: High  
**Estimated Hours**: 8  
**Assigned To**: Domain Expert  
**Dependencies**: Task 1.1.1 - Create Solution Structure, Task 1.1.2 - Setup Dependency Injection

## 🎯 Objective
Implement PrinterConfiguration aggregate root với business rules, validation logic, và domain events theo Domain-Driven Design principles cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **PrinterConfiguration Aggregate Root**:
  - Unique identifier với PrinterConfigurationId
  - Name và description fields
  - Printer settings với device information
  - Food category mapping
  - Template management
  - Print queue configuration
  - Status tracking (Online, Offline, Error)
  - Creation và modification timestamps

- [ ] **Business Rules Implementation**:
  - Mỗi food category có thể có multiple printers
  - Validation printer availability trước khi assign
  - Template versioning và rollback capability
  - Print job retry mechanism
  - Printer discovery và auto-configuration

- [ ] **Value Objects**:
  - PrinterSettings với device information
  - PrinterConfigurationId với uniqueness
  - PrinterStatus enum
  - FoodCategory enum
  - TemplateSettings với versioning

### Technical Requirements
- [ ] **Entity Structure**:
  ```csharp
  // SystemConfig.Domain/Entities/PrinterConfiguration.cs
  public class PrinterConfiguration : AggregateRoot
  {
      public PrinterConfigurationId Id { get; private set; }
      public string Name { get; private set; }
      public string Description { get; private set; }
      public PrinterSettings PrinterSettings { get; private set; }
      public PrinterStatus Status { get; private set; }
      public List<FoodCategory> AssignedCategories { get; private set; }
      public TemplateSettings TemplateSettings { get; private set; }
      public PrintQueueSettings PrintQueueSettings { get; private set; }
      public DateTime CreatedAt { get; private set; }
      public DateTime? LastModifiedAt { get; private set; }
      public string CreatedBy { get; private set; }
      public string LastModifiedBy { get; private set; }
      
      // Business methods
      public void UpdatePrinterSettings(PrinterSettings settings, string modifiedBy)
      public void AssignFoodCategory(FoodCategory category, string modifiedBy)
      public void RemoveFoodCategory(FoodCategory category, string modifiedBy)
      public void UpdateTemplateSettings(TemplateSettings settings, string modifiedBy)
      public void UpdateStatus(PrinterStatus status)
      public void TestPrint(string testContent)
      public void ValidatePrinterAvailability()
  }
  ```

- [ ] **Value Objects**:
  ```csharp
  // SystemConfig.Domain/ValueObjects/PrinterConfigurationId.cs
  public class PrinterConfigurationId : ValueObject
  {
      public Guid Value { get; }
      
      public PrinterConfigurationId(Guid value)
      {
          if (value == Guid.Empty)
              throw new DomainException("Printer configuration ID cannot be empty");
          
          Value = value;
      }
      
      public static PrinterConfigurationId New() => new PrinterConfigurationId(Guid.NewGuid());
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Value;
      }
      
      public static implicit operator Guid(PrinterConfigurationId id) => id.Value;
      public static implicit operator PrinterConfigurationId(Guid value) => new PrinterConfigurationId(value);
  }
  
  // SystemConfig.Domain/ValueObjects/PrinterSettings.cs
  public class PrinterSettings : ValueObject
  {
      public string DeviceName { get; }
      public string PortName { get; }
      public string DriverName { get; }
      public string IPAddress { get; }
      public int Port { get; }
      public bool IsNetworkPrinter { get; }
      public PaperSize PaperSize { get; }
      public PrintQuality PrintQuality { get; }
      public bool AutoCut { get; }
      public bool CashDrawer { get; }
      
      public PrinterSettings(
          string deviceName,
          string portName,
          string driverName,
          string ipAddress = null,
          int port = 0,
          bool isNetworkPrinter = false,
          PaperSize paperSize = PaperSize.A4,
          PrintQuality printQuality = PrintQuality.Normal,
          bool autoCut = false,
          bool cashDrawer = false)
      {
          if (string.IsNullOrWhiteSpace(deviceName))
              throw new DomainException("Device name cannot be empty");
          
          if (string.IsNullOrWhiteSpace(portName))
              throw new DomainException("Port name cannot be empty");
          
          if (string.IsNullOrWhiteSpace(driverName))
              throw new DomainException("Driver name cannot be empty");
          
          DeviceName = deviceName;
          PortName = portName;
          DriverName = driverName;
          IPAddress = ipAddress;
          Port = port;
          IsNetworkPrinter = isNetworkPrinter;
          PaperSize = paperSize;
          PrintQuality = printQuality;
          AutoCut = autoCut;
          CashDrawer = cashDrawer;
      }
      
      public bool IsValid()
      {
          return !string.IsNullOrWhiteSpace(DeviceName) &&
                 !string.IsNullOrWhiteSpace(PortName) &&
                 !string.IsNullOrWhiteSpace(DriverName);
      }
      
      public string GetConnectionString()
      {
          if (IsNetworkPrinter && !string.IsNullOrWhiteSpace(IPAddress))
          {
              return $"TCP:{IPAddress}:{Port}";
          }
          
          return PortName;
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return DeviceName;
          yield return PortName;
          yield return DriverName;
          yield return IPAddress;
          yield return Port;
          yield return IsNetworkPrinter;
          yield return PaperSize;
          yield return PrintQuality;
          yield return AutoCut;
          yield return CashDrawer;
      }
  }
  
  // SystemConfig.Domain/ValueObjects/TemplateSettings.cs
  public class TemplateSettings : ValueObject
  {
      public string TemplateName { get; }
      public string TemplateContent { get; }
      public int Version { get; }
      public DateTime CreatedAt { get; }
      public string CreatedBy { get; }
      
      public TemplateSettings(
          string templateName,
          string templateContent,
          int version,
          string createdBy)
      {
          if (string.IsNullOrWhiteSpace(templateName))
              throw new DomainException("Template name cannot be empty");
          
          if (string.IsNullOrWhiteSpace(templateContent))
              throw new DomainException("Template content cannot be empty");
          
          if (version <= 0)
              throw new DomainException("Version must be greater than 0");
          
          if (string.IsNullOrWhiteSpace(createdBy))
              throw new DomainException("Created by cannot be empty");
          
          TemplateName = templateName;
          TemplateContent = templateContent;
          Version = version;
          CreatedAt = DateTime.UtcNow;
          CreatedBy = createdBy;
      }
      
      public TemplateSettings CreateNewVersion(string newContent, string modifiedBy)
      {
          return new TemplateSettings(TemplateName, newContent, Version + 1, modifiedBy);
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return TemplateName;
          yield return TemplateContent;
          yield return Version;
          yield return CreatedAt;
          yield return CreatedBy;
      }
  }
  ```

- [ ] **Enums**:
  ```csharp
  // SystemConfig.Domain/Enums/PrinterStatus.cs
  public enum PrinterStatus
  {
      Unknown,
      Online,
      Offline,
      Error,
      PaperOut,
      PaperJam,
      OutOfInk
  }
  
  // SystemConfig.Domain/Enums/FoodCategory.cs
  public enum FoodCategory
  {
      Appetizer,
      MainCourse,
      Dessert,
      Beverage,
      Alcoholic,
      Special,
      All
  }
  
  // SystemConfig.Domain/Enums/PaperSize.cs
  public enum PaperSize
  {
      A4,
      A5,
      A6,
      Receipt,
      Label,
      Custom
  }
  
  // SystemConfig.Domain/Enums/PrintQuality.cs
  public enum PrintQuality
  {
      Draft,
      Normal,
      High,
      Photo
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
// SystemConfig.Domain/Entities/PrinterConfiguration.cs
public class PrinterConfiguration : AggregateRoot
{
    public PrinterConfigurationId Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public PrinterSettings PrinterSettings { get; private set; }
    public PrinterStatus Status { get; private set; }
    public List<FoodCategory> AssignedCategories { get; private set; }
    public TemplateSettings TemplateSettings { get; private set; }
    public PrintQueueSettings PrintQueueSettings { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public string CreatedBy { get; private set; }
    public string LastModifiedBy { get; private set; }
    
    // Private constructor for EF Core
    private PrinterConfiguration()
    {
        AssignedCategories = new List<FoodCategory>();
    }
    
    // Factory method
    public static PrinterConfiguration Create(
        string name,
        string description,
        PrinterSettings printerSettings,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new DomainException("Created by cannot be empty");
        
        var configuration = new PrinterConfiguration
        {
            Id = PrinterConfigurationId.New(),
            Name = name,
            Description = description ?? string.Empty,
            PrinterSettings = printerSettings ?? throw new DomainException("Printer settings cannot be null"),
            Status = PrinterStatus.Unknown,
            TemplateSettings = null,
            PrintQueueSettings = new PrintQueueSettings(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
        
        configuration.AddDomainEvent(new PrinterConfigurationCreatedEvent(configuration.Id, configuration.Name));
        
        return configuration;
    }
    
    public void UpdatePrinterSettings(PrinterSettings settings, string modifiedBy)
    {
        if (settings == null)
            throw new DomainException("Printer settings cannot be null");
        
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        var oldSettings = PrinterSettings;
        PrinterSettings = settings;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new PrinterConfigurationSettingsUpdatedEvent(Id, oldSettings, settings));
    }
    
    public void AssignFoodCategory(FoodCategory category, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        if (AssignedCategories.Contains(category))
            return; // Already assigned
        
        AssignedCategories.Add(category);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new PrinterConfigurationCategoryAssignedEvent(Id, category));
    }
    
    public void RemoveFoodCategory(FoodCategory category, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        if (!AssignedCategories.Contains(category))
            return; // Not assigned
        
        AssignedCategories.Remove(category);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new PrinterConfigurationCategoryRemovedEvent(Id, category));
    }
    
    public void UpdateTemplateSettings(TemplateSettings settings, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(modifiedBy))
            throw new DomainException("Modified by cannot be empty");
        
        var oldSettings = TemplateSettings;
        TemplateSettings = settings;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
        
        AddDomainEvent(new PrinterConfigurationTemplateUpdatedEvent(Id, oldSettings, settings));
    }
    
    public void UpdateStatus(PrinterStatus status)
    {
        var oldStatus = Status;
        Status = status;
        
        if (oldStatus != status)
        {
            AddDomainEvent(new PrinterConfigurationStatusChangedEvent(Id, oldStatus, status));
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
        
        AddDomainEvent(new PrinterConfigurationNameUpdatedEvent(Id, oldName, name));
    }
    
    public void TestPrint(string testContent)
    {
        if (Status != PrinterStatus.Online)
        {
            throw new DomainException("Printer is not online");
        }
        
        if (string.IsNullOrWhiteSpace(testContent))
        {
            testContent = "Test Print - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        
        AddDomainEvent(new PrinterConfigurationTestPrintEvent(Id, testContent));
    }
    
    public void ValidatePrinterAvailability()
    {
        if (!PrinterSettings.IsValid())
        {
            UpdateStatus(PrinterStatus.Error);
            throw new DomainException("Printer settings are invalid");
        }
        
        // Additional validation logic can be added here
        UpdateStatus(PrinterStatus.Online);
    }
}
```

### Phase 2: Domain Events Implementation (2 hours)
```csharp
// SystemConfig.Domain/Events/PrinterConfigurationEvents.cs
public class PrinterConfigurationCreatedEvent : DomainEvent
{
    public PrinterConfigurationId ConfigurationId { get; }
    public string Name { get; }
    
    public PrinterConfigurationCreatedEvent(PrinterConfigurationId configurationId, string name)
    {
        ConfigurationId = configurationId;
        Name = name;
    }
}

public class PrinterConfigurationSettingsUpdatedEvent : DomainEvent
{
    public PrinterConfigurationId ConfigurationId { get; }
    public PrinterSettings OldSettings { get; }
    public PrinterSettings NewSettings { get; }
    
    public PrinterConfigurationSettingsUpdatedEvent(
        PrinterConfigurationId configurationId,
        PrinterSettings oldSettings,
        PrinterSettings newSettings)
    {
        ConfigurationId = configurationId;
        OldSettings = oldSettings;
        NewSettings = newSettings;
    }
}

public class PrinterConfigurationCategoryAssignedEvent : DomainEvent
{
    public PrinterConfigurationId ConfigurationId { get; }
    public FoodCategory Category { get; }
    
    public PrinterConfigurationCategoryAssignedEvent(PrinterConfigurationId configurationId, FoodCategory category)
    {
        ConfigurationId = configurationId;
        Category = category;
    }
}

public class PrinterConfigurationStatusChangedEvent : DomainEvent
{
    public PrinterConfigurationId ConfigurationId { get; }
    public PrinterStatus OldStatus { get; }
    public PrinterStatus NewStatus { get; }
    
    public PrinterConfigurationStatusChangedEvent(
        PrinterConfigurationId configurationId,
        PrinterStatus oldStatus,
        PrinterStatus newStatus)
    {
        ConfigurationId = configurationId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

public class PrinterConfigurationTestPrintEvent : DomainEvent
{
    public PrinterConfigurationId ConfigurationId { get; }
    public string TestContent { get; }
    
    public PrinterConfigurationTestPrintEvent(PrinterConfigurationId configurationId, string testContent)
    {
        ConfigurationId = configurationId;
        TestContent = testContent;
    }
}
```

### Phase 3: Business Rules & Validation (3 hours)
```csharp
// SystemConfig.Domain/Services/IPrinterConfigurationValidator.cs
public interface IPrinterConfigurationValidator
{
    Task<ValidationResult> ValidateAsync(PrinterConfiguration configuration);
    Task<ValidationResult> ValidatePrinterAvailabilityAsync(PrinterConfiguration configuration);
    Task<ValidationResult> ValidateTemplateSettingsAsync(TemplateSettings templateSettings);
    Task<ValidationResult> ValidateFoodCategoryAssignmentAsync(PrinterConfiguration configuration, FoodCategory category);
}

// SystemConfig.Domain/Services/PrinterConfigurationValidator.cs
public class PrinterConfigurationValidator : IPrinterConfigurationValidator
{
    public async Task<ValidationResult> ValidateAsync(PrinterConfiguration configuration)
    {
        var errors = new List<ValidationError>();
        
        // Validate name
        if (string.IsNullOrWhiteSpace(configuration.Name))
        {
            errors.Add(new ValidationError("Name", "Name cannot be empty"));
        }
        
        // Validate printer settings
        if (configuration.PrinterSettings == null)
        {
            errors.Add(new ValidationError("PrinterSettings", "Printer settings cannot be null"));
        }
        else if (!configuration.PrinterSettings.IsValid())
        {
            errors.Add(new ValidationError("PrinterSettings", "Printer settings are invalid"));
        }
        
        // Validate status
        if (!Enum.IsDefined(typeof(PrinterStatus), configuration.Status))
        {
            errors.Add(new ValidationError("Status", "Invalid printer status"));
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors) 
            : ValidationResult.Success();
    }
    
    public async Task<ValidationResult> ValidatePrinterAvailabilityAsync(PrinterConfiguration configuration)
    {
        var errors = new List<ValidationError>();
        
        if (configuration.Status == PrinterStatus.Offline)
        {
            errors.Add(new ValidationError("Status", "Printer is offline"));
        }
        
        if (configuration.Status == PrinterStatus.Error)
        {
            errors.Add(new ValidationError("Status", "Printer has error status"));
        }
        
        if (configuration.Status == PrinterStatus.PaperOut)
        {
            errors.Add(new ValidationError("Status", "Printer is out of paper"));
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors) 
            : ValidationResult.Success();
    }
    
    public async Task<ValidationResult> ValidateTemplateSettingsAsync(TemplateSettings templateSettings)
    {
        var errors = new List<ValidationError>();
        
        if (templateSettings == null)
        {
            errors.Add(new ValidationError("TemplateSettings", "Template settings cannot be null"));
            return ValidationResult.Failure(errors);
        }
        
        if (string.IsNullOrWhiteSpace(templateSettings.TemplateName))
        {
            errors.Add(new ValidationError("TemplateName", "Template name cannot be empty"));
        }
        
        if (string.IsNullOrWhiteSpace(templateSettings.TemplateContent))
        {
            errors.Add(new ValidationError("TemplateContent", "Template content cannot be empty"));
        }
        
        if (templateSettings.Version <= 0)
        {
            errors.Add(new ValidationError("Version", "Template version must be greater than 0"));
        }
        
        return errors.Any() 
            ? ValidationResult.Failure(errors) 
            : ValidationResult.Success();
    }
    
    public async Task<ValidationResult> ValidateFoodCategoryAssignmentAsync(PrinterConfiguration configuration, FoodCategory category)
    {
        var errors = new List<ValidationError>();
        
        if (configuration.AssignedCategories.Contains(category))
        {
            errors.Add(new ValidationError("FoodCategory", $"Category {category} is already assigned to this printer"));
        }
        
        if (configuration.Status != PrinterStatus.Online)
        {
            errors.Add(new ValidationError("Status", "Cannot assign category to offline printer"));
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
// SystemConfig.UnitTests/Domain/PrinterConfigurationTests.cs
public class PrinterConfigurationTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateConfiguration()
    {
        // Arrange
        var name = "Test Printer";
        var description = "Test description";
        var printerSettings = new PrinterSettings("TestDevice", "COM1", "TestDriver");
        var createdBy = "testuser";
        
        // Act
        var configuration = PrinterConfiguration.Create(name, description, printerSettings, createdBy);
        
        // Assert
        Assert.NotNull(configuration);
        Assert.Equal(name, configuration.Name);
        Assert.Equal(description, configuration.Description);
        Assert.Equal(printerSettings, configuration.PrinterSettings);
        Assert.Equal(PrinterStatus.Unknown, configuration.Status);
        Assert.Empty(configuration.AssignedCategories);
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
        var printerSettings = new PrinterSettings("TestDevice", "COM1", "TestDriver");
        
        // Act & Assert
        Assert.Throws<DomainException>(() => 
            PrinterConfiguration.Create(name, description, printerSettings, createdBy));
    }
    
    [Fact]
    public void AssignFoodCategory_WithValidCategory_ShouldAssignCategory()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var category = FoodCategory.MainCourse;
        var modifiedBy = "testuser";
        
        // Act
        configuration.AssignFoodCategory(category, modifiedBy);
        
        // Assert
        Assert.Contains(category, configuration.AssignedCategories);
        Assert.Equal(modifiedBy, configuration.LastModifiedBy);
        Assert.True(configuration.LastModifiedAt > DateTime.UtcNow.AddMinutes(-1));
    }
    
    [Fact]
    public void RemoveFoodCategory_WithAssignedCategory_ShouldRemoveCategory()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var category = FoodCategory.MainCourse;
        var modifiedBy = "testuser";
        configuration.AssignFoodCategory(category, modifiedBy);
        
        // Act
        configuration.RemoveFoodCategory(category, modifiedBy);
        
        // Assert
        Assert.DoesNotContain(category, configuration.AssignedCategories);
    }
    
    [Fact]
    public void UpdateStatus_ShouldUpdateStatus()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        var newStatus = PrinterStatus.Online;
        
        // Act
        configuration.UpdateStatus(newStatus);
        
        // Assert
        Assert.Equal(newStatus, configuration.Status);
    }
    
    [Fact]
    public void TestPrint_WithOnlinePrinter_ShouldCreateTestPrintEvent()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        configuration.UpdateStatus(PrinterStatus.Online);
        var testContent = "Test print content";
        
        // Act
        configuration.TestPrint(testContent);
        
        // Assert
        Assert.Contains(configuration.DomainEvents, e => e is PrinterConfigurationTestPrintEvent);
    }
    
    [Fact]
    public void TestPrint_WithOfflinePrinter_ShouldThrowException()
    {
        // Arrange
        var configuration = CreateTestConfiguration();
        configuration.UpdateStatus(PrinterStatus.Offline);
        
        // Act & Assert
        Assert.Throws<DomainException>(() => configuration.TestPrint("test"));
    }
    
    private PrinterConfiguration CreateTestConfiguration()
    {
        var printerSettings = new PrinterSettings("TestDevice", "COM1", "TestDriver");
        return PrinterConfiguration.Create("Test", "Description", printerSettings, "testuser");
    }
}

// SystemConfig.UnitTests/Domain/ValueObjects/PrinterSettingsTests.cs
public class PrinterSettingsTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateSettings()
    {
        // Arrange
        var deviceName = "TestDevice";
        var portName = "COM1";
        var driverName = "TestDriver";
        
        // Act
        var settings = new PrinterSettings(deviceName, portName, driverName);
        
        // Assert
        Assert.Equal(deviceName, settings.DeviceName);
        Assert.Equal(portName, settings.PortName);
        Assert.Equal(driverName, settings.DriverName);
        Assert.True(settings.IsValid());
    }
    
    [Theory]
    [InlineData("", "port", "driver")]
    [InlineData("device", "", "driver")]
    [InlineData("device", "port", "")]
    public void Create_WithInvalidParameters_ShouldThrowException(string deviceName, string portName, string driverName)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => 
            new PrinterSettings(deviceName, portName, driverName));
    }
    
    [Fact]
    public void GetConnectionString_ForLocalPrinter_ShouldReturnPortName()
    {
        // Arrange
        var settings = new PrinterSettings("TestDevice", "COM1", "TestDriver");
        
        // Act
        var connectionString = settings.GetConnectionString();
        
        // Assert
        Assert.Equal("COM1", connectionString);
    }
    
    [Fact]
    public void GetConnectionString_ForNetworkPrinter_ShouldReturnTCPAddress()
    {
        // Arrange
        var settings = new PrinterSettings("TestDevice", "COM1", "TestDriver", "192.168.1.100", 9100, true);
        
        // Act
        var connectionString = settings.GetConnectionString();
        
        // Assert
        Assert.Equal("TCP:192.168.1.100:9100", connectionString);
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Domain/PrinterConfigurationIntegrationTests.cs
public class PrinterConfigurationIntegrationTests
{
    [Fact]
    public async Task PrinterConfiguration_WithDomainEvents_ShouldPublishEvents()
    {
        // Arrange
        var configuration = PrinterConfiguration.Create("Test", "Description", 
            new PrinterSettings("TestDevice", "COM1", "TestDriver"), "testuser");
        
        // Act
        // Domain events should be published when configuration is created
        
        // Assert
        Assert.Single(configuration.DomainEvents);
        Assert.IsType<PrinterConfigurationCreatedEvent>(configuration.DomainEvents.First());
    }
    
    [Fact]
    public async Task PrinterConfiguration_WithCategoryAssignment_ShouldPublishEvents()
    {
        // Arrange
        var configuration = PrinterConfiguration.Create("Test", "Description", 
            new PrinterSettings("TestDevice", "COM1", "TestDriver"), "testuser");
        
        // Act
        configuration.AssignFoodCategory(FoodCategory.MainCourse, "testuser");
        
        // Assert
        Assert.Contains(configuration.DomainEvents, e => e is PrinterConfigurationCategoryAssignedEvent);
    }
}
```

## 📊 Definition of Done
- [ ] **Entity Implementation**: PrinterConfiguration aggregate root được implement đầy đủ
- [ ] **Value Objects**: PrinterSettings, PrinterConfigurationId, TemplateSettings được implement
- [ ] **Business Rules**: Tất cả business rules được implement
- [ ] **Domain Events**: Events được publish cho important changes
- [ ] **Validation**: Comprehensive validation cho tất cả inputs
- [ ] **Unit Tests**: >95% coverage cho domain entities
- [ ] **Integration Tests**: Domain events tests pass
- [ ] **Code Review**: Domain model được approve
- [ ] **Documentation**: Business rules documentation hoàn thành

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Complex printer device integration
- **Mitigation**: Start với simple printer settings, add complexity gradually

- **Risk**: Template management complexity
- **Mitigation**: Implement basic template versioning first

- **Risk**: Network printer discovery issues
- **Mitigation**: Implement fallback mechanisms

### Domain Risks
- **Risk**: Printer-specific business rules không đầy đủ
- **Mitigation**: Regular stakeholder review

- **Risk**: Food category mapping complexity
- **Mitigation**: Start với simple categories, extend later

## 📚 Resources & References
- Domain-Driven Design by Eric Evans
- Clean Architecture by Robert C. Martin
- .NET 8 Value Objects Best Practices
- Printer Device Integration Patterns
- Template Management Best Practices

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
- Focus on domain logic, not printer hardware specifics
- Use value objects for complex printer settings
- Implement proper encapsulation
- Consider future extensibility
- Document business rules clearly
- Regular domain model reviews 