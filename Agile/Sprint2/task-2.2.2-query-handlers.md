# Task 2.2.2: Query Handlers Implementation

## 📋 Task Overview
**Sprint**: 2  
**Story**: 2.2 - CQRS với MediatR  
**Priority**: High  
**Estimated Hours**: 5  
**Assigned To**: Application Developer  
**Dependencies**: Task 1.1.1 - Create Solution Structure, Task 1.1.2 - Setup Dependency Injection

## 🎯 Objective
Thiết kế và triển khai các query handler cho thao tác đọc (read) theo CQRS pattern sử dụng MediatR, đảm bảo separation of concerns, performance, validation và testability cho POS Multi-Store Configuration Solution.

## 📝 Detailed Requirements

### Functional Requirements
- [ ] **Query Handlers**:
  - Thiết kế các query: GetConfigByIdQuery, ListConfigsQuery, ...
  - Implement handler cho từng query, mỗi handler xử lý một use case đọc dữ liệu.
  - Hỗ trợ filter, paging, sorting, mapping sang DTO.
  - Đảm bảo không có logic ghi (write) trong query handler.
  - Tích hợp caching nếu cần.

- [ ] **Query & Handler Structure**:
  ```csharp
  // SystemConfig.Application/Queries/GetConfigByIdQuery.cs
  public class GetConfigByIdQuery : IRequest<ConfigDto>
  {
      public Guid Id { get; set; }
  }

  // SystemConfig.Application/Queries/Handlers/GetConfigByIdQueryHandler.cs
  public class GetConfigByIdQueryHandler : IRequestHandler<GetConfigByIdQuery, ConfigDto>
  {
      private readonly IRepository<Config> _repository;
      public GetConfigByIdQueryHandler(IRepository<Config> repository)
      {
          _repository = repository;
      }
      public async Task<ConfigDto> Handle(GetConfigByIdQuery request, CancellationToken cancellationToken)
      {
          var entity = await _repository.GetByIdAsync(request.Id);
          if (entity == null) throw new NotFoundException();
          return new ConfigDto { ... };
      }
  }
  ```

### Technical Requirements
- [ ] **CQRS & MediatR Integration**:
  - Sử dụng MediatR cho query dispatching.
  - Đăng ký handler qua DI container.
  - Hỗ trợ filter, paging, sorting, mapping sang DTO.
  - Không có logic ghi trong query handler.

- [ ] **Performance & Caching**:
  - Tích hợp caching cho các query tần suất cao.
  - Đảm bảo query handler trả về kết quả nhanh.

### Quality Requirements
- [ ] **Separation of Concerns**: Query handler chỉ xử lý read logic.
- [ ] **Validation**: Validate input query.
- [ ] **Testability**: Dễ mock, unit test từng handler.
- [ ] **Performance**: Xử lý nhanh, hỗ trợ paging/filter.

## 🏗️ Implementation Plan

### Phase 1: Query & Handler Design (1.5 hours)
- Thiết kế các query cho từng use case đọc dữ liệu.
- Định nghĩa handler, inject dependencies.

### Phase 2: MediatR & DI Integration (1 hour)
- Đăng ký handler với MediatR qua DI.
- Tích hợp validation, caching nếu cần.

### Phase 3: Performance & Caching (1 hour)
- Tối ưu query, implement caching cho query tần suất cao.

### Phase 4: Testing (1.5 hours)
- Viết unit test cho từng handler (success, not found, invalid input).
- Test filter, paging, caching.

## 🧪 Testing Strategy

### Unit Tests
```csharp
// SystemConfig.UnitTests/Application/Queries/GetConfigByIdQueryHandlerTests.cs
public class GetConfigByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithValidId_ShouldReturnConfig()
    {
        // Arrange
        var handler = new GetConfigByIdQueryHandler(...);
        var query = new GetConfigByIdQuery { Id = Guid.NewGuid() };
        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        // Assert
        Assert.NotNull(result);
    }
    [Fact]
    public async Task Handle_WithInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var handler = new GetConfigByIdQueryHandler(...);
        var query = new GetConfigByIdQuery { Id = Guid.NewGuid() };
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(query, CancellationToken.None));
    }
}
```

### Integration Tests
```csharp
// SystemConfig.IntegrationTests/Application/Queries/QueryHandlersIntegrationTests.cs
public class QueryHandlersIntegrationTests
{
    [Fact]
    public async Task ListConfigsQuery_ShouldReturnPagedResult()
    {
        // Arrange
        // ... setup DI, MediatR, repo ...
        // Act
        // ... send query ...
        // Assert
        // ... verify paging/filter ...
    }
}
```

## 📊 Definition of Done
- [ ] Query handler implement đầy đủ cho tất cả use case đọc dữ liệu.
- [ ] Hỗ trợ filter, paging, sorting, mapping sang DTO.
- [ ] Không có logic ghi trong handler.
- [ ] Unit test và integration test pass.
- [ ] Code review và approve.
- [ ] Documentation hoàn thành.

## 🚨 Risks & Mitigation

### Technical Risks
- **Risk**: Query handler chứa logic ghi.
  - **Mitigation**: Review code, enforce CQRS separation.
- **Risk**: Query performance kém với dữ liệu lớn.
  - **Mitigation**: Tối ưu query, implement caching.
- **Risk**: Handler khó test do phụ thuộc nhiều service.
  - **Mitigation**: Sử dụng DI, mock dependencies.

### Business Risks
- **Risk**: Query validation không đủ chặt.
  - **Mitigation**: Bổ sung validation rule, test case.

## 📚 Resources & References
- CQRS with MediatR Best Practices
- Clean Architecture Query Handler Patterns
- .NET MediatR Documentation
- Repository & Paging Patterns

## 🔄 Dependencies
- Task 1.1.1: Create Solution Structure
- Task 1.1.2: Setup Dependency Injection

## 📈 Success Metrics
- 100% use case đọc dữ liệu đều qua query handler.
- Test coverage >95% cho query handler.
- Không có logic ghi trong handler.
- Paging/filter hoạt động đúng.

## 📝 Notes
- Tách biệt rõ query handler và command handler.
- Đảm bảo handler dễ test, dễ mở rộng.
- Có thể mở rộng cho các entity khác trong tương lai. 