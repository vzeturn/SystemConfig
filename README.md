# SystemConfig - Mini Clean Architecture (.NET 8)

## 1. Cấu trúc thư mục tối giản

```
SystemConfig.sln
├── src/
│   ├── Presentation/           # Windows Forms UI
│   ├── Application/            # CQRS, workflow
│   ├── Domain/                 # Entities, ValueObjects, Business Rules
│   └── Infrastructure/         # Registry, Encryption, Logging
├── tests/
│   └── SystemConfig.Tests/     # Unit & Integration tests
├── README.md
```

## 2. Khởi tạo dự án tự động

Chạy script sau bằng PowerShell (Windows):

```powershell
# Tạo solution và các project
mkdir src; mkdir tests
cd src

dotnet new winforms -n Presentation --framework net8.0-windows
cd ..
cd src; dotnet new classlib -n Application --framework net8.0; cd ..
cd src; dotnet new classlib -n Domain --framework net8.0; cd ..
cd src; dotnet new classlib -n Infrastructure --framework net8.0; cd ..
cd tests; dotnet new xunit -n SystemConfig.Tests --framework net8.0; cd ..

dotnet new sln -n SystemConfig

dotnet sln add src/Presentation/Presentation.csproj
"src/Application/Application.csproj", "src/Domain/Domain.csproj", "src/Infrastructure/Infrastructure.csproj", "tests/SystemConfig.Tests/SystemConfig.Tests.csproj" | ForEach-Object { dotnet sln add $_ }

# Thêm project reference
cd src
cd Presentation; dotnet add reference ../Application/Application.csproj; dotnet add reference ../Infrastructure/Infrastructure.csproj; cd ..
cd Application; dotnet add reference ../Domain/Domain.csproj; cd ..
cd Infrastructure; dotnet add reference ../Domain/Domain.csproj; cd ../..
cd tests/SystemConfig.Tests; dotnet add reference ../../src/Domain/Domain.csproj; dotnet add reference ../../src/Application/Application.csproj; cd ../..

# Quay lại thư mục gốc
cd ..
```

## 3. Build & Run

```powershell
dotnet build SystemConfig.sln
dotnet run --project src/Presentation/Presentation.csproj
```

## 4. Test

```powershell
dotnet test tests/SystemConfig.Tests/SystemConfig.Tests.csproj
```

## 5. Ghi chú
- Có thể thêm các base class mẫu (AggregateRoot, ValueObject, IRepository, ...)
- Có thể mở rộng thêm các project test nếu cần.
- Đảm bảo .NET 8 SDK đã cài đặt. 