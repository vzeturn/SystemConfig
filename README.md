# SystemConfig - POS Multi-Store Configuration System

A modern .NET 8 Windows Forms standalone application for managing multi-store POS configurations with database management, printer configuration, and flexible system settings. The application runs entirely on the client machine without requiring any server infrastructure.

## Features

### Core Features
- **Multi-Database Management**: Connect to multiple database masters
- **Smart Printer Configuration**: Configure printers by food category
- **Flexible System Configuration**: User-defined system settings
- **Backup & Restore**: Configuration backup and restore functionality
- **Registry-based Storage**: Windows Registry storage for configurations
- **Modern UI/UX**: Clean and responsive Windows Forms interface

### Advanced Features
- **Encryption Service**: AES-256 encryption for sensitive data
- **Performance Monitoring**: Real-time system health and metrics
- **Notification System**: Multi-channel notification and alert management
- **Standalone Application**: No server required - runs entirely on client machine
- **Windows Integration**: Native Windows Registry and system integration
- **Local Data Storage**: All data stored locally on the client machine

## Architecture

The solution follows Clean Architecture principles with three main layers for a standalone Windows application:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
├─────────────────────────────────────────────────────────────┤
│  Windows Forms Application                                 │
│  - MainForm (Database Selection)                          │
│  - DatabaseConfigurationForm                              │
│  - PrinterConfigurationForm                               │
│  - SystemConfigurationForm                                │
│  - BackupManagementForm                                   │
│  - PerformanceDashboardForm                               │
│  - NotificationForm                                       │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
├─────────────────────────────────────────────────────────────┤
│  Services       │  Interfaces  │  DTOs           │         │
│  (Business)     │  (Contracts) │  (Data Transfer)│         │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                     │
├─────────────────────────────────────────────────────────────┤
│  Registry       │  Encryption   │  Performance    │         │
│  Services       │  Services     │  Monitoring     │         │
│  (Local)        │  (Local)      │  (Local)        │         │
└─────────────────────────────────────────────────────────────┘
```

## Projects Structure

```
SystemConfig/
├── src/
│   ├── SystemConfig.Domain/           # Domain entities and enums
│   ├── SystemConfig.Application/      # Application services and interfaces
│   ├── SystemConfig.Infrastructure/  # Infrastructure implementations
│   └── SystemConfig.Presentation/    # Windows Forms application
├── tests/
│   └── SystemConfig.Tests/           # Unit tests
├── scripts/                         # Management scripts
└── docs/                           # Documentation
```

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Windows 10/11
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/systemconfig.git
   cd systemconfig
   ```

2. **Build and run the application**
   ```bash
   # Build the solution
   dotnet build
   
   # Run the application
   dotnet run --project src/SystemConfig.Presentation
   ```

3. **Using PowerShell Management Script**
   ```powershell
   # Install system
   .\scripts\manage-system.ps1 install
   
   # Start system
   .\scripts\manage-system.ps1 start
   
   # Check status
   .\scripts\manage-system.ps1 status
   ```

## Usage

### Database Configuration
- Add multiple database connections
- Test connections before saving
- Set active database configuration
- Encrypt sensitive connection strings

### Printer Configuration
- Configure printers by food category
- Set default printers
- Test printer connections
- Manage print templates

### System Configuration
- Create custom configuration keys
- Set default values and validation rules
- Organize by categories
- Support multiple data types

### Backup & Restore
- Create full or incremental backups
- Export/import configurations
- Automatic backup scheduling
- Integrity validation

### Performance Monitoring
- Real-time system health monitoring
- Performance metrics collection
- Error tracking and logging
- Resource usage monitoring

### Notification System
- Multi-channel notifications (Email, Desktop, In-app)
- System alerts with severity levels
- Configurable notification preferences
- Quiet hours support
- Notification statistics and analytics
- User subscription management

## Management Scripts

The PowerShell management script provides comprehensive system management:

```powershell
# System management
.\scripts\manage-system.ps1 start
.\scripts\manage-system.ps1 stop
.\scripts\manage-system.ps1 restart
.\scripts\manage-system.ps1 status

# Monitoring and logs
.\scripts\manage-system.ps1 logs
.\scripts\manage-system.ps1 health
.\scripts\manage-system.ps1 config

# Backup and restore
.\scripts\manage-system.ps1 backup my-backup
.\scripts\manage-system.ps1 restore my-backup

# System maintenance
.\scripts\manage-system.ps1 update
.\scripts\manage-system.ps1 install
.\scripts\manage-system.ps1 uninstall -Force
```

## Development

### Running Tests
```bash
dotnet test
```

### Code Quality
```bash
dotnet format
dotnet build --configuration Release
```

### Building Application
```powershell
# Build with tests
.\scripts\build.ps1 -Test

# Build with package creation
.\scripts\build.ps1 -Package

# Clean build
.\scripts\build.ps1 -Clean
```

## Configuration

### Registry Structure
```
HKEY_LOCAL_MACHINE\SOFTWARE\SystemConfig\
├── Database\
│   ├── {ConfigId1}\
│   ├── {ConfigId2}\
│   └── ...
├── Printer\
│   ├── {ConfigId1}\
│   ├── {ConfigId2}\
│   └── ...
├── System\
│   ├── {ConfigId1}\
│   ├── {ConfigId2}\
│   └── ...
├── Backup\
│   ├── {BackupId1}\
│   ├── {BackupId2}\
│   └── ...
└── Notifications\
    ├── {UserId1}\
    ├── {UserId2}\
    └── ...
```

### Application Data Locations
- **Installation**: `%ProgramFiles%\SystemConfig\`
- **Logs**: `%LOCALAPPDATA%\SystemConfig\logs\`
- **Configuration**: `%LOCALAPPDATA%\SystemConfig\config\`
- **Backups**: `%LOCALAPPDATA%\SystemConfig\backups\`

## Security

### Data Protection
- AES-256 encryption for sensitive data
- Windows DPAPI for key management
- Secure connection string handling
- Audit logging for configuration changes

### Access Control
- Windows Authentication integration
- Registry permission validation
- Configuration change tracking
- Local data storage only

## Deployment

### Windows Forms Application
- Standalone executable
- .NET 8.0 runtime dependency
- Registry setup and permissions
- Local data storage

### Distribution Options
- **Direct Copy**: Copy executable and dependencies
- **PowerShell Script**: Automated installation
- **ZIP Package**: Portable distribution
- **Windows Installer**: Professional installation

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions:
- Create an issue in the repository
- Check the documentation
- Review the logs using `.\scripts\manage-system.ps1 logs` 