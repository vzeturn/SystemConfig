# Task 1.2.4: Value Objects Implementation

## 📋 Task Overview
**Sprint**: 1  
**Story**: 1.2 - Domain Models Development  
**Priority**: High  
**Estimated Hours**: 6  
**Assigned To**: Domain Expert  
**Dependencies**: Task 1.1.1 - Create Solution Structure, Task 1.1.2 - Setup Dependency Injection

## 🎯 Objective
Implement common value objects và base classes cho domain entities theo Domain-Driven Design principles cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Base Value Object Class**:
  - Abstract base class cho tất cả value objects
  - Equality comparison implementation
  - Immutability enforcement
  - Serialization support

- [ ] **Common Value Objects**:
  - Email address với validation
  - Phone number với formatting
  - Money với currency support
  - DateRange với business logic
  - Address với structured data

- [ ] **Domain-Specific Value Objects**:
  - StoreId với uniqueness
  - UserId với validation
  - ConfigurationKey với naming rules
  - VersionNumber với semantic versioning

### Technical Requirements
- [ ] **Base Value Object Structure**:
  ```csharp
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
      
      public static bool operator ==(ValueObject left, ValueObject right)
      {
          return EqualOperator(left, right);
      }
      
      public static bool operator !=(ValueObject left, ValueObject right)
      {
          return NotEqualOperator(left, right);
      }
      
      protected static bool EqualOperator(ValueObject left, ValueObject right)
      {
          if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
              return true;
          
          if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
              return false;
          
          return left.Equals(right);
      }
      
      protected static bool NotEqualOperator(ValueObject left, ValueObject right)
      {
          return !EqualOperator(left, right);
      }
  }
  ```

- [ ] **Common Value Objects**:
  ```csharp
  // SystemConfig.Domain/ValueObjects/Email.cs
  public class Email : ValueObject
  {
      public string Value { get; }
      
      public Email(string value)
      {
          if (string.IsNullOrWhiteSpace(value))
              throw new DomainException("Email cannot be empty");
          
          if (!IsValidEmail(value))
              throw new DomainException("Invalid email format");
          
          Value = value.ToLowerInvariant();
      }
      
      private static bool IsValidEmail(string email)
      {
          try
          {
              var addr = new System.Net.Mail.MailAddress(email);
              return addr.Address == email;
          }
          catch
          {
              return false;
          }
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Value;
      }
      
      public static implicit operator string(Email email) => email.Value;
  }
  
  // SystemConfig.Domain/ValueObjects/PhoneNumber.cs
  public class PhoneNumber : ValueObject
  {
      public string Value { get; }
      public string CountryCode { get; }
      
      public PhoneNumber(string value, string countryCode = "+84")
      {
          if (string.IsNullOrWhiteSpace(value))
              throw new DomainException("Phone number cannot be empty");
          
          if (!IsValidPhoneNumber(value))
              throw new DomainException("Invalid phone number format");
          
          Value = NormalizePhoneNumber(value);
          CountryCode = countryCode;
      }
      
      private static bool IsValidPhoneNumber(string phoneNumber)
      {
          // Remove all non-digit characters
          var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
          return digits.Length >= 10 && digits.Length <= 15;
      }
      
      private static string NormalizePhoneNumber(string phoneNumber)
      {
          // Remove all non-digit characters
          var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
          
          // Format as Vietnamese phone number
          if (digits.StartsWith("84"))
          {
              digits = digits.Substring(2);
          }
          
          if (digits.StartsWith("0"))
          {
              digits = digits.Substring(1);
          }
          
          return digits;
      }
      
      public string GetFormattedNumber()
      {
          return $"{CountryCode}{Value}";
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Value;
          yield return CountryCode;
      }
  }
  
  // SystemConfig.Domain/ValueObjects/Money.cs
  public class Money : ValueObject
  {
      public decimal Amount { get; }
      public string Currency { get; }
      
      public Money(decimal amount, string currency = "VND")
      {
          if (amount < 0)
              throw new DomainException("Amount cannot be negative");
          
          if (string.IsNullOrWhiteSpace(currency))
              throw new DomainException("Currency cannot be empty");
          
          Amount = amount;
          Currency = currency.ToUpperInvariant();
      }
      
      public Money Add(Money other)
      {
          if (Currency != other.Currency)
              throw new DomainException("Cannot add money with different currencies");
          
          return new Money(Amount + other.Amount, Currency);
      }
      
      public Money Subtract(Money other)
      {
          if (Currency != other.Currency)
              throw new DomainException("Cannot subtract money with different currencies");
          
          return new Money(Amount - other.Amount, Currency);
      }
      
      public Money Multiply(decimal factor)
      {
          return new Money(Amount * factor, Currency);
      }
      
      public string GetFormattedAmount()
      {
          return $"{Amount:N0} {Currency}";
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Amount;
          yield return Currency;
      }
  }
  
  // SystemConfig.Domain/ValueObjects/DateRange.cs
  public class DateRange : ValueObject
  {
      public DateTime StartDate { get; }
      public DateTime EndDate { get; }
      
      public DateRange(DateTime startDate, DateTime endDate)
      {
          if (startDate >= endDate)
              throw new DomainException("Start date must be before end date");
          
          StartDate = startDate.Date;
          EndDate = endDate.Date;
      }
      
      public bool Contains(DateTime date)
      {
          return date.Date >= StartDate && date.Date <= EndDate;
      }
      
      public int GetDays()
      {
          return (EndDate - StartDate).Days + 1;
      }
      
      public bool Overlaps(DateRange other)
      {
          return StartDate <= other.EndDate && EndDate >= other.StartDate;
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return StartDate;
          yield return EndDate;
      }
  }
  
  // SystemConfig.Domain/ValueObjects/Address.cs
  public class Address : ValueObject
  {
      public string Street { get; }
      public string City { get; }
      public string State { get; }
      public string PostalCode { get; }
      public string Country { get; }
      
      public Address(string street, string city, string state = null, string postalCode = null, string country = "Vietnam")
      {
          if (string.IsNullOrWhiteSpace(street))
              throw new DomainException("Street cannot be empty");
          
          if (string.IsNullOrWhiteSpace(city))
              throw new DomainException("City cannot be empty");
          
          Street = street;
          City = city;
          State = state;
          PostalCode = postalCode;
          Country = country ?? "Vietnam";
      }
      
      public string GetFullAddress()
      {
          var parts = new List<string> { Street, City };
          
          if (!string.IsNullOrWhiteSpace(State))
              parts.Add(State);
          
          if (!string.IsNullOrWhiteSpace(PostalCode))
              parts.Add(PostalCode);
          
          parts.Add(Country);
          
          return string.Join(", ", parts);
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Street;
          yield return City;
          yield return State;
          yield return PostalCode;
          yield return Country;
      }
  }
  ```

