# Task 2.2.1: Command Handlers Implementation

## 📋 Task Overview
**Sprint**: 2  
**Story**: 2.2 - CQRS với MediatR  
**Priority**: High  
**Estimated Hours**: 6  
**Assigned To**: Application Developer  
**Dependencies**: Task 1.1.1 - Create Solution Structure, Task 1.1.2 - Setup Dependency Injection

## 🎯 Objective
Thiết kế và triển khai các command handler cho thao tác ghi (write) theo CQRS pattern sử dụng MediatR, đảm bảo separation of concerns, validation, logging và testability cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Command Handlers**:
  - Thiết kế các command: CreateConfigCommand, UpdateConfigCommand, DeleteConfigCommand, ...
  - Implement handler cho từng command, mỗi handler xử lý một use case ghi dữ liệu.
  - Đảm bảo validation, logging, transaction, publish domain events.
  - Hỗ trợ rollback khi thao tác thất bại.
  - Tích hợp với unit of work và repository pattern.

- [ ] **Command & Handler Structure**:
  ```csharp
  // SystemConfig.Application/Commands/CreateConfigCommand.cs
  public class CreateConfigCommand : IRequest<ConfigDto>
  {
      public string Name { get; set; }
      public string Type { get; set; }
      public string Value { get; set; }
      // ... các trường khác ...
  }

  // SystemConfig.Application/Commands/Handlers/CreateConfigCommandHandler.cs
  public class CreateConfigCommandHandler : IRequestHandler<CreateConfigCommand, ConfigDto>
  {
      private readonly IRepository<Config> _repository;
      private readonly IUnitOfWork _unitOfWork;
      private readonly IValidationService _validationService;
      private readonly ILoggingService _loggingService;
      public CreateConfigCommandHandler(...)
      {
          // inject dependencies
      }
      public async Task<ConfigDto> Handle(CreateConfigCommand request, CancellationToken cancellationToken)
      {
          // Validate
          await _validationService.ValidateAsync(request);
          // Map to entity
          var entity = new Config { ... };
          await _repository.AddAsync(entity);
          await _unitOfWork.SaveChangesAsync();
          _loggingService.LogInformation("Created config: {Name}", request.Name);
          // Map to DTO
          return new ConfigDto { ... };
      }
  }
  ```

### Technical Requirements
- [ ] **CQRS & MediatR Integration**:
  - Sử dụng MediatR cho command dispatching.
  - Đăng ký handler qua DI container.
  - Tích hợp validation pipeline, logging, transaction.
  - Đảm bảo command handler không chứa logic truy vấn (read).

- [ ] **Error Handling & Transaction**:
  - Rollback khi thao tác thất bại.
  - Log lỗi chi tiết.

- [ ] **Domain Events**:
  - Publish domain events sau khi thao tác thành công.

### Quality Requirements
- [ ] **Separation of Concerns**: Command handler chỉ xử lý write logic.
- [ ] **Validation**: Tích hợp validation pipeline.
- [ ] **Testability**: Dễ mock, unit test từng handler.
- [ ] **Performance**: Xử lý nhanh, không block UI.

## 🏗️ Implementation Plan

### Phase 1: Command & Handler Design (2 hours)
- Thiết kế các command cho từng use case ghi dữ liệu.
- Định nghĩa handler, inject dependencies.

### Phase 2: MediatR & DI Integration (1 hour)
- Đăng ký handler với MediatR qua DI.
- Tích hợp validation, logging, transaction.

### Phase 3: Error Handling & Domain Events (1 hour)
- Implement rollback khi lỗi.
- Publish domain events sau thao tác thành công.

### Phase 4: Testing (2 hours)
- Viết unit test cho từng handler (success, validation fail, exception).
- Test rollback, event publish.

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Application/Commands/CreateConfigCommandHandlerTests.cs
public class CreateConfigCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateConfig()
    {
        // Arrange
        var handler = new CreateConfigCommandHandler(...);
        var command = new CreateConfigCommand { Name = "Test", ... };
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
    }
    [Fact]
    public async Task Handle_WithInvalidCommand_ShouldThrowValidationException()
    {
        // Arrange
        var handler = new CreateConfigCommandHandler(...);
        var command = new CreateConfigCommand { Name = "" };
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => handler.Handle(command, CancellationToken.None));
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Application/Commands/CommandHandlersIntegrationTests.cs
public class CommandHandlersIntegrationTests
{
    [Fact]
    public async Task CreateConfigCommand_ShouldPersistConfig()
    {
        // Arrange
        // ... setup DI, MediatR, repo ...
        // Act
        // ... send command ...
        // Assert
        // ... verify config persisted ...
    }
}
```

## 📊 Definition of Done
- [ ] Command handler implement đầy đủ cho tất cả use case ghi dữ liệu.
- [ ] Tích hợp validation, logging, transaction.
- [ ] Publish domain events đúng chuẩn.
- [ ] Unit test và integration test pass.
- [ ] Code review và approve.
- [ ] Documentation hoàn thành.

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Command handler chứa logic truy vấn.
  - **Mitigation**: Review code, enforce CQRS separation.
- **Risk**: Rollback không đúng khi lỗi.
  - **Mitigation**: Test kỹ rollback, transaction.
- **Risk**: Handler khó test do phụ thuộc nhiều service.
  - **Mitigation**: Sử dụng DI, mock dependencies.

### Business Risks
- **Risk**: Command validation không đủ chặt.
  - **Mitigation**: Bổ sung validation rule, test case.

## 📚 Resources & References
- CQRS with MediatR Best Practices
- Clean Architecture Command Handler Patterns
- .NET MediatR Documentation
- Unit of Work & Repository Patterns

## 🔄 Dependencies
- Task 1.1.1: Create Solution Structure
- Task 1.1.2: Setup Dependency Injection

## 📈 Success Metrics
- 100% use case ghi dữ liệu đều qua command handler.
- Test coverage >95% cho command handler.
- Không có logic truy vấn trong handler.
- Rollback và event publish hoạt động đúng.

## 📝 Notes
- Tách biệt rõ command handler và query handler.
- Đảm bảo handler dễ test, dễ mở rộng.
- Có thể mở rộng cho các entity khác trong tương lai. 