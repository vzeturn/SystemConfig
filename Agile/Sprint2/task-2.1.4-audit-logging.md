# Task 2.1.4: Audit Logging

## 📋 Task Overview
**Sprint**: 2  
**Story**: 2.1 - Windows Registry Service  
**Priority**: High  
**Estimated Hours**: 5  
**Assigned To**: Senior Developer  
**Dependencies**: Task 2.1.1 - Registry Operations, Task 2.1.3 - Permission Validation

## 🎯 Objective
Thiết kế và triển khai hệ thống audit logging cho mọi thao tác registry, đảm bảo ghi nhận đầy đủ lịch sử thao tác (ai, khi nào, thao tác gì, thành công/thất bại, giá trị cũ/mới), hỗ trợ truy vết, bảo mật và tuân thủ quy định cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Audit Logging Service**:
  - Ghi nhận đầy đủ lịch sử thao tác registry (CRUD, batch, backup/restore, permission check).
  - Lưu thông tin: user, thời gian, thao tác, registry path, giá trị cũ/mới, trạng thái thành công/thất bại, lỗi nếu có.
  - Hỗ trợ truy vấn, lọc, xóa log theo registry path, user, thời gian, loại thao tác.
  - Đảm bảo log không chứa dữ liệu nhạy cảm (mask nếu cần).
  - Tích hợp audit logging vào tất cả các thao tác registry và permission validation.
  - Cung cấp API cho các service khác truy vấn/xóa log.

- [ ] **Audit Log Model**:
  ```csharp
  // SystemConfig.Infrastructure/Models/RegistryAuditLog.cs
  public class RegistryAuditLog
  {
      public Guid Id { get; set; }
      public string RegistryPath { get; set; }
      public string Operation { get; set; }
      public string UserId { get; set; }
      public DateTime Timestamp { get; set; }
      public string OldValue { get; set; }
      public string NewValue { get; set; }
      public bool IsSuccessful { get; set; }
      public string ErrorMessage { get; set; }
  }
  ```

- [ ] **Audit Logging Service Interface**:
  ```csharp
  // SystemConfig.Infrastructure/Services/IAuditLoggingService.cs
  public interface IAuditLoggingService
  {
      Task LogAsync(RegistryAuditLog log);
      Task<IEnumerable<RegistryAuditLog>> QueryAsync(string registryPath = null, string userId = null, string operation = null, DateTime? from = null, DateTime? to = null);
      Task ClearAsync(string registryPath = null, string userId = null, string operation = null);
  }
  ```

### Technical Requirements
- [ ] **Audit Log Storage**:
  - Lưu log vào file, database hoặc registry key riêng biệt (tùy cấu hình).
  - Đảm bảo hiệu năng và khả năng mở rộng khi log lớn.
  - Hỗ trợ retention policy (giới hạn số lượng/ngày lưu log).

- [ ] **Integration Points**:
  - RegistryService, RegistryBatchService, RegistryBackupService, PermissionService phải gọi ghi log cho mọi thao tác.
  - Log cả thao tác thành công và thất bại.

- [ ] **Security & Compliance**:
  - Mask dữ liệu nhạy cảm (password, key) trong log.
  - Đảm bảo log không bị sửa/xóa ngoài quy trình cho phép.
  - Hỗ trợ export log phục vụ audit external.

### Quality Requirements
- [ ] **Completeness**: 100% thao tác registry đều sinh log.
- [ ] **Performance**: Logging không làm chậm thao tác registry.
- [ ] **Security**: Không log dữ liệu nhạy cảm.
- [ ] **Auditability**: Log có thể truy vấn, export, xóa theo policy.
- [ ] **Testability**: Dễ kiểm thử, mock, kiểm tra log coverage.

## 🏗️ Implementation Plan

### Phase 1: Audit Log Model & Storage (1.5 hours)
- Thiết kế model RegistryAuditLog.
- Lựa chọn phương thức lưu log (file/db/registry key), implement storage provider.
- Hỗ trợ retention policy.

### Phase 2: Audit Logging Service Implementation (1.5 hours)
- Implement IAuditLoggingService với các method LogAsync, QueryAsync, ClearAsync.
- Tích hợp vào RegistryService, PermissionService, các thao tác registry.
- Mask dữ liệu nhạy cảm khi log.

