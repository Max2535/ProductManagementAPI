# Product Management API

ระบบ API สำหรับจัดการข้อมูลสินค้าแบบ E-commerce พัฒนาด้วย .NET 9.0 โดยใช้หลักการ Clean Architecture, Domain-Driven Design และ SOLID Principles พร้อมระบบ Authentication, Orders Management, Background Jobs และ Message Queue

## ✨ Features

### 🏗️ Architecture & Design Patterns
- ✅ **Clean Architecture** - แบ่งชั้นการทำงานอย่างชัดเจน (Domain, Application, Infrastructure, API)
- ✅ **Domain-Driven Design** - Rich Domain Models พร้อม Business Logic
- ✅ **Repository & Unit of Work Pattern** - จัดการ Data Access อย่างมีประสิทธิภาพ
- ✅ **CQRS Ready** - แยก Commands และ Queries อย่างชัดเจน
- ✅ **Event-Driven Architecture** - ใช้ RabbitMQ สำหรับ asynchronous communication

### 🔐 Security & Authentication
- ✅ **JWT Authentication** - Token-based authentication พร้อม Refresh Token
- ✅ **Role-Based Authorization** - จัดการสิทธิ์ Admin, Manager, User
- ✅ **Password Hashing** - SHA-256 hashing สำหรับรหัสผ่าน
- ✅ **Rate Limiting** - ป้องกัน API abuse ด้วย AspNetCoreRateLimit
- ✅ **CORS Configuration** - รองรับ Cross-Origin Requests

### 📦 Core Features
- ✅ **Product Management** - CRUD สินค้า, Stock Management, Discount System
- ✅ **Order Management** - สร้างคำสั่งซื้อ, จัดการสถานะ, Order Items
- ✅ **User Management** - Register, Login, Profile, Password Management
- ✅ **Category System** - จัดการหมวดหมู่สินค้า
- ✅ **Review System** - รีวิวและให้คะแนนสินค้า

### 🚀 Performance & Scalability
- ✅ **Redis Caching** - StackExchange.Redis สำหรับ distributed caching
- ✅ **Dapper** - High-Performance queries สำหรับ read operations
- ✅ **Connection Resilience** - Auto-retry และ circuit breaker patterns
- ✅ **Pagination** - ทุก list endpoint รองรับ pagination

### 🔄 Background Processing
- ✅ **Hangfire** - Background job processing และ scheduled tasks
- ✅ **Stock Alert Jobs** - ตรวจสอบสินค้าใกล้หมดอัตโนมัติ
- ✅ **Cleanup Jobs** - ลบ expired tokens และข้อมูลเก่าอัตโนมัติ
- ✅ **RabbitMQ Integration** - Message queue สำหรับ event handling

### 📧 Communication
- ✅ **Email Service** - ส่ง email ด้วย SMTP (Gmail support)
- ✅ **Email Templates** - Welcome emails, Low stock alerts
- ✅ **RabbitMQ Message Broker** - Event publishing และ consuming
- ✅ **SignalR Ready** - เตรียมพร้อมสำหรับ real-time features

### 🛠️ Development Tools
- ✅ **FluentValidation** - Validation rules ที่อ่านง่ายและยืดหยุ่น
- ✅ **AutoMapper** - Object-to-object mapping อัตโนมัติ
- ✅ **Serilog** - Structured logging พร้อม file และ console output
- ✅ **Swagger/OpenAPI** - Interactive API documentation พร้อม JWT support
- ✅ **Entity Framework Core** - Code-first migrations
- ✅ **Health Checks** - Database health monitoring

## 📁 Project Structure