- [ ] **Domain-Specific Value Objects**:
  ```csharp
  // SystemConfig.Domain/ValueObjects/StoreId.cs
  public class StoreId : ValueObject
  {
      public string Value { get; }
      
      public StoreId(string value)
      {
          if (string.IsNullOrWhiteSpace(value))
              throw new DomainException("Store ID cannot be empty");
          
          if (!IsValidStoreId(value))
              throw new DomainException("Invalid store ID format");
          
          Value = value.ToUpperInvariant();
      }
      
      private static bool IsValidStoreId(string storeId)
      {
          // Store ID format: STORE-XXXXX (5 digits)
          return System.Text.RegularExpressions.Regex.IsMatch(storeId, @"^STORE-\d{5}$");
      }
      
      public static StoreId Generate()
      {
          var random = new Random();
          var number = random.Next(10000, 99999);
          return new StoreId($"STORE-{number:D5}");
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Value;
      }
      
      public static implicit operator string(StoreId storeId) => storeId.Value;
  }
  
  // SystemConfig.Domain/ValueObjects/UserId.cs
  public class UserId : ValueObject
  {
      public Guid Value { get; }
      
      public UserId(Guid value)
      {
          if (value == Guid.Empty)
              throw new DomainException("User ID cannot be empty");
          
          Value = value;
      }
      
      public static UserId New() => new UserId(Guid.NewGuid());
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Value;
      }
      
      public static implicit operator Guid(UserId userId) => userId.Value;
      public static implicit operator UserId(Guid value) => new UserId(value);
  }
  
  // SystemConfig.Domain/ValueObjects/ConfigurationKey.cs
  public class ConfigurationKey : ValueObject
  {
      public string Value { get; }
      
      public ConfigurationKey(string value)
      {
          if (string.IsNullOrWhiteSpace(value))
              throw new DomainException("Configuration key cannot be empty");
          
          if (!IsValidConfigurationKey(value))
              throw new DomainException("Invalid configuration key format");
          
          Value = value.ToLowerInvariant();
      }
      
      private static bool IsValidConfigurationKey(string key)
      {
          // Key format: lowercase, alphanumeric, dots, and underscores only
          return System.Text.RegularExpressions.Regex.IsMatch(key, @"^[a-z0-9._]+$");
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Value;
      }
      
      public static implicit operator string(ConfigurationKey key) => key.Value;
  }
  
  // SystemConfig.Domain/ValueObjects/VersionNumber.cs
  public class VersionNumber : ValueObject
  {
      public int Major { get; }
      public int Minor { get; }
      public int Patch { get; }
      public string PreRelease { get; }
      
      public VersionNumber(int major, int minor, int patch, string preRelease = null)
      {
          if (major < 0 || minor < 0 || patch < 0)
              throw new DomainException("Version numbers cannot be negative");
          
          Major = major;
          Minor = minor;
          Patch = patch;
          PreRelease = preRelease;
      }
      
      public static VersionNumber Parse(string version)
      {
          if (string.IsNullOrWhiteSpace(version))
              throw new DomainException("Version string cannot be empty");
          
          var parts = version.Split('.');
          if (parts.Length < 3)
              throw new DomainException("Invalid version format");
          
          if (!int.TryParse(parts[0], out var major) ||
              !int.TryParse(parts[1], out var minor) ||
              !int.TryParse(parts[2], out var patch))
          {
              throw new DomainException("Invalid version numbers");
          }
          
          string preRelease = null;
          if (parts.Length > 3)
          {
              preRelease = parts[3];
          }
          
          return new VersionNumber(major, minor, patch, preRelease);
      }
      
      public VersionNumber IncrementMajor()
      {
          return new VersionNumber(Major + 1, 0, 0);
      }
      
      public VersionNumber IncrementMinor()
      {
          return new VersionNumber(Major, Minor + 1, 0);
      }
      
      public VersionNumber IncrementPatch()
      {
          return new VersionNumber(Major, Minor, Patch + 1);
      }
      
      public override string ToString()
      {
          var version = $"{Major}.{Minor}.{Patch}";
          if (!string.IsNullOrWhiteSpace(PreRelease))
          {
              version += $"-{PreRelease}";
          }
          return version;
      }
      
      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Major;
          yield return Minor;
          yield return Patch;
          yield return PreRelease;
      }
  }
  ```