### Phase 3: API & Integration (1 hour)
- Cung cấp API cho các service khác truy vấn/xóa log.
- Hỗ trợ export log ra file phục vụ audit external.

### Phase 4: Testing (1 hour)
- Viết unit test cho các case log thành công/thất bại, mask dữ liệu, retention policy.
- Viết integration test kiểm tra log coverage khi thao tác registry.

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Infrastructure/Services/AuditLoggingServiceTests.cs
public class AuditLoggingServiceTests
{
    private readonly AuditLoggingService _auditLoggingService;
    // ... setup mock storage ...

    [Fact]
    public async Task LogAsync_ShouldPersistLog()
    {
        // Arrange
        var log = new RegistryAuditLog { RegistryPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\SystemConfig\\Test", Operation = "SetValue", UserId = "user1", Timestamp = DateTime.UtcNow, IsSuccessful = true };
        // Act
        await _auditLoggingService.LogAsync(log);
        // Assert
        var logs = await _auditLoggingService.QueryAsync(log.RegistryPath);
        Assert.Contains(logs, l => l.Operation == "SetValue");
    }

    [Fact]
    public async Task LogAsync_ShouldMaskSensitiveData()
    {
        // Arrange
        var log = new RegistryAuditLog { OldValue = "password=123456", NewValue = "password=abcdef" };
        // Act
        await _auditLoggingService.LogAsync(log);
        // Assert
        var logs = await _auditLoggingService.QueryAsync();
        Assert.All(logs, l => Assert.DoesNotContain("123456", l.OldValue));
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Infrastructure/Services/AuditLoggingIntegrationTests.cs
public class AuditLoggingIntegrationTests
{
    private readonly IAuditLoggingService _auditLoggingService;
    public AuditLoggingIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure();
        var serviceProvider = services.BuildServiceProvider();
        _auditLoggingService = serviceProvider.GetRequiredService<IAuditLoggingService>();
    }

    [Fact]
    public async Task AuditLogging_ShouldLogAllRegistryOperations()
    {
        // Arrange
        // ... perform registry operations ...
        // Act
        var logs = await _auditLoggingService.QueryAsync();
        // Assert
        Assert.NotEmpty(logs);
    }
}
```

## 📊 Definition of Done
- [ ] Audit logging service implement đầy đủ interface và model.
- [ ] 100% thao tác registry đều sinh log.
- [ ] Log đầy đủ, mask dữ liệu nhạy cảm.
- [ ] API truy vấn/xóa/export log hoạt động.
- [ ] Retention policy hoạt động đúng.
- [ ] Unit test và integration test pass.
- [ ] Code review và approve.
- [ ] Documentation hoàn thành.

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Log file/database/registry key quá lớn gây chậm hệ thống.
  - **Mitigation**: Áp dụng retention policy, log rotation, archive.
- **Risk**: Log chứa dữ liệu nhạy cảm.
  - **Mitigation**: Mask dữ liệu trước khi ghi log, review log format.
- **Risk**: Log bị sửa/xóa ngoài quy trình.
  - **Mitigation**: Phân quyền truy cập log, lưu log immutable nếu cần.
- **Risk**: Logging làm chậm thao tác registry.
  - **Mitigation**: Ghi log async, tối ưu storage provider.

### Business Risks
- **Risk**: Log không đủ chi tiết cho audit external.
  - **Mitigation**: Review định kỳ với auditor, bổ sung trường log nếu cần.

## 📚 Resources & References
- Windows Registry Audit Logging Best Practices
- .NET Logging Patterns
- Security Audit Guidelines
- Data Masking Techniques
- Clean Architecture Logging Patterns

## 🔄 Dependencies
- Task 2.1.1: Registry Operations
- Task 2.1.3: Permission Validation

## 📈 Success Metrics
- 100% thao tác registry đều sinh log.
- Không log dữ liệu nhạy cảm.
- Log truy vấn/export/xóa đúng policy.
- Performance logging < 10ms/thao tác.
- Test coverage >95% cho audit logging service.

## 📝 Notes
- Audit log là bắt buộc cho mọi thao tác registry.
- Có thể mở rộng lưu log ra external SIEM hoặc cloud storage trong tương lai.
- Đảm bảo log không bị ghi đè hoặc mất dữ liệu ngoài retention policy. 