```
ProductManagementAPI/
├── src/
│   ├── ProductManagement.API/              # 🌐 API Layer
│   │   ├── Controllers/                     # REST API Controllers
│   │   │   ├── ProductsController.cs       # Products CRUD endpoints
│   │   │   ├── OrdersController.cs         # Orders management endpoints
│   │   │   ├── AuthController.cs           # Authentication endpoints
│   │   │   ├── EmailController.cs          # Email testing endpoints
│   │   │   └── JobsController.cs           # Background jobs endpoints
│   │   ├── Middleware/                      # Custom middleware
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Authorization/                   # Authorization handlers
│   │   │   ├── AuthorizeRolesAttribute.cs
│   │   │   ├── RoleRequirement.cs
│   │   │   └── RoleRequirementHandler.cs
│   │   ├── Attributes/
│   │   │   └── CachedAttribute.cs          # Caching attribute
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── Program.cs                       # Application entry point & DI
│   │   ├── appsettings.json                # Production configuration
│   │   ├── appsettings.Development.json    # Development configuration
│   │   ├── Dockerfile                       # Docker configuration
│   │   └── ProductManagement.API.http      # HTTP request samples
│   │
│   ├── ProductManagement.Application/      # 💼 Application Layer
│   │   ├── Services/                        # Business services
│   │   │   ├── ProductService.cs           # Product business logic
│   │   │   ├── OrderService.cs             # Order business logic
│   │   │   ├── AuthService.cs              # Authentication logic
│   │   │   ├── TokenService.cs             # JWT token generation
│   │   │   ├── EmailService.cs             # Email sending
│   │   │   ├── PasswordHasher.cs           # Password hashing
│   │   │   └── HangfireBackgroundJobService.cs
│   │   ├── BackgroundJobs/                  # Hangfire jobs
│   │   │   ├── ProductStockAlertJob.cs     # Stock monitoring
│   │   │   └── CleanupExpiredTokensJob.cs  # Token cleanup
│   │   ├── DTOs/                            # Data Transfer Objects
│   │   ├── Validators/                      # FluentValidation rules
│   │   ├── Mappings/                        # AutoMapper profiles
│   │   ├── Models/                          # Configuration models
│   │   │   ├── JwtSettings.cs
│   │   │   ├── EmailSettings.cs
│   │   │   └── RabbitMQSettings.cs
│   │   └── Interfaces/                      # Service interfaces
│   │
│   ├── ProductManagement.Domain/           # 🏛️ Domain Layer
│   │   ├── Entities/
│   │   │   ├── Product.cs                  # Product entity with business logic
│   │   │   ├── Order.cs                    # Order entity
│   │   │   ├── OrderItem.cs                # Order item entity
│   │   │   ├── User.cs                     # User entity
│   │   │   ├── Role.cs                     # Role entity
│   │   │   ├── UserRole.cs                 # User-Role mapping
│   │   │   ├── Category.cs                 # Category entity
│   │   │   ├── Review.cs                   # Review entity
│   │   │   ├── ProductImage.cs             # Product image entity
│   │   │   └── BaseEntity.cs               # Base entity class
│   │   └── Interfaces/
│   │       ├── IRepository.cs              # Generic repository
│   │       ├── IProductRepository.cs       # Product repository
│   │       ├── IOrderRepository.cs         # Order repository
│   │       ├── IUserRepository.cs          # User repository
│   │       └── IUnitOfWork.cs              # Unit of Work
│   │
│   └── ProductManagement.Infrastructure/   # 🔧 Infrastructure Layer
│       ├── Data/
│       │   ├── ApplicationDbContext.cs     # EF Core DbContext
│       │   └── UnitOfWork.cs               # Unit of Work implementation
│       ├── Repositories/                    # Repository implementations
│       │   ├── Repository.cs               # Generic repository
│       │   ├── ProductRepository.cs
│       │   ├── OrderRepository.cs
│       │   └── UserRepository.cs
│       ├── Services/                        # Infrastructure services
│       │   ├── RedisCacheService.cs        # Redis caching
│       │   ├── RabbitMQMessagePublisher.cs # Message publishing
│       │   └── RabbitMQMessageConsumer.cs  # Message consuming
│       └── Migrations/                      # EF Core migrations
│
├── tests/
│   ├── ProductManagement.UnitTests/        # 🧪 Unit tests
│   └── ProductManagement.IntegrationTests/ # 🧪 Integration tests
│
├── nginx/
│   ├── Dockerfile                           # Nginx custom image (bakes config in)
│   └── nginx.conf                           # Nginx reverse proxy configuration
├── docker-compose.yml                       # Docker services configuration
├── ProductManagement.sln                    # Solution file
├── README.md                                # This file
└── todo.md                                  # Development tasks
```

## 🧪 Testing

โปรเจกต์มี Test Coverage ครอบคลุมทั้ง Unit Tests และ Integration Tests ด้วย xUnit, Moq, และ FluentAssertions

### 📋 Test Structure

```
tests/
├── ProductManagement.UnitTests/
│   ├── Application/
│   │   ├── CategoryServiceTests.cs          # Unit tests for CategoryService
│   │   └── ProductServiceTests.cs           # Unit tests for ProductService
│   ├── Domain/
│   │   └── ProductTests.cs                  # Unit tests for Product entity
│   └── Builders/
│       ├── ProductBuilder.cs                # Test data builder for Product
│       ├── CategoryBuilder.cs               # Test data builder for Category
│       └── UserBuilder.cs                   # Test data builder for User
│
└── ProductManagement.IntegrationTests/
    ├── Controllers/                          # Integration tests for API endpoints
    └── Scenarios/                            # End-to-end test scenarios
```

### 🚀 Running Tests

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test src/ProductManagement.UnitTests/ProductManagement.UnitTests.csproj

# Run only integration tests
dotnet test src/ProductManagement.IntegrationTests/ProductManagement.IntegrationTests.csproj

# Run with detailed output
dotnet test --verbosity normal

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### ✅ Testing Best Practices

#### 1. **AAA Pattern (Arrange-Act-Assert)**
```csharp
[Fact]
public async Task CreateCategoryAsync_WithValidData_ShouldReturnSuccess()
{
    // ============================================
    // ARRANGE - Setup test data and dependencies
    // ============================================
    var request = new CreateCategoryRequest { Name = "Electronics" };
    _categoryRepositoryMock.Setup(x => x.FindAsync(...)).ReturnsAsync(new List<Category>());

    // ============================================
    // ACT - Execute the method being tested
    // ============================================
    var result = await _sut.CreateCategoryAsync(request, "TestUser");

    // ============================================
    // ASSERT - Verify the results
    // ============================================
    result.IsSuccess.Should().BeTrue();
    result.Data.Name.Should().Be("Electronics");
}
```

#### 2. **Descriptive Test Names** (Method_Scenario_ExpectedResult)
```csharp
✅ CreateCategoryAsync_WithValidData_ShouldReturnSuccess
✅ CreateCategoryAsync_WithExistingName_ShouldReturnFailure
✅ UpdateStock_WithNegativeQuantity_ShouldThrowException
```

