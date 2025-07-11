# Task 2.1.3: Permission Validation

## 📋 Task Overview
**Sprint**: 2  
**Story**: 2.1 - Windows Registry Service  
**Priority**: High  
**Estimated Hours**: 5  
**Assigned To**: Senior Developer  
**Dependencies**: Task 2.1.1 - Registry Operations, Task 2.1.2 - Encryption Service

## 🎯 Objective
Thiết kế và triển khai hệ thống kiểm tra, xác thực quyền truy cập registry, đảm bảo mọi thao tác registry đều được kiểm tra quyền hợp lệ trước khi thực hiện, tăng cường bảo mật, audit và giảm thiểu lỗi truy cập trái phép cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Permission Validation Service**:
  - Kiểm tra quyền truy cập (Read, Write, Delete, Create) trên registry path trước khi thao tác.
  - Xác định và báo cáo rõ quyền còn thiếu nếu không đủ quyền.
  - Hỗ trợ kiểm tra quyền cho cả key và value.
  - Tích hợp kiểm tra quyền vào tất cả các thao tác registry (CRUD, batch, backup/restore).
  - Ghi log khi phát hiện truy cập trái phép hoặc thiếu quyền.
  - Cung cấp API kiểm tra quyền cho các service khác.
  - Audit log mọi lần kiểm tra quyền bị từ chối.

- [ ] **Permission Models**:
  ```csharp
  // SystemConfig.Infrastructure/Models/PermissionCheckResult.cs
  public class PermissionCheckResult
  {
      public bool IsGranted { get; set; }
      public List<RegistryRights> MissingRights { get; set; }
      public string ErrorMessage { get; set; }
      public string CheckedBy { get; set; }
      public DateTime CheckedAt { get; set; }
  }
  ```

- [ ] **Permission Service Interface**:
  ```csharp
  // SystemConfig.Infrastructure/Services/IPermissionService.cs
  public interface IPermissionService
  {
      Task<bool> HasPermissionAsync(string registryPath, RegistryRights rights);
      Task<PermissionCheckResult> ValidatePermissionAsync(string registryPath, RegistryRights rights);
  }
  ```

### Technical Requirements
- [ ] **Permission Checking Logic**:
  - Sử dụng .NET RegistrySecurity và RegistryRights để kiểm tra quyền thực tế của user hiện tại.
  - Xử lý các trường hợp đặc biệt: key không tồn tại, user không xác định, quyền bị deny bởi group policy.
  - Tích hợp kiểm tra quyền vào tất cả các method của RegistryService trước khi thao tác.
  - Ghi log chi tiết khi phát hiện thiếu quyền hoặc truy cập trái phép.
  - Audit log mọi lần kiểm tra quyền bị từ chối (ghi lại user, thời gian, quyền thiếu, thao tác, registry path).

- [ ] **Integration Points**:
  - RegistryService, RegistryBatchService, RegistryBackupService phải gọi kiểm tra quyền trước khi thao tác.
  - Audit log phải ghi lại mọi lần kiểm tra quyền bị từ chối.

- [ ] **Permission Enum**:
  ```csharp
  // System.Security.AccessControl.RegistryRights
  // (dùng sẵn trong .NET, ví dụ: ReadKey, WriteKey, Delete, CreateSubKey, ...)
  ```

### Quality Requirements
- [ ] **Security**: Không cho phép thao tác registry khi thiếu quyền.
- [ ] **Error Handling**: Báo lỗi rõ ràng, log đầy đủ khi thiếu quyền.
- [ ] **Testing**: Unit test và integration test cho các trường hợp granted/denied.
- [ ] **Auditability**: Mọi lần kiểm tra quyền bị từ chối đều được log lại.
- [ ] **Performance**: Kiểm tra quyền không làm chậm thao tác registry.

## 🏗️ Implementation Plan

### Phase 1: Permission Service Implementation (2 hours)
- Thiết kế interface và model cho kiểm tra quyền.
- Implement logic kiểm tra quyền sử dụng .NET RegistrySecurity và RegistryRights.
- Tích hợp kiểm tra quyền vào các thao tác registry (trước khi thực hiện thao tác chính).

### Phase 2: Logging & Audit (1 hour)
- Ghi log khi phát hiện truy cập trái phép hoặc thiếu quyền.
- Trả về thông báo lỗi rõ ràng cho client/service layer.
- Đảm bảo audit log ghi lại mọi lần kiểm tra quyền bị từ chối (user, thời gian, quyền thiếu, thao tác, registry path).

### Phase 3: Testing (2 hours)
- Viết unit test cho các trường hợp:
  - Đủ quyền (IsGranted = true)
  - Thiếu quyền (IsGranted = false, MissingRights != null)
  - Truy cập trái phép (ErrorMessage != null)