### Quality Requirements
- [ ] **Immutability**: Tất cả value objects immutable
- [ ] **Validation**: Comprehensive validation cho tất cả inputs
- [ ] **Equality**: Proper equality comparison
- [ ] **Serialization**: JSON serialization support
- [ ] **Testability**: Easy to test và mock

## 🏗️ Implementation Plan

### Phase 1: Base Value Object Implementation (2 hours)
```csharp
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
    
    public static bool operator ==(ValueObject left, ValueObject right)
    {
        return EqualOperator(left, right);
    }
    
    public static bool operator !=(ValueObject left, ValueObject right)
    {
        return NotEqualOperator(left, right);
    }
    
    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;
        
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            return false;
        
        return left.Equals(right);
    }
    
    protected static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !EqualOperator(left, right);
    }
    
    public virtual object Clone()
    {
        return MemberwiseClone();
    }
}
```

### Phase 2: Common Value Objects Implementation (2 hours)
```csharp
// SystemConfig.Domain/ValueObjects/Email.cs
public class Email : ValueObject
{
    public string Value { get; }
    
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty");
        
        if (!IsValidEmail(value))
            throw new DomainException("Invalid email format");
        
        Value = value.ToLowerInvariant();
    }
    
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(Email email) => email.Value;
    
    public override string ToString()
    {
        return Value;
    }
}

// SystemConfig.Domain/ValueObjects/PhoneNumber.cs
public class PhoneNumber : ValueObject
{
    public string Value { get; }
    public string CountryCode { get; }
    
    public PhoneNumber(string value, string countryCode = "+84")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Phone number cannot be empty");
        
        if (!IsValidPhoneNumber(value))
            throw new DomainException("Invalid phone number format");
        
        Value = NormalizePhoneNumber(value);
        CountryCode = countryCode;
    }
    
    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        // Remove all non-digit characters
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        return digits.Length >= 10 && digits.Length <= 15;
    }
    
    private static string NormalizePhoneNumber(string phoneNumber)
    {
        // Remove all non-digit characters
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        
        // Format as Vietnamese phone number
        if (digits.StartsWith("84"))
        {
            digits = digits.Substring(2);
        }
        
        if (digits.StartsWith("0"))
        {
            digits = digits.Substring(1);
        }
        
        return digits;
    }
    
    public string GetFormattedNumber()
    {
        return $"{CountryCode}{Value}";
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return CountryCode;
    }
    
    public override string ToString()
    {
        return GetFormattedNumber();
    }
}

// SystemConfig.Domain/ValueObjects/Money.cs
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public Money(decimal amount, string currency = "VND")
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative");
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency cannot be empty");
        
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Cannot add money with different currencies");
        
        return new Money(Amount + other.Amount, Currency);
    }
    
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Cannot subtract money with different currencies");
        
        return new Money(Amount - other.Amount, Currency);
    }
    
    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }
    
    public string GetFormattedAmount()
    {
        return $"{Amount:N0} {Currency}";
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
    
    public override string ToString()
    {
        return GetFormattedAmount();
    }
}
```