#### 3. **Theory for Parameterized Tests**
```csharp
[Theory]
[InlineData(-10)]
[InlineData(0)]
[InlineData(-0.01)]
public void Create_WithInvalidPrice_ShouldThrowException(decimal invalidPrice)
{
    Action act = () => Product.Create("Name", "Desc", "SKU", invalidPrice, 100, Guid.NewGuid(), "User");
    act.Should().Throw<InvalidOperationException>();
}
```

#### 4. **Test Data Builders**
```csharp
var product = new ProductBuilder()
    .WithName("iPhone 15")
    .WithPrice(999.99m)
    .WithStock(100)
    .Build();
```

#### 5. **Verify Important Interactions**
```csharp
_categoryRepositoryMock.Verify(
    x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
    Times.Once,
    "Should add the new category");

_unitOfWorkMock.Verify(
    x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
    Times.Once,
    "Should save changes to database");
```

### 🎯 Test Coverage Areas

- ✅ **Domain Logic**: Product validation, business rules, computed properties
- ✅ **Service Layer**: CRUD operations, error handling, validation
- ✅ **Edge Cases**: Boundary values, negative/zero inputs, overflow scenarios
- ✅ **Price Validation**: Minimum (0.01), maximum values, negative prices
- ✅ **Stock Management**: Add/reduce stock, insufficient stock, zero boundaries
- ✅ **Discount Logic**: Valid ranges, constraint violations, boundary prices
- ✅ **Error Handling**: Database errors, validation failures, null inputs

### 📊 Current Test Statistics

- **Total Tests**: 57 passing
- **Unit Tests**: 44 (Product domain + Services)
- **Integration Tests**: 13 (API endpoints)
- **Code Coverage**: Focus on critical business logic

### 🔧 Testing Tools & Libraries

```xml
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="FluentAssertions" Version="8.8.0" />
<PackageReference Include="coverlet.collector" Version="6.0.4" />
<PackageReference Include="coverlet.msbuild" Version="6.0.4" />
```

### 🎓 Testing Principles

1. **Test Isolation** - แต่ละ test เป็นอิสระ ไม่มี shared state
2. **Mock Only What You Own** - Mock เฉพาะ interface ที่เราสร้าง (Repository, UnitOfWork)
3. **Don't Test Framework** - ไม่ test EF Core, AutoMapper, หรือ library ภายนอก
4. **Focus on Business Logic** - เน้น test logic ที่เราเขียน ไม่ใช่ framework behavior
5. **Fast & Reliable** - Tests ต้องรันเร็วและให้ผลลัพธ์เหมือนเดิมทุกครั้ง

## 🏗️ Architecture

โปรเจกต์นี้ใช้ **Clean Architecture** แบ่งชั้นการทำงานตามหลัก Dependency Inversion:

### 🏛️ Domain Layer (Core)
- **Entities**: Rich Domain Models พร้อม Business Logic
  - `Product`, `Category`, `Review`, `ProductImage`
  - Factory Methods, Business Rules, Computed Properties
- **Interfaces**: Repository Contracts
- **No Dependencies**: ไม่มี dependency กับชั้นอื่น

### 💼 Application Layer
- **Services**: Business Logic และ Use Cases
- **DTOs**: Data Transfer Objects สำหรับ API
- **Validators**: FluentValidation Rules
- **Mappings**: AutoMapper Profiles
- **Depends on**: Domain Layer เท่านั้น

### 🔧 Infrastructure Layer
- **Data Access**:
  - Entity Framework Core (DbContext, Migrations)
  - Dapper (High-Performance Queries)
  - Repository Implementations
  - Unit of Work Pattern
- **Depends on**: Domain & Application Layers

### 🌐 API Layer (Presentation)
- **Controllers**: RESTful API Endpoints
- **Middleware**: Custom middleware (error handling, logging)
- **Configuration**: DI, CORS, Swagger, Serilog
- **Depends on**: ทุก Layer ผ่าน Dependency Injection

## 🛠️ Technologies & Packages

### Core Framework
- **.NET 9.0** - Latest .NET version
- **C# 13** - Latest language features
- **ASP.NET Core 9.0** - Web API framework

### API & Web
- **Swashbuckle.AspNetCore 6.9.0** - Swagger/OpenAPI documentation
- **Microsoft.AspNetCore.OpenApi 9.0.8** - OpenAPI support
- **Microsoft.AspNetCore.SignalR 1.2.0** - Real-time communication (ready)

### Authentication & Security
- **Microsoft.AspNetCore.Authentication.JwtBearer 9.0.8** - JWT authentication
- **System.IdentityModel.Tokens.Jwt 8.15.0** - JWT token handling
- **AspNetCoreRateLimit 5.0.0** - API rate limiting

### Data Access & Caching
- **Microsoft.EntityFrameworkCore 9.0.8** - ORM
- **Microsoft.EntityFrameworkCore.SqlServer 9.0.8** - SQL Server provider
- **Microsoft.EntityFrameworkCore.Design 9.0.8** - Migration tools
- **Dapper 2.1.66** - Micro-ORM for high-performance queries
- **StackExchange.Redis 2.10.1** - Redis client
- **Microsoft.Extensions.Caching.StackExchangeRedis 10.0.1** - Redis caching

### Background Processing & Messaging
- **Hangfire.AspNetCore 1.8.22** - Background job processing
- **Hangfire.Core 1.8.22** - Hangfire core
- **Hangfire.SqlServer 1.8.22** - SQL Server storage for Hangfire
- **RabbitMQ.Client 6.x** - Message queue client

