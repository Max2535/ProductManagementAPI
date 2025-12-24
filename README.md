# Product Management API

ระบบ API สำหรับจัดการข้อมูลสินค้าแบบ E-commerce พัฒนาด้วย .NET 9.0 โดยใช้หลักการ Clean Architecture, Domain-Driven Design และ SOLID Principles

## ✨ Features

- ✅ **Clean Architecture** - แบ่งชั้นการทำงานอย่างชัดเจน (Domain, Application, Infrastructure, API)
- ✅ **Rich Domain Model** - Entity มี Business Logic ที่สมบูรณ์
- ✅ **Repository & Unit of Work Pattern** - จัดการ Data Access อย่างมีประสิทธิภาพ
- ✅ **CQRS Ready** - รองรับ MediatR สำหรับ Command/Query Separation
- ✅ **FluentValidation** - Validation ที่ยืดหยุ่นและอ่านง่าย
- ✅ **AutoMapper** - Object Mapping อัตโนมัติ
- ✅ **Serilog** - Structured Logging ระดับ Production
- ✅ **Swagger/OpenAPI** - API Documentation แบบ Interactive
- ✅ **Entity Framework Core** - ORM พร้อม Migration Support
- ✅ **Dapper** - High-Performance Data Access (Infrastructure Layer)

## 📁 Project Structure

```
ProductManagementAPI/
├── src/
│   ├── ProductManagement.API/              # 🌐 API Layer
│   │   ├── Controllers/                     # REST API Controllers
│   │   │   └── ProductsController.cs       # Products endpoint
│   │   ├── Middleware/                      # Custom middleware
│   │   ├── Properties/
│   │   │   └── launchSettings.json         # Launch profiles
│   │   ├── Program.cs                       # Application entry point
│   │   ├── appsettings.json                # Configuration
│   │   └── ProductManagement.API.http      # HTTP request samples
│   │
│   ├── ProductManagement.Application/      # 💼 Application Layer
│   │   ├── Services/                        # Business services
│   │   ├── DTOs/                            # Data Transfer Objects
│   │   ├── Validators/                      # FluentValidation rules
│   │   ├── Mappings/                        # AutoMapper profiles
│   │   └── Interfaces/                      # Service interfaces
│   │
│   ├── ProductManagement.Domain/           # 🏛️ Domain Layer
│   │   ├── Entities/
│   │   │   ├── Product.cs                  # Product entity
│   │   │   ├── Category.cs                 # Category entity
│   │   │   ├── Review.cs                   # Review entity
│   │   │   ├── ProductImage.cs             # Product image entity
│   │   │   └── BaseEntity.cs               # Base entity class
│   │   └── Interfaces/
│   │       ├── IRepository.cs              # Generic repository
│   │       └── IProductRepository.cs       # Product repository
│   │
│   └── ProductManagement.Infrastructure/   # 🔧 Infrastructure Layer
│       ├── Data/
│       │   ├── ApplicationDbContext.cs     # EF Core DbContext
│       │   ├── UnitOfWork.cs               # Unit of Work implementation
│       │   └── Repositories/               # Repository implementations
│       └── Migrations/                      # EF Core migrations
│
├── tests/
│   ├── ProductManagement.UnitTests/        # 🧪 Unit tests
│   └── ProductManagement.IntegrationTests/ # 🧪 Integration tests
│
├── docker-compose.yml                       # Docker configuration
├── ProductManagement.sln                    # Solution file
└── README.md                                # Documentation
```

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

### API & Web
- **ASP.NET Core 9.0** - Web API framework
- **Swashbuckle 10.1.0** - Swagger/OpenAPI
- **SignalR 1.2.0** - Real-time communication

### Data Access
- **Entity Framework Core 10.0.1** - ORM
- **Dapper 2.1.66** - Micro-ORM for performance-critical queries
- **SQL Server** - Database

### Validation & Mapping
- **FluentValidation 12.1.1** - Validation library
- **AutoMapper 16.0.0** - Object-to-object mapping

### Logging
- **Serilog 10.0.0** - Structured logging
- **Serilog.Sinks.Console 6.1.1**
- **Serilog.Sinks.File 7.0.0**

### Testing
- **xUnit** - Testing framework
- **FluentAssertions** - Test assertions

### CQRS & Mediator
- **MediatR 14.0.0** - Mediator pattern implementation

## 📋 Prerequisites