### Phase 3: Domain-Specific Value Objects Implementation (2 hours)
```csharp
// SystemConfig.Domain/ValueObjects/StoreId.cs
public class StoreId : ValueObject
{
    public string Value { get; }
    
    public StoreId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Store ID cannot be empty");
        
        if (!IsValidStoreId(value))
            throw new DomainException("Invalid store ID format");
        
        Value = value.ToUpperInvariant();
    }
    
    private static bool IsValidStoreId(string storeId)
    {
        // Store ID format: STORE-XXXXX (5 digits)
        return System.Text.RegularExpressions.Regex.IsMatch(storeId, @"^STORE-\d{5}$");
    }
    
    public static StoreId Generate()
    {
        var random = new Random();
        var number = random.Next(10000, 99999);
        return new StoreId($"STORE-{number:D5}");
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(StoreId storeId) => storeId.Value;
    
    public override string ToString()
    {
        return Value;
    }
}

// SystemConfig.Domain/ValueObjects/ConfigurationKey.cs
public class ConfigurationKey : ValueObject
{
    public string Value { get; }
    
    public ConfigurationKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Configuration key cannot be empty");
        
        if (!IsValidConfigurationKey(value))
            throw new DomainException("Invalid configuration key format");
        
        Value = value.ToLowerInvariant();
    }
    
    private static bool IsValidConfigurationKey(string key)
    {
        // Key format: lowercase, alphanumeric, dots, and underscores only
        return System.Text.RegularExpressions.Regex.IsMatch(key, @"^[a-z0-9._]+$");
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(ConfigurationKey key) => key.Value;
    
    public override string ToString()
    {
        return Value;
    }
}

// SystemConfig.Domain/ValueObjects/VersionNumber.cs
public class VersionNumber : ValueObject
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string PreRelease { get; }
    
    public VersionNumber(int major, int minor, int patch, string preRelease = null)
    {
        if (major < 0 || minor < 0 || patch < 0)
            throw new DomainException("Version numbers cannot be negative");
        
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
    }
    
    public static VersionNumber Parse(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new DomainException("Version string cannot be empty");
        
        var parts = version.Split('.');
        if (parts.Length < 3)
            throw new DomainException("Invalid version format");
        
        if (!int.TryParse(parts[0], out var major) ||
            !int.TryParse(parts[1], out var minor) ||
            !int.TryParse(parts[2], out var patch))
        {
            throw new DomainException("Invalid version numbers");
        }
        
        string preRelease = null;
        if (parts.Length > 3)
        {
            preRelease = parts[3];
        }
        
        return new VersionNumber(major, minor, patch, preRelease);
    }
    
    public VersionNumber IncrementMajor()
    {
        return new VersionNumber(Major + 1, 0, 0);
    }
    
    public VersionNumber IncrementMinor()
    {
        return new VersionNumber(Major, Minor + 1, 0);
    }
    
    public VersionNumber IncrementPatch()
    {
        return new VersionNumber(Major, Minor, Patch + 1);
    }
    
    public override string ToString()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        if (!string.IsNullOrWhiteSpace(PreRelease))
        {
            version += $"-{PreRelease}";
        }
        return version;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Major;
        yield return Minor;
        yield return Patch;
        yield return PreRelease;
    }
}
```

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Domain/ValueObjects/EmailTests.cs
public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("user+tag@example.com")]
    public void Create_WithValidEmail_ShouldCreateEmail(string email)
    {
        // Act
        var emailValue = new Email(email);
        
        // Assert
        Assert.Equal(email.ToLowerInvariant(), emailValue.Value);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void Create_WithInvalidEmail_ShouldThrowException(string email)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new Email(email));
    }
    
    [Fact]
    public void Equality_WithSameEmail_ShouldBeEqual()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("TEST@EXAMPLE.COM");
        
        // Act & Assert
        Assert.Equal(email1, email2);
        Assert.True(email1 == email2);
    }
}