### Validation & Mapping
- **FluentValidation 12.1.1** - Fluent validation library
- **FluentValidation.AspNetCore** - ASP.NET Core integration
- **AutoMapper 16.0.0** - Object-to-object mapping
- **AutoMapper.Extensions.Microsoft.DependencyInjection** - DI integration

### Logging & Monitoring
- **Serilog 10.0.0** - Structured logging framework
- **Serilog.AspNetCore 10.0.0** - ASP.NET Core integration
- **Serilog.Sinks.Console 6.1.1** - Console output
- **Serilog.Sinks.File 7.0.0** - File output

### Health & Diagnostics
- **AspNetCore.HealthChecks.SqlServer 9.0.0** - SQL Server health checks
- **Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore 9.0.0** - EF Core health checks

### Testing
- **xUnit** - Testing framework
- **FluentAssertions** - Fluent assertion library
- **Moq** - Mocking framework (ready)

## 📋 Prerequisites

- **.NET SDK 9.0** or later ([ดาวน์โหลด](https://dotnet.microsoft.com/download))
- **SQL Server** (LocalDB, Express, Developer หรือ Full version)
- **Redis** (Optional - จะใช้ in-memory cache ถ้าไม่มี Redis)
- **RabbitMQ** (Optional - สำหรับ message queue features)
- **Docker Desktop** (Optional - สำหรับรัน services ด้วย Docker)
- **Visual Studio 2022** / **VS Code** / **JetBrains Rider**
- **Git** (optional)

## 🚀 Installation & Setup

### Option 1: รัน Services ด้วย Docker (แนะนำ)

#### 1. Clone Repository
```bash
git clone https://github.com/Max2535/ProductManagementAPI.git
cd ProductManagementAPI
```

#### 2. Start Services
```bash
docker-compose up -d
```

Services ที่จะรัน:
- **SQL Server** - Port 1433
- **RabbitMQ** - Port 5672, Management UI: 15672
- **API** - Port 8080 (HTTP), 8081 (HTTPS) (internal, ผ่าน Nginx)
- **Nginx** - Port 80 (HTTP), 443 (HTTPS) — reverse proxy หน้า API

#### 3. Access Services
- **API** (ผ่าน Nginx): http://localhost
- **Swagger UI**: http://localhost/swagger
- **Hangfire Dashboard**: http://localhost/hangfire
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

---

### Option 2: รันแบบ Manual (Development)

#### 1. Restore Dependencies
```bash
dotnet restore
```

#### 2. Update Connection Strings
แก้ไข [appsettings.Development.json](src/ProductManagement.API/appsettings.Development.json):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProductManagementDB;User Id=sa;Password=YourStrongPassword;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  }
}
```

#### 3. Create Database & Run Migrations
```bash
cd src/ProductManagement.API
dotnet ef database update
```

หรือสร้าง migration ใหม่:
```bash
dotnet ef migrations add YourMigrationName --project ../ProductManagement.Infrastructure
dotnet ef database update
```

#### 4. Build Solution
```bash
dotnet build
```

#### 5. Run Application
```bash
cd src/ProductManagement.API
dotnet run
```

หรือใช้ **watch mode** (auto-reload):
```bash
dotnet watch run
```

## 🔌 API Endpoints & Examples

### 🔐 1. Authentication & Authorization

#### 1.1 Register New User
```bash
POST /api/auth/register
Content-Type: application/json

{
  "username": "testuser",
  "email": "testuser@example.com",
  "password": "Test@1234",
  "confirmPassword": "Test@1234",
  "firstName": "Test",
  "lastName": "User"
}
```

#### 1.2 Login
```bash
POST /api/auth/login
Content-Type: application/json

{
  "emailOrUsername": "testuser@example.com",
  "password": "Test@1234"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "refresh_token_here",
    "user": {
      "id": "guid",
      "username": "testuser",
      "email": "testuser@example.com"
    }
  }
}
```

#### 1.3 Refresh Token
```bash
POST /api/auth/refresh-token
Content-Type: application/json

{
  "accessToken": "your_token",
  "refreshToken": "your_refresh_token"
}
```

#### 1.4 Logout
```bash
POST /api/auth/logout
Authorization: Bearer {token}
```

#### 1.5 Get Profile
```bash
GET /api/auth/profile
Authorization: Bearer {token}
```

#### 1.6 Update Profile
```bash
PUT /api/auth/profile
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "Updated",
  "lastName": "Name",
  "phoneNumber": "0812345678"
}
```

#### 1.7 Change Password
```bash
POST /api/auth/change-password
Authorization: Bearer {token}
Content-Type: application/json

{
  "currentPassword": "Test@1234",
  "newPassword": "NewTest@1234",
  "confirmPassword": "NewTest@1234"
}
```

---

### 📦 2. Products Management

#### 2.1 Get All Products (Paginated)
```bash
GET /api/products?pageNumber=1&pageSize=20
```

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [...],
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 5,
    "totalCount": 100
  }
}
```

#### 2.2 Get Product by ID
```bash
GET /api/products/{id}
```

#### 2.3 Get Product by SKU
```bash
GET /api/products/by-sku/SKU001
```

#### 2.4 Search Products
```bash
GET /api/products/search?searchTerm=laptop&pageNumber=1&pageSize=20
```