- **.NET SDK 9.0** or later ([ดาวน์โหลด](https://dotnet.microsoft.com/download))
- **SQL Server** (LocalDB, Express หรือ Full version)
- **Visual Studio 2022** / **VS Code** / **Rider**
- **Git** (optional)

## 🚀 Installation & Setup

### 1. Clone Repository
```bash
git clone <repository-url>
cd ProductManagementAPI
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Update Connection String
แก้ไข [appsettings.json](src/ProductManagement.API/appsettings.json) หรือ `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProductManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 4. Create Database & Run Migrations
```bash
cd src/ProductManagement.API
dotnet ef database update
```

หรือสร้าง migration ใหม่:
```bash
dotnet ef migrations add InitialCreate --project ../ProductManagement.Infrastructure
dotnet ef database update
```

### 5. Build Solution
```bash
dotnet build
```

## ▶️ Running the Application

### Development Mode
```bash
cd src/ProductManagement.API
dotnet run
```

หรือใช้ **watch mode** (auto-reload):
```bash
dotnet watch run
```

### Access Points
- **API**: `https://localhost:7100` หรือ `http://localhost:5100`
- **Swagger UI**: `https://localhost:7100/swagger`
- **API Docs**: `https://localhost:7100/swagger/v1/swagger.json`

### Using Docker (Coming Soon)
```bash
docker-compose up
```

## 🔌 API Endpoints

### Products

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/products` | ดึงรายการสินค้าแบบ Pagination |
| `GET` | `/api/products/{id}` | ดึงข้อมูลสินค้าตาม ID |
| `POST` | `/api/products` | สร้างสินค้าใหม่ |
| `PUT` | `/api/products/{id}` | แก้ไขข้อมูลสินค้า |
| `DELETE` | `/api/products/{id}` | ลบสินค้า |
| `PATCH` | `/api/products/{id}/stock` | อัพเดท Stock |
| `GET` | `/api/products/search` | ค้นหาสินค้า |
| `GET` | `/api/products/category/{categoryId}` | ดึงสินค้าตาม Category |

### Example Requests
ดูตัวอย่างการเรียก API ใน [ProductManagement.API.http](src/ProductManagement.API/ProductManagement.API.http)

## 🧪 Testing

### Run All Tests
```bash
dotnet test
```

### Run with Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=lcov
```

### Run Specific Test Project
```bash
# Unit Tests
dotnet test tests/ProductManagement.UnitTests

# Integration Tests
dotnet test tests/ProductManagement.IntegrationTests
```

## 📝 Configuration

### Application Settings
- [appsettings.json](src/ProductManagement.API/appsettings.json) - Production config
- [appsettings.Development.json](src/ProductManagement.API/appsettings.Development.json) - Development config

### Key Configurations

**Database Connection**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ProductManagementDB;..."
}
```

**Logging (Serilog)**:
- Log file location: `logs/productmanagement-YYYYMMDD.txt`
- Rolling interval: Daily
- Minimum level: Information (Warning for Microsoft/EF Core)

**CORS**:
- Development: AllowAll policy
- Production: Restricted to specific domains

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

### Entities
- **Product** - สินค้า (Name, SKU, Price, Stock, Status)
- **Category** - หมวดหมู่สินค้า
- **ProductImage** - รูปภาพสินค้า
- **Review** - รีวิวสินค้า

### Features
- Audit fields (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
- Soft Delete support
- Computed properties (EffectivePrice, IsLowStock, AverageRating)
- Retry logic & Connection resilience

## 🔐 Security Features (Planned)
- [ ] JWT Authentication
- [ ] Role-Based Authorization
- [ ] API Rate Limiting
- [ ] Input Sanitization
- [ ] HTTPS Enforcement

## 📚 Additional Resources

### Documentation
- [API Documentation](src/ProductManagement.API/ProductManagement.API.http) - HTTP request examples
- [Entity Models](src/ProductManagement.Domain/Entities/) - Domain entities

### Learning Resources
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)

## 🤝 Contributing

1. Fork the project
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open Pull Request

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

Made with ❤️ using .NET 9.0
## 📊 Architecture Diagram
```
┌─────────────────────────────────────────────────────────┐
│                    API Layer (Controllers)               │
│  ┌───────────────┐  ┌────────────────┐  ┌────────────┐ │
│  │ Products      │  │ Categories     │  │ Reviews    │ │
│  │ Controller    │  │ Controller     │  │ Controller │ │
│  └───────┬───────┘  └────────┬───────┘  └──────┬─────┘ │
└──────────┼──────────────────┼─────────────────┼────────┘
           │                  │                  │
           │                  ▼                  │
           │         ┌────────────────┐          │
           └────────►│ Middleware     │◄─────────┘
                     │ - Exception    │
                     │ - Logging      │
                     └────────┬───────┘
                              │
┌─────────────────────────────▼───────────────────────────┐
│              Application Layer (Services)                │
│  ┌───────────────────────────────────────────────────┐  │
│  │              Product Service                      │  │
│  │  - CreateAsync()    - UpdateAsync()               │  │
│  │  - GetByIdAsync()   - DeleteAsync()               │  │
│  │  - SearchAsync()    - UpdateStockAsync()          │  │
│  └───────────────────────┬───────────────────────────┘  │
│                          │                               │
│  ┌───────────────────────▼───────────────────────────┐  │
│  │           AutoMapper & FluentValidation           │  │
│  └───────────────────────┬───────────────────────────┘  │
└────────────────────────────┼─────────────────────────────┘
                             │
┌────────────────────────────▼─────────────────────────────┐
│            Infrastructure Layer (Data Access)             │
│  ┌──────────────────┐         ┌─────────────────────┐   │
│  │  Unit of Work    │◄────────┤  ApplicationDbContext│   │
│  │                  │         │  - Products          │   │
│  │  ┌────────────┐  │         │  - Categories        │   │
│  │  │ Products   │  │         │  - Reviews           │   │
│  │  │ Repository │  │         └─────────────────────┘   │
│  │  └────────────┘  │                                    │
│  └──────────────────┘                                    │
└──────────────────────────┬───────────────────────────────┘
                           │
                           ▼
                  ┌────────────────┐
                  │  SQL Server    │
                  │  Database      │
                  └────────────────┘