// SystemConfig.UnitTests/Domain/ValueObjects/PhoneNumberTests.cs
public class PhoneNumberTests
{
    [Theory]
    [InlineData("0123456789")]
    [InlineData("0987654321")]
    [InlineData("+84123456789")]
    public void Create_WithValidPhoneNumber_ShouldCreatePhoneNumber(string phoneNumber)
    {
        // Act
        var phone = new PhoneNumber(phoneNumber);
        
        // Assert
        Assert.NotNull(phone.Value);
        Assert.Equal("+84", phone.CountryCode);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("123")]
    [InlineData("abcdefghij")]
    public void Create_WithInvalidPhoneNumber_ShouldThrowException(string phoneNumber)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new PhoneNumber(phoneNumber));
    }
    
    [Fact]
    public void GetFormattedNumber_ShouldReturnFormattedNumber()
    {
        // Arrange
        var phone = new PhoneNumber("0123456789");
        
        // Act
        var formatted = phone.GetFormattedNumber();
        
        // Assert
        Assert.Equal("+84123456789", formatted);
    }
}

// SystemConfig.UnitTests/Domain/ValueObjects/MoneyTests.cs
public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldCreateMoney()
    {
        // Arrange
        var amount = 1000.50m;
        var currency = "VND";
        
        // Act
        var money = new Money(amount, currency);
        
        // Assert
        Assert.Equal(amount, money.Amount);
        Assert.Equal(currency.ToUpperInvariant(), money.Currency);
    }
    
    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new Money(-100));
    }
    
    [Fact]
    public void Add_WithSameCurrency_ShouldAddAmounts()
    {
        // Arrange
        var money1 = new Money(100, "VND");
        var money2 = new Money(200, "VND");
        
        // Act
        var result = money1.Add(money2);
        
        // Assert
        Assert.Equal(300m, result.Amount);
        Assert.Equal("VND", result.Currency);
    }
    
    [Fact]
    public void Add_WithDifferentCurrencies_ShouldThrowException()
    {
        // Arrange
        var money1 = new Money(100, "VND");
        var money2 = new Money(200, "USD");
        
        // Act & Assert
        Assert.Throws<DomainException>(() => money1.Add(money2));
    }
}

// SystemConfig.UnitTests/Domain/ValueObjects/StoreIdTests.cs
public class StoreIdTests
{
    [Theory]
    [InlineData("STORE-12345")]
    [InlineData("STORE-00001")]
    [InlineData("STORE-99999")]
    public void Create_WithValidStoreId_ShouldCreateStoreId(string storeId)
    {
        // Act
        var id = new StoreId(storeId);
        
        // Assert
        Assert.Equal(storeId.ToUpperInvariant(), id.Value);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("STORE-1234")]
    [InlineData("STORE-123456")]
    [InlineData("store-12345")]
    [InlineData("SHOP-12345")]
    public void Create_WithInvalidStoreId_ShouldThrowException(string storeId)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new StoreId(storeId));
    }
    
    [Fact]
    public void Generate_ShouldCreateValidStoreId()
    {
        // Act
        var storeId = StoreId.Generate();
        
        // Assert
        Assert.NotNull(storeId.Value);
        Assert.Matches(@"^STORE-\d{5}$", storeId.Value);
    }
}