#### 2.5 Get Products by Category
```bash
GET /api/products/by-category/{categoryId}
```

#### 2.6 Get Low Stock Products
```bash
GET /api/products/low-stock
```

#### 2.7 Create Product
```bash
POST /api/products
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "New Product",
  "sku": "SKU999",
  "description": "Product description",
  "price": 999.99,
  "stockQuantity": 100,
  "minimumStockLevel": 10,
  "categoryId": "guid",
  "isActive": true
}
```

#### 2.8 Update Product
```bash
PUT /api/products/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Updated Product",
  "description": "Updated description",
  "price": 1199.99,
  "categoryId": "guid"
}
```

#### 2.9 Delete Product (Soft Delete)
```bash
DELETE /api/products/{id}
Authorization: Bearer {token}
```

#### 2.10 Update Product Stock
```bash
PATCH /api/products/{id}/stock
Authorization: Bearer {token}
Content-Type: application/json

{
  "quantity": 50,
  "adjustmentType": "Add",
  "notes": "Stock replenishment"
}
```

#### 2.11 Set Product Discount
```bash
PATCH /api/products/{id}/discount
Authorization: Bearer {token}
Content-Type: application/json

{
  "discountPrice": 799.99,
  "discountStartDate": "2025-12-30T00:00:00Z",
  "discountEndDate": "2026-01-31T23:59:59Z"
}
```

#### 2.12 Remove Product Discount
```bash
DELETE /api/products/{id}/discount
Authorization: Bearer {token}
```

#### 2.13 Activate Product
```bash
PATCH /api/products/{id}/activate
Authorization: Bearer {token}
```

#### 2.14 Deactivate Product
```bash
PATCH /api/products/{id}/deactivate
Authorization: Bearer {token}
```

---

### 🛒 3. Orders Management

#### 3.1 Create Order
```bash
POST /api/orders
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "guid",
  "items": [
    {
      "productId": "guid",
      "quantity": 2,
      "unitPrice": 999.99
    }
  ],
  "shippingAddress": "123 Main St, City, Country",
  "notes": "Please deliver carefully"
}
```

#### 3.2 Get Order by ID
```bash
GET /api/orders/{id}
Authorization: Bearer {token}
```

#### 3.3 Get Order by Order Number
```bash
GET /api/orders/number/ORD20251230001
Authorization: Bearer {token}
```

#### 3.4 Get All Orders (Paginated)
```bash
GET /api/orders?pageNumber=1&pageSize=20
Authorization: Bearer {token}
```

#### 3.5 Get My Orders
```bash
GET /api/orders/my-orders
Authorization: Bearer {token}
```

#### 3.6 Update Order Status
```bash
PATCH /api/orders/{id}/status
Authorization: Bearer {token}
Content-Type: application/json

{
  "status": "Confirmed",
  "notes": "Order confirmed"
}
```

**Available Statuses:**
- `Pending` - รอดำเนินการ
- `Confirmed` - ยืนยันคำสั่งซื้อ
- `Paid` - ชำระเงินแล้ว
- `Shipped` - จัดส่งแล้ว
- `Delivered` - ส่งถึงแล้ว
- `Cancelled` - ยกเลิก

#### 3.7 Add Item to Order
```bash
POST /api/orders/{id}/items
Authorization: Bearer {token}
Content-Type: application/json

{
  "productId": "guid",
  "quantity": 1,
  "unitPrice": 499.99
}
```

#### 3.8 Update Order Item Quantity
```bash
PATCH /api/orders/{id}/items
Authorization: Bearer {token}
Content-Type: application/json

{
  "orderItemId": "guid",
  "quantity": 3
}
```

#### 3.9 Delete Order
```bash
DELETE /api/orders/{id}
Authorization: Bearer {token}
```

---

### 📧 4. Email Testing

#### 4.1 Send Test Email
```bash
POST /api/email/send-test
Content-Type: application/json

{
  "to": "recipient@example.com"
}
```

#### 4.2 Send Welcome Email
```bash
POST /api/email/send-welcome
Content-Type: application/json

{
  "email": "newuser@example.com",
  "userName": "New User"
}
```

#### 4.3 Send Low Stock Alert
```bash
POST /api/email/send-low-stock-alert
Content-Type: application/json

{
  "productName": "Laptop Dell XPS 15",
  "sku": "SKU001",
  "currentStock": 5,
  "minimumStock": 10
}
```

---

### ⚙️ 5. Background Jobs (Admin Only)

#### 5.1 Trigger Stock Alert Job
```bash
POST /api/jobs/trigger-stock-alert
Authorization: Bearer {token}
```

#### 5.2 Trigger Cleanup Tokens Job
```bash
POST /api/jobs/trigger-cleanup-tokens
Authorization: Bearer {token}
```

#### 5.3 Schedule Delayed Job
```bash
POST /api/jobs/schedule-delayed-job?delayMinutes=5
Authorization: Bearer {token}
```

#### 5.4 Get Job Status
```bash
GET /api/jobs/status/{jobId}
Authorization: Bearer {token}
```

---

### 📝 API Response Format

All API endpoints return responses in a consistent format:

