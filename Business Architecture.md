# Simplified POS Configuration System - Architecture Document

## 📋 Document Information

| **Attribute** | **Value** |
|---------------|-----------|
| **Document Version** | 3.0 (Simplified) |
| **Last Updated** | December 2024 |
| **Architecture Pattern** | Simplified 3-Layer Architecture |
| **Technology Stack** | .NET 8, Windows Forms, Registry, JSON |
| **Target Deployment** | Windows 10/11 Desktop |

---

## 📚 Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Simplified Architecture](#2-simplified-architecture)
3. [Data Models](#3-data-models)
4. [Business Services](#4-business-services)
5. [Registry Management](#5-registry-management)
6. [UI Components](#6-ui-components)
7. [Implementation Plan](#7-implementation-plan)

---

## 1. Executive Summary

### 1.1 📊 Business Context

**Vision**: Tạo một ứng dụng cấu hình POS đơn giản với Windows Forms để quản lý cấu hình database, máy in theo khu vực, và các thiết lập hệ thống tùy chỉnh.

**Yêu cầu cốt lõi**:
- Quản lý kết nối database (chính/phụ)
- Thiết lập máy in theo khu vực (Zone A, B, C...)
- Cấu hình hệ thống tùy chỉnh (key-value)
- Backup/Restore toàn bộ cấu hình
- API cho dự án khác sử dụng
- Tự tạo cấu hình mặc định

### 1.2 🎯 Solution Overview

**Nguyên tắc thiết kế**:
- **Đơn giản**: 3 lớp rõ ràng (UI - Business - Data)
- **Thực dụng**: Chỉ implement những tính năng cần thiết
- **Registry Storage**: Sử dụng Windows Registry làm storage
- **JSON Serialization**: Dễ đọc và maintain
- **External API**: Hỗ trợ dự án khác truy cập cấu hình

---

## 2. Simplified Architecture

### 2.1 🏗️ 3-Layer Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                 PRESENTATION LAYER                          │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐│
│  │ Database Config │ │ Printer Config  │ │ System Config   ││
│  │ Form           │ │ Form           │ │ Form           ││
│  └─────────────────┘ └─────────────────┘ └─────────────────┘│
│  ┌─────────────────┐ ┌─────────────────┐                   │
│  │ Backup Manager  │ │ Main Dashboard  │                   │
│  │ Form           │ │ Form           │                   │
│  └─────────────────┘ └─────────────────┘                   │
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                   BUSINESS LAYER                            │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐│
│  │ ConfigService   │ │ BackupService   │ │ DefaultService  ││
│  │ - CRUD操作      │ │ - Export/Import │ │ - Auto Create   ││
│  └─────────────────┘ └─────────────────┘ └─────────────────┘│
│  ┌─────────────────┐ ┌─────────────────────────────────────┐│
│  │ ValidationSvc   │ │ External API Service                ││
│  │ - Rule Check    │ │ - For Other Projects               ││
│  └─────────────────┘ └─────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────┐
│                   DATA LAYER                                │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐│
│  │ Registry Helper │ │ JSON Converter  │ │ File Helper     ││
│  │ - Read/Write    │ │ - Serialize     │ │ - Backup Files  ││
│  └─────────────────┘ └─────────────────┘ └─────────────────┘│
└─────────────────────────────────────────────────────────────┘
```

### 2.2 🎯 Layer Responsibilities

#### **Presentation Layer**
- Windows Forms UI
- User input validation
- Data binding
- User experience

#### **Business Layer**
- Business logic và validation
- Configuration management
- Backup/Restore operations
- External API for other projects

#### **Data Layer**
- Windows Registry operations
- JSON serialization
- File operations for backup

---

## 3. Data Models

### 3.1 📋 Core Models

```csharp
// Database Configuration Model
public class DatabaseConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsMainDatabase { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    
    public string ConnectionString => 
        $"Server={Server};Database={Database};User Id={Username};Password={Password};";
}

// Printer Configuration Model
public class PrinterConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Zone { get; set; } = string.Empty; // A, B, C, Kitchen, Bar...
    public string PrinterName { get; set; } = string.Empty;
    public string PrinterPath { get; set; } = string.Empty; // Network path or local
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
}

// System Configuration Model
public class SystemConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConfigName { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public string DataType { get; set; } = "String"; // String, Integer, Boolean, Decimal
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastModified { get; set; } = DateTime.Now;
}

// Complete Configuration Container
public class AppConfiguration
{
    public List<DatabaseConfig> Databases { get; set; } = new();
    public List<PrinterConfig> Printers { get; set; } = new();
    public List<SystemConfig> SystemConfigs { get; set; } = new();
    public DateTime LastModified { get; set; } = DateTime.Now;
    public string Version { get; set; } = "1.0.0";
    public string CreatedBy { get; set; } = Environment.UserName;
}

// Backup Information
public class BackupInfo
{
    public string BackupName { get; set; } = string.Empty;
    public DateTime BackupDate { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Version { get; set; } = string.Empty;
}
```

---

## 4. Business Services

### 4.1 🔧 Core Services

```csharp
// Main Configuration Service
public class ConfigurationService
{
    private readonly RegistryHelper _registryHelper;
    private readonly ValidationService _validator;
    
    // Database Configuration Methods
    public async Task<List<DatabaseConfig>> GetDatabasesAsync()
    public async Task<DatabaseConfig> GetDatabaseByIdAsync(string id)
    public async Task<DatabaseConfig> GetMainDatabaseAsync()
    public async Task SaveDatabaseAsync(DatabaseConfig config)
    public async Task DeleteDatabaseAsync(string id)
    public async Task SetAsMainDatabaseAsync(string id)
    
    // Printer Configuration Methods
    public async Task<List<PrinterConfig>> GetPrintersAsync()
    public async Task<List<PrinterConfig>> GetPrintersByZoneAsync(string zone)
    public async Task SavePrinterAsync(PrinterConfig config)
    public async Task DeletePrinterAsync(string id)
    
    // System Configuration Methods
    public async Task<List<SystemConfig>> GetSystemConfigsAsync()
    public async Task<SystemConfig> GetSystemConfigAsync(string configName)
    public async Task SaveSystemConfigAsync(SystemConfig config)
    public async Task DeleteSystemConfigAsync(string id)
    
    // Helper Methods
    public async Task<bool> TestDatabaseConnectionAsync(DatabaseConfig config)
    public async Task<List<string>> GetAvailablePrintersAsync()
}

// Backup and Restore Service
public class BackupService
{
    public async Task<string> CreateBackupAsync(string backupName, string description = "")
    public async Task<List<BackupInfo>> GetBackupHistoryAsync()
    public async Task RestoreFromBackupAsync(string backupFilePath)
    public async Task<bool> ValidateBackupFileAsync(string backupFilePath)
    public async Task DeleteBackupAsync(string backupFilePath)
}

// Default Configuration Service
public class DefaultConfigurationService
{
    public async Task CreateDefaultConfigurationsAsync()
    public async Task<bool> HasDefaultConfigurationsAsync()
    public async Task ResetToDefaultAsync()
    
    private AppConfiguration GetDefaultConfiguration()
    {
        return new AppConfiguration
        {
            Databases = new List<DatabaseConfig>
            {
                new DatabaseConfig
                {
                    Name = "Main Database",
                    Server = "localhost",
                    Database = "POSDatabase",
                    Username = "sa",
                    Password = "",
                    IsMainDatabase = true
                }
            },
            Printers = new List<PrinterConfig>
            {
                new PrinterConfig { Zone = "A", PrinterName = "Receipt Printer A" },
                new PrinterConfig { Zone = "B", PrinterName = "Receipt Printer B" },
                new PrinterConfig { Zone = "Kitchen", PrinterName = "Kitchen Printer" }
            },
            SystemConfigs = new List<SystemConfig>
            {
                new SystemConfig { ConfigName = "CompanyName", ConfigValue = "Your Company", DataType = "String" },
                new SystemConfig { ConfigName = "TaxRate", ConfigValue = "10", DataType = "Decimal" },
                new SystemConfig { ConfigName = "AutoBackup", ConfigValue = "true", DataType = "Boolean" }
            }
        };
    }
}

// External API Service (For other projects)
public class ExternalApiService
{
    // Static methods for other projects to use
    public static DatabaseConfig GetMainDatabase()
    public static List<PrinterConfig> GetPrintersByZone(string zone)
    public static string GetSystemConfigValue(string configName)
    public static T GetSystemConfigValue<T>(string configName)
    public static bool IsConfigurationExists()
}

// Validation Service
public class ValidationService
{
    public ValidationResult ValidateDatabase(DatabaseConfig config)
    public ValidationResult ValidatePrinter(PrinterConfig config)
    public ValidationResult ValidateSystemConfig(SystemConfig config)
    public async Task<bool> TestDatabaseConnectionAsync(DatabaseConfig config)
}
```

---

## 5. Registry Management

### 5.1 💾 Registry Structure

```
HKEY_LOCAL_MACHINE\SOFTWARE\YourCompany\POSConfig\
├── Databases\
│   ├── [GUID1] = {JSON DatabaseConfig}
│   ├── [GUID2] = {JSON DatabaseConfig}
│   └── MainDatabaseId = {GUID1}
├── Printers\
│   ├── [GUID1] = {JSON PrinterConfig}
│   ├── [GUID2] = {JSON PrinterConfig}
│   └── [GUID3] = {JSON PrinterConfig}
├── SystemConfigs\
│   ├── [GUID1] = {JSON SystemConfig}
│   ├── [GUID2] = {JSON SystemConfig}
│   └── [GUID3] = {JSON SystemConfig}
└── Metadata\
    ├── Version = "1.0.0"
    ├── LastModified = "2024-12-20T10:30:00"
    └── CreatedBy = "Administrator"
```

### 5.2 🔧 Registry Helper Implementation

```csharp
public class RegistryHelper
{
    private const string REGISTRY_PATH = @"SOFTWARE\YourCompany\POSConfig";
    
    public async Task<T> GetValueAsync<T>(string subKey, string valueName)
    public async Task SetValueAsync(string subKey, string valueName, object value)
    public async Task DeleteValueAsync(string subKey, string valueName)
    public async Task<List<string>> GetSubKeyNamesAsync(string subKey)
    public async Task<Dictionary<string, object>> GetAllValuesAsync(string subKey)
    
    // JSON Serialization helpers
    private string SerializeToJson<T>(T obj)
    private T DeserializeFromJson<T>(string json)
    
    // Registry key management
    private RegistryKey OpenKey(string subKey, bool writable = false)
    private void EnsureKeyExists(string subKey)
}

// File Helper for Backup operations
public class FileHelper
{
    public async Task<string> ExportToFileAsync(AppConfiguration config, string filePath)
    public async Task<AppConfiguration> ImportFromFileAsync(string filePath)
    public async Task<bool> ValidateBackupFileAsync(string filePath)
    public async Task<List<string>> GetBackupFilesAsync(string backupDirectory)
}
```

---

## 6. UI Components

### 6.1 🖥️ Windows Forms Structure

#### **Main Dashboard (MainForm)**
```csharp
public partial class MainForm : Form
{
    // Overview của toàn bộ hệ thống
    // Quick access buttons
    // Recent activity log
    // System status indicators
}
```

#### **Database Configuration Form**
```csharp
public partial class DatabaseConfigForm : Form
{
    // DataGridView for database list
    // Form for add/edit database
    // Test connection button
    // Set as main database option
}
```

#### **Printer Configuration Form**
```csharp
public partial class PrinterConfigForm : Form
{
    // Zone selection combo
    // Available printers dropdown
    // Printer mapping grid
    // Test print function
}
```

#### **System Configuration Form**
```csharp
public partial class SystemConfigForm : Form
{
    // Dynamic form based on config type
    // Add custom config option
    // Validation feedback
    // Save/Cancel operations
}
```

#### **Backup Manager Form**
```csharp
public partial class BackupManagerForm : Form
{
    // Create backup section
    // Backup history list
    // Restore functionality
    // Backup file validation
}
```

### 6.2 ✨ UI Features

#### **Common UI Components**
- Modern flat design với consistent styling
- Real-time validation feedback
- Progress indicators cho long operations
- Toast notifications
- Confirmation dialogs
- Error handling với user-friendly messages

#### **Data Binding Strategy**
- BindingSource cho data grids
- Two-way binding cho form controls
- Validation attributes
- Custom validation rules

---

## 7. Implementation Plan

### 7.1 🚀 Development Phases

#### **Phase 1: Core Foundation (Week 1)**
- [ ] Setup project structure
- [ ] Implement data models
- [ ] Create registry helper
- [ ] Basic configuration service

#### **Phase 2: Business Services (Week 2)**
- [ ] Complete configuration service
- [ ] Implement validation service
- [ ] Create backup service
- [ ] Add default configuration service

#### **Phase 3: User Interface (Week 3)**
- [ ] Create main dashboard
- [ ] Database configuration form
- [ ] Printer configuration form
- [ ] System configuration form

#### **Phase 4: Advanced Features (Week 4)**
- [ ] Backup manager form
- [ ] External API service
- [ ] Testing và bug fixes
- [ ] Documentation

### 7.2 📊 File Structure

```
POSConfigApp/
├── Models/
│   ├── DatabaseConfig.cs
│   ├── PrinterConfig.cs
│   ├── SystemConfig.cs
│   └── AppConfiguration.cs
├── Services/
│   ├── ConfigurationService.cs
│   ├── BackupService.cs
│   ├── DefaultConfigurationService.cs
│   ├── ExternalApiService.cs
│   └── ValidationService.cs
├── Helpers/
│   ├── RegistryHelper.cs
│   ├── FileHelper.cs
│   └── JsonHelper.cs
├── Forms/
│   ├── MainForm.cs
│   ├── DatabaseConfigForm.cs
│   ├── PrinterConfigForm.cs
│   ├── SystemConfigForm.cs
│   └── BackupManagerForm.cs
├── Utils/
│   ├── Constants.cs
│   ├── Validators.cs
│   └── Extensions.cs
└── Program.cs
```

### 7.3 🎯 Usage Examples

#### **For External Projects**
```csharp
// Get main database connection
var mainDb = ExternalApiService.GetMainDatabase();
var connectionString = mainDb.ConnectionString;

// Get printer for specific zone
var kitchenPrinters = ExternalApiService.GetPrintersByZone("Kitchen");
var printerName = kitchenPrinters.FirstOrDefault()?.PrinterName;

// Get system configuration
var companyName = ExternalApiService.GetSystemConfigValue("CompanyName");
var taxRate = ExternalApiService.GetSystemConfigValue<decimal>("TaxRate");
```

### 7.4 ✅ Success Criteria

- [ ] **Functionality**: All core features working correctly
- [ ] **UI/UX**: Intuitive và user-friendly interface
- [ ] **Performance**: Fast response times cho all operations
- [ ] **Reliability**: No data loss during backup/restore
- [ ] **External API**: Other projects có thể dễ dàng sử dụng
- [ ] **Default Configs**: Automatically create khi cần thiết

---

## 📋 Conclusion

Architecture này được thiết kế đơn giản nhưng đầy đủ chức năng cho việc quản lý cấu hình POS. Điểm mạnh:

### 🎯 **Core Strengths**
- **Simple & Practical**: 3-layer architecture dễ hiểu và maintain
- **Registry Storage**: Native Windows storage, reliable và secure
- **External API**: Easy integration với other projects
- **Auto Default**: Tự động tạo config mặc định
- **Backup/Restore**: Complete data protection

### 📈 **Benefits**
- **Fast Development**: Simple architecture = faster implementation
- **Easy Maintenance**: Clear separation of concerns
- **User Friendly**: Intuitive Windows Forms interface  
- **Integration Ready**: Ready-to-use API cho external projects
- **Data Safety**: Comprehensive backup and restore functionality

Kiến trúc này đảm bảo đáp ứng tất cả yêu cầu business trong khi giữ được tính đơn giản và dễ maintain.