// SystemConfig.UnitTests/Domain/ValueObjects/VersionNumberTests.cs
public class VersionNumberTests
{
    [Fact]
    public void Create_WithValidVersion_ShouldCreateVersionNumber()
    {
        // Arrange
        var major = 1;
        var minor = 2;
        var patch = 3;
        
        // Act
        var version = new VersionNumber(major, minor, patch);
        
        // Assert
        Assert.Equal(major, version.Major);
        Assert.Equal(minor, version.Minor);
        Assert.Equal(patch, version.Patch);
    }
    
    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 0, -1)]
    public void Create_WithNegativeNumbers_ShouldThrowException(int major, int minor, int patch)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new VersionNumber(major, minor, patch));
    }
    
    [Theory]
    [InlineData("1.2.3", 1, 2, 3, null)]
    [InlineData("1.2.3-beta", 1, 2, 3, "beta")]
    public void Parse_WithValidVersion_ShouldParseCorrectly(string versionString, int major, int minor, int patch, string preRelease)
    {
        // Act
        var version = VersionNumber.Parse(versionString);
        
        // Assert
        Assert.Equal(major, version.Major);
        Assert.Equal(minor, version.Minor);
        Assert.Equal(patch, version.Patch);
        Assert.Equal(preRelease, version.PreRelease);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("1.2")]
    [InlineData("1.2.3.4.5")]
    [InlineData("a.b.c")]
    public void Parse_WithInvalidVersion_ShouldThrowException(string versionString)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => VersionNumber.Parse(versionString));
    }
    
    [Fact]
    public void IncrementMajor_ShouldIncrementMajorAndResetOthers()
    {
        // Arrange
        var version = new VersionNumber(1, 2, 3);
        
        // Act
        var newVersion = version.IncrementMajor();
        
        // Assert
        Assert.Equal(2, newVersion.Major);
        Assert.Equal(0, newVersion.Minor);
        Assert.Equal(0, newVersion.Patch);
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Domain/ValueObjects/ValueObjectIntegrationTests.cs
public class ValueObjectIntegrationTests
{
    [Fact]
    public void ValueObjects_WithSerialization_ShouldSerializeCorrectly()
    {
        // Arrange
        var email = new Email("test@example.com");
        var phone = new PhoneNumber("0123456789");
        var money = new Money(1000, "VND");
        var storeId = new StoreId("STORE-12345");
        var version = new VersionNumber(1, 2, 3);
        
        // Act
        var emailJson = JsonSerializer.Serialize(email);
        var phoneJson = JsonSerializer.Serialize(phone);
        var moneyJson = JsonSerializer.Serialize(money);
        var storeIdJson = JsonSerializer.Serialize(storeId);
        var versionJson = JsonSerializer.Serialize(version);
        
        // Assert
        Assert.NotNull(emailJson);
        Assert.NotNull(phoneJson);
        Assert.NotNull(moneyJson);
        Assert.NotNull(storeIdJson);
        Assert.NotNull(versionJson);
    }
    
    [Fact]
    public void ValueObjects_WithEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("TEST@EXAMPLE.COM");
        var email3 = new Email("other@example.com");
        
        // Act & Assert
        Assert.Equal(email1, email2);
        Assert.NotEqual(email1, email3);
        Assert.True(email1 == email2);
        Assert.False(email1 == email3);
    }
}
```

## 📊 Definition of Done
- [ ] **Base Value Object**: Abstract base class được implement đầy đủ
- [ ] **Common Value Objects**: Email, PhoneNumber, Money, DateRange, Address được implement
- [ ] **Domain-Specific Value Objects**: StoreId, UserId, ConfigurationKey, VersionNumber được implement
- [ ] **Validation**: Comprehensive validation cho tất cả value objects
- [ ] **Immutability**: Tất cả value objects immutable
- [ ] **Equality**: Proper equality comparison implementation
- [ ] **Serialization**: JSON serialization support
- [ ] **Unit Tests**: >95% coverage cho value objects
- [ ] **Integration Tests**: Serialization và equality tests pass
- [ ] **Code Review**: Value objects được approve
- [ ] **Documentation**: Value objects documentation hoàn thành

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Complex validation logic
- **Mitigation**: Start với simple validation, add complexity gradually

- **Risk**: Serialization issues với custom types
- **Mitigation**: Use proper JSON serialization attributes

- **Risk**: Performance issues với value object creation
- **Mitigation**: Use efficient validation algorithms

### Domain Risks
- **Risk**: Validation rules không đầy đủ
- **Mitigation**: Regular stakeholder review

- **Risk**: Business logic complexity
- **Mitigation**: Keep value objects simple và focused

## 📚 Resources & References
- Domain-Driven Design by Eric Evans
- Clean Architecture by Robert C. Martin
- .NET 8 Value Objects Best Practices
- JSON Serialization Guidelines
- Validation Patterns

## 🔄 Dependencies
- Task 1.1.1: Create Solution Structure
- Task 1.1.2: Setup Dependency Injection
- Domain expert consultation

## 📈 Success Metrics
- Value objects follow DDD principles
- All validation rules enforced
- Immutability maintained
- Equality comparison works correctly
- High test coverage achieved
- Serialization works properly

## 📝 Notes
- Keep value objects simple và focused
- Use strong validation rules
- Implement proper immutability
- Consider performance implications
- Document validation rules clearly
- Regular value object reviews 