**Success Response:**
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful"
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Error message",
  "errors": [
    "Detailed error 1",
    "Detailed error 2"
  ]
}
```

### 💡 Testing with VS Code REST Client
ดูตัวอย่างการเรียก API แบบสมบูรณ์พร้อม authentication ใน [ProductManagement.API.http](src/ProductManagement.API/ProductManagement.API.http)

## 🔄 Background Jobs (Hangfire)

### Recurring Jobs
1. **Product Stock Alert Job**
   - Schedule: ทุกวันเวลา 09:00 น.
   - หน้าที่: ตรวจสอบสินค้าที่ใกล้หมดและส่งอีเมลแจ้งเตือน
   - Cron: `0 9 * * *`

2. **Cleanup Expired Tokens Job**
   - Schedule: ทุกวันเวลา 02:00 น.
   - หน้าที่: ลบ refresh tokens ที่หมดอายุออกจากฐานข้อมูล
   - Cron: `0 2 * * *`

### Manual Job Triggers (Admin Only)
- **POST** `/api/Jobs/trigger-stock-alert` - รันเช็คสต็อกทันที
- **POST** `/api/Jobs/trigger-cleanup-tokens` - รันลบ tokens ทันที
- **POST** `/api/Jobs/schedule-delayed-job?delayMinutes=5` - กำหนด job ที่จะรันภายหลัง
- **GET** `/api/Jobs/status/{jobId}` - ตรวจสอบสถานะ job

### Hangfire Dashboard
- **URL**: `http://localhost:5219/hangfire`
- **Features**:
  - ดู job history
  - Monitor job execution
  - Retry failed jobs
  - Schedule new jobs

## 📧 Email Features

### Available Email Types
1. **Test Email** - ทดสอบการส่งอีเมล
2. **Welcome Email** - ส่งให้ผู้ใช้ใหม่หลังสมัครสมาชิก
3. **Low Stock Alert** - แจ้งเตือนสินค้าใกล้หมด
4. **Order Confirmation** - ยืนยันคำสั่งซื้อ (ready)

### Testing Email
```bash
# Send test email
curl -X POST "http://localhost:5219/api/Email/send-test" \
  -H "Content-Type: application/json" \
  -d '{"to": "recipient@example.com"}'

# Send welcome email
curl -X POST "http://localhost:5219/api/Email/send-welcome" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "userName": "New User"
  }'

# Send low stock alert
curl -X POST "http://localhost:5219/api/Email/send-low-stock-alert" \
  -H "Content-Type: application/json" \
  -d '{
    "productName": "Product Name",
    "sku": "SKU001",
    "currentStock": 5,
    "minimumStock": 10
  }'
```

## 🔄 RabbitMQ Message Flow

### Events
1. **Order Created Event**
   - Published เมื่อสร้างคำสั่งซื้อใหม่
   - Queue: `order.created`
   - Consumer: บันทึก log, ส่ง notification

2. **Stock Reservation Event**
   - Published เมื่อ order status เป็น "Paid"
   - Queue: `stock.reservation`
   - Consumer: ลดจำนวนสินค้าในคลัง

3. **Stock Release Event**
   - Published เมื่อ order ถูกยกเลิก
   - Queue: `stock.release`
   - Consumer: คืนจำนวนสินค้าในคลัง

4. **Product Updated Event**
   - Published เมื่อมีการอัพเดทสินค้า
   - Queue: `product.updated`
   - Consumer: อัพเดท cache, sync ข้อมูล

### Message Format Example
```json
{
  "eventId": "guid",
  "eventType": "OrderCreated",
  "timestamp": "2025-12-30T10:00:00Z",
  "payload": {
    "orderId": "guid",
    "orderNumber": "ORD20251230001",
    "userId": "guid",
    "totalAmount": 1999.99
  }
}
```

### RabbitMQ Management
- **URL**: http://localhost:15672
- **Credentials**: guest/guest
- **Features**:
  - Monitor queues and exchanges
  - View message rates
  - Manage bindings
  - Purge queues

## 🎯 Monitoring & Observability

### Health Checks
```bash
# Check API health
curl http://localhost:5219/health

# Response
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "database": {
      "status": "Healthy",
      "description": "Database connection is healthy"
    }
  }
}
```

### Logging
- **Location**: `logs/productmanagement-YYYYMMDD.txt` และ `src/ProductManagement.API/logs/`
- **Format**: Structured JSON logging
- **Levels**: Debug, Information, Warning, Error, Fatal
- **Features**:
  - Request/Response logging
  - Exception tracking
  - Performance metrics
  - Correlation IDs

### Metrics to Monitor
- API response times
- Database connection pool
- Redis cache hit rate
- RabbitMQ queue depth
- Hangfire job success rate
- Rate limit violations

## 🐛 Troubleshooting

### Common Issues

#### 1. Database Connection Failed
```
Error: Cannot connect to SQL Server
Solution:
- ตรวจสอบ SQL Server service กำลังรันอยู่
- ตรวจสอบ connection string ใน appsettings
- ตรวจสอบ firewall settings
- สำหรับ Docker: ใช้ server name "sqlserver" แทน "localhost"
```

#### 2. Redis Connection Failed
```
Error: Unable to connect to Redis
Solution:
- ระบบจะใช้ in-memory cache แทนอัตโนมัติ
- ถ้าต้องการใช้ Redis: docker-compose up redis
- ตรวจสอบ Redis service: redis-cli ping
```

#### 3. RabbitMQ Connection Failed
```
Error: Cannot connect to RabbitMQ
Solution:
- ตรวจสอบ RabbitMQ service: docker-compose ps rabbitmq
- ตรวจสอบ credentials ใน appsettings
- ดู RabbitMQ logs: docker-compose logs rabbitmq
```