- Viết integration test với registry thật (nếu có thể).
- Test performance kiểm tra quyền trên nhiều thao tác liên tục.

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Infrastructure/Services/PermissionServiceTests.cs
public class PermissionServiceTests
{
    private readonly PermissionService _permissionService;
    // ... setup mock registry/context ...

    [Fact]
    public async Task HasPermissionAsync_WithSufficientRights_ShouldReturnTrue()
    {
        // Arrange
        var registryPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\SystemConfig\\Test";
        var rights = RegistryRights.ReadKey;
        // Act
        var result = await _permissionService.HasPermissionAsync(registryPath, rights);
        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasPermissionAsync_WithInsufficientRights_ShouldReturnFalse()
    {
        // Arrange
        var registryPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\SystemConfig\\Test";
        var rights = RegistryRights.WriteKey;
        // Act
        var result = await _permissionService.HasPermissionAsync(registryPath, rights);
        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidatePermissionAsync_WithDeniedAccess_ShouldReturnError()
    {
        // Arrange
        var registryPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\SystemConfig\\Test";
        var rights = RegistryRights.Delete;
        // Act
        var result = await _permissionService.ValidatePermissionAsync(registryPath, rights);
        // Assert
        Assert.False(result.IsGranted);
        Assert.NotNull(result.ErrorMessage);
        Assert.NotEmpty(result.MissingRights);
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Infrastructure/Services/PermissionServiceIntegrationTests.cs
public class PermissionServiceIntegrationTests
{
    private readonly IPermissionService _permissionService;
    private readonly string _testRegistryPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\SystemConfig\\Test";

    public PermissionServiceIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure();
        var serviceProvider = services.BuildServiceProvider();
        _permissionService = serviceProvider.GetRequiredService<IPermissionService>();
    }

    [Fact]
    public async Task PermissionService_ShouldDetectMissingRights()
    {
        // Arrange
        var rights = RegistryRights.FullControl;
        // Act
        var result = await _permissionService.ValidatePermissionAsync(_testRegistryPath, rights);
        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsGranted);
        Assert.NotEmpty(result.MissingRights);
    }
}
```

## 📊 Definition of Done
- [ ] Permission service implement đầy đủ interface và model.
- [ ] Tích hợp kiểm tra quyền vào tất cả thao tác registry.
- [ ] Log đầy đủ khi thiếu quyền hoặc truy cập trái phép.
- [ ] Audit log ghi lại mọi lần kiểm tra quyền bị từ chối.
- [ ] Unit test và integration test pass.
- [ ] Performance kiểm tra quyền đạt yêu cầu.
- [ ] Code review và approve.
- [ ] Documentation hoàn thành.

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Không đủ quyền truy cập registry trên môi trường test/dev.
  - **Mitigation**: Sử dụng mock hoặc test trên registry key riêng biệt.
- **Risk**: Lỗi xác định quyền do cấu trúc registry phức tạp hoặc group policy.
  - **Mitigation**: Bổ sung log chi tiết và kiểm thử nhiều trường hợp thực tế.
- **Risk**: Audit log bị bỏ sót khi kiểm tra quyền.
  - **Mitigation**: Tích hợp kiểm tra log coverage trong CI/CD.
- **Risk**: Performance kiểm tra quyền làm chậm thao tác registry.
  - **Mitigation**: Benchmark và tối ưu code kiểm tra quyền.

### Business Risks
- **Risk**: Quy trình kiểm tra quyền gây khó khăn cho user cuối.
  - **Mitigation**: Thông báo lỗi rõ ràng, hướng dẫn user cấp quyền đúng.

## 📚 Resources & References
- .NET RegistryRights Documentation
- Windows Registry Security Best Practices
- Registry Permission Troubleshooting Guide
- .NET RegistrySecurity Class Reference
- Windows Group Policy and Registry Permissions
- Clean Architecture Security Patterns

## 🔄 Dependencies
- Task 2.1.1: Registry Operations
- Task 2.1.2: Encryption Service

## 📈 Success Metrics
- 100% thao tác registry đều kiểm tra quyền trước khi thực hiện.
- Test coverage >95% cho permission service.
- Không có lỗi truy cập trái phép trong log thực tế.
- Audit log ghi nhận đầy đủ các lần kiểm tra quyền bị từ chối.
- Performance kiểm tra quyền < 10ms/truy vấn.

## 📝 Notes
- Ưu tiên kiểm tra quyền trước khi thực hiện bất kỳ thao tác registry nào.
- Ghi log chi tiết để hỗ trợ audit và troubleshooting.
- Đảm bảo audit log không chứa thông tin nhạy cảm không cần thiết.
- Có thể mở rộng kiểm tra quyền cho các resource khác ngoài registry trong tương lai. 