#### 4. JWT Token Invalid
```
Error: 401 Unauthorized
Solution:
- ตรวจสอบ token ยังไม่หมดอายุ
- ใช้ refresh token เพื่อขอ access token ใหม่
- ตรวจสอบ JWT settings (SecretKey, Issuer, Audience)
```

#### 5. Migration Failed
```
Error: Migration failed
Solution:
- ลบ database และสร้างใหม่: dotnet ef database drop
- Update migration: dotnet ef database update
- ตรวจสอบ connection string
```

### Debug Mode
เปิด detailed logging โดยแก้ไข `appsettings.Development.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.EntityFrameworkCore": "Information"
      }
    }
  }
}
```

## 📝 Configuration

### Application Settings Files
- [appsettings.json](src/ProductManagement.API/appsettings.json) - Base configuration
- [appsettings.Development.json](src/ProductManagement.API/appsettings.Development.json) - Development overrides
- [appsettings.Production.json](src/ProductManagement.API/appsettings.Production.json) - Production overrides

### Key Configurations

#### 1. Database Connection
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ProductManagementDB;User Id=sa;Password=YourStrongPassword;TrustServerCertificate=True;MultipleActiveResultSets=true",
  "Redis": "localhost:6379"
}
```

#### 2. JWT Settings
```json
"JwtSettings": {
  "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
  "Issuer": "ProductManagementAPI",
  "Audience": "ProductManagementClient",
  "AccessTokenExpirationMinutes": 60,
  "RefreshTokenExpirationDays": 7
}
```

> ⚠️ **สำคัญ**: เปลี่ยน SecretKey ใน production และเก็บไว้ใน environment variables หรือ Azure Key Vault

#### 3. Email Configuration (Gmail SMTP)
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderName": "Product Management System",
  "SenderEmail": "your-email@gmail.com",
  "Username": "your-email@gmail.com",
  "Password": "your-16-char-app-password",
  "EnableSsl": true
}
```

> ⚠️ **Gmail App Password Setup:**
> 1. เปิด [Google Account Security](https://myaccount.google.com/security)
> 2. เปิดใช้งาน **2-Step Verification**
> 3. ไปที่ [App Passwords](https://myaccount.google.com/apppasswords)
> 4. สร้าง App Password สำหรับ "Mail"
> 5. คัดลอก 16 ตัวอักษรไปใส่ใน `Password` field

#### 4. RabbitMQ Configuration
```json
"RabbitMQ": {
  "HostName": "localhost",
  "Port": 5672,
  "UserName": "guest",
  "Password": "guest",
  "VirtualHost": "/",
  "OrderExchange": "order.exchange",
  "ProductExchange": "product.exchange",
  "OrderCreatedQueue": "order.created",
  "StockReservationQueue": "stock.reservation",
  "StockReleaseQueue": "stock.release",
  "ProductUpdatedQueue": "product.updated"
}
```

#### 5. Rate Limiting
```json
"IpRateLimiting": {
  "EnableEndpointRateLimiting": true,
  "GeneralRules": [
    {
      "Endpoint": "*",
      "Period": "1m",
      "Limit": 60
    },
    {
      "Endpoint": "POST:/api/Auth/login",
      "Period": "1m",
      "Limit": 5
    }
  ]
}
```

#### 6. Logging (Serilog)
- **Log Location**: `logs/productmanagement-YYYYMMDD.txt`
- **Rolling Interval**: Daily
- **Minimum Level**: Information
- **Override Levels**:
  - Microsoft: Warning
  - EntityFrameworkCore: Warning
  - Hangfire: Information

#### 7. CORS
- **Development**: AllowAll policy (อนุญาตทุก origin)
- **Production**: Restricted policy (ระบุ domain ที่อนุญาต)

## 🎯 Development Features

### Domain-Driven Design
- **Rich Domain Models**: Entities มี business logic
- **Factory Methods**: `Product.Create()` pattern
- **Value Objects**: Encapsulation ของข้อมูล
- **Domain Events**: (Ready for implementation)

### Best Practices
- ✅ **Async/Await** - ทุก operation เป็น asynchronous
- ✅ **CancellationToken** - รองรับ cancellation ของ requests
- ✅ **Pagination** - ทุก list endpoint มี paging
- ✅ **Validation** - FluentValidation ทั้งระบบ
- ✅ **Error Handling** - Global exception middleware
- ✅ **Logging** - Structured logging ด้วย Serilog
- ✅ **API Response** - Consistent response format
- ✅ **Repository Pattern** - Data access abstraction
- ✅ **Unit of Work** - Transaction management

## 📊 Database

### Entity Relationship
- **User** - ผู้ใช้งานระบบ
  - Roles: Admin, Manager, User
  - Authentication & Profile management

- **Product** - สินค้า
  - Name, SKU, Price, Stock, Category
  - Discount pricing with date range
  - Status tracking (Active/Inactive)
  - Low stock alerts

- **Category** - หมวดหมู่สินค้า
  - Hierarchical structure support

- **Order** - คำสั่งซื้อ
  - Order number generation (ORDYYYYMMDDnnn)
  - Status workflow (Pending → Confirmed → Paid → Shipped → Delivered)
  - Cancel with stock release
  - Automatic totals calculation

- **OrderItem** - รายการสินค้าในคำสั่งซื้อ
  - Product snapshot at order time
  - Quantity and price tracking

- **Review** - รีวิวสินค้า
  - Rating (1-5 stars)
  - Comment and user tracking

- **ProductImage** - รูปภาพสินค้า
  - Multiple images per product
  - Display order support

### Database Features
- **Audit Fields**: CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
- **Soft Delete**: IsDeleted, DeletedAt, DeletedBy
- **Computed Properties**: EffectivePrice, IsLowStock, AverageRating
- **Connection Resilience**: Auto-retry (3 attempts, 30s max delay)
- **Migration Management**: Code-first with EF Core

### Seed Data
Default admin user จะถูกสร้างอัตโนมัติเมื่อรันครั้งแรก:
- **Email**: admin@example.com
- **Password**: Admin@123 (ควรเปลี่ยนทันที)

## 🔐 Security Features

### Implemented
- ✅ **JWT Authentication** - Token-based auth with access & refresh tokens
- ✅ **Password Hashing** - SHA-256 with salt
- ✅ **Role-Based Authorization** - Admin, Manager, User roles
- ✅ **API Rate Limiting** - Endpoint-specific limits
- ✅ **CORS Configuration** - Environment-based policies
- ✅ **HTTPS Redirection** - Force secure connections
- ✅ **Input Validation** - FluentValidation on all inputs
- ✅ **Audit Logging** - Track who created/modified what

### Security Best Practices
1. **Secrets Management**
   - Use environment variables for sensitive data
   - Never commit secrets to repository
   - Use Azure Key Vault in production

2. **Token Management**
   - Short-lived access tokens (60 minutes)
   - Long-lived refresh tokens (7 days)
   - Automatic token cleanup job

3. **Rate Limiting**
   - General API: 60 requests/minute
   - Login endpoint: 5 requests/minute
   - Registration: 10 requests/hour

4. **Password Requirements**
   - Minimum 8 characters
   - Requires uppercase, lowercase, number, special character

## 📚 Additional Resources

### Documentation
- [API Documentation](src/ProductManagement.API/ProductManagement.API.http) - HTTP request examples
- [Entity Models](src/ProductManagement.Domain/Entities/) - Domain entities
- [Swagger UI](http://localhost:5219/swagger) - Interactive API docs
- [Hangfire Dashboard](http://localhost:5219/hangfire) - Background jobs monitoring
- [RabbitMQ Management](http://localhost:15672) - Message queue monitoring

### Architecture References
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) by Uncle Bob
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html) by Martin Fowler
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [Unit of Work Pattern](https://martinfowler.com/eaaCatalog/unitOfWork.html)

### Technology Documentation
- [.NET 9.0 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Hangfire Documentation](https://docs.hangfire.io/)
- [RabbitMQ .NET Client](https://www.rabbitmq.com/dotnet.html)
- [Serilog Documentation](https://serilog.net/)

### Best Practices
- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [RESTful API Design](https://restfulapi.net/)
- [Microservices Patterns](https://microservices.io/patterns/)

## 🤝 Contributing

1. Fork the project
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

## 🗺️ Roadmap

### Phase 1 - Core Features ✅ (Completed)
- [x] Clean Architecture setup
- [x] Product CRUD operations
- [x] JWT Authentication & Authorization
- [x] Order management
- [x] Background jobs (Hangfire)
- [x] Email notifications
- [x] RabbitMQ integration
- [x] Redis caching
- [x] Rate limiting
- [x] Category management

### Phase 2 - Testing & Quality 🚧 (In Progress)
- [x] Unit tests with xUnit, Moq, FluentAssertions
- [x] Integration tests for API endpoints
- [x] Test Data Builders (Product, Category, User)
- [x] Parameterized tests with Theory/InlineData
- [x] Increase test coverage (>80%)
- [x] Performance/Load testing

### Phase 3 - Advanced Features 📋 (Planned)
- [ ] Payment integration (Stripe/Omise)
- [ ] Image upload to cloud storage (Azure Blob/AWS S3)
- [ ] Advanced search with Elasticsearch
- [ ] Real-time notifications (SignalR)
- [ ] GraphQL API
- [ ] API versioning

### Phase 4 - DevOps & Production 🚧 (In Progress)
- [x] CI/CD pipeline (GitHub Actions)
- [x] Kubernetes deployment
- [x] Nginx Reverse Proxy (custom Docker image)
- [ ] API Gateway (Ocelot)
- [ ] Monitoring (Prometheus + Grafana)
- [ ] Distributed tracing (Jaeger)
- [ ] Container orchestration

### Phase 5 - Microservices 🔮 (Future)
- [ ] Split into microservices
- [ ] Event sourcing with EventStore
- [ ] CQRS with MediatR
- [ ] Service mesh (Istio)
- [ ] Service discovery (Consul)
- [ ] Distributed cache (Redis Cluster)

## 📄 License

This project is licensed under the MIT License

## 👨‍💻 Author

**Your Name**
📧 Email: your.email@example.com
🐙 GitHub: [@yourusername](https://github.com/yourusername)

## 🙏 Acknowledgments

- Clean Architecture by Robert C. Martin (Uncle Bob)
- Domain-Driven Design by Eric Evans
- ASP.NET Core Team

---

<div align="center">

### 🌟 ถ้าโปรเจกต์นี้มีประโยชน์ อย่าลืมกด Star ⭐

**Made with ❤️ using .NET 9.0**

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13-239120?logo=c-sharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

</div>
