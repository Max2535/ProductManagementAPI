# TODO: Config-Driven Hangfire Jobs

## Overview
ระบบ Hangfire jobs ที่ดึง configuration จาก database เพื่อรันงานแบบ dynamic โดยไม่ต้องแก้ไข code

## Database Schema

### Table: BatchJobConfig
```sql
CREATE TABLE BatchJobConfig (
    BatchId UNIQUEIDENTIFIER PRIMARY KEY,
    BatchCode NVARCHAR(100) NOT NULL,
    BatchType NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500),
    Crontab NVARCHAR(100),
    CallMethod NVARCHAR(200) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CreatedDt DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(100) NOT NULL,
    UpdatedDt DATETIME2,
    UpdatedBy NVARCHAR(100),
    QueueName NVARCHAR(100),
    TargetService NVARCHAR(200),
    TargetParam NVARCHAR(MAX)
);

-- Sample data
INSERT INTO BatchJobConfig
(BatchId, BatchCode, BatchType, Description, Crontab, CallMethod, Status,
 CreatedDt, CreatedBy, QueueName, TargetService, TargetParam)
VALUES
('0445834f-3853-4689-98b4-35136d524e03', 'dsssu018OneFile', 'REPORT',
 'Report DSSSU018 One File', '0 9 * * *', 'GenerateOneFileAsync', 'ACTIVE',
 GETDATE(), 'system', 'default', 'ReportService', 'DSSSU018');
```

## Implementation Steps

### 1. Domain Layer
- [ ] สร้าง `ProductManagement.Domain/Entities/BatchJobConfig.cs`
- [ ] สร้าง `ProductManagement.Domain/Interfaces/IBatchJobConfigRepository.cs`

### 2. Infrastructure Layer
- [ ] เพิ่ม DbSet ใน `ApplicationDbContext.cs`
- [ ] Configure entity mapping ใน `OnModelCreating`
- [ ] สร้าง `ProductManagement.Infrastructure/Repositories/BatchJobConfigRepository.cs`
- [ ] สร้าง migration: `dotnet ef migrations add AddBatchJobConfig`

### 3. Application Layer
- [ ] สร้าง `ProductManagement.Application/BackgroundJobs/ConfigDrivenJobRunner.cs`
  - Implement dynamic method invocation using Reflection
  - Support async methods
  - Handle parameters from database
  - Error handling and logging

- [ ] สร้าง `ProductManagement.Application/BackgroundJobs/ConfigBatchJobScheduler.cs`
  - IHostedService for loading configs on startup
  - Register recurring jobs from database
  - Support multiple queues

- [ ] สร้าง example services (ถ้ายังไม่มี):
  - `ProductManagement.Application/Services/ReportService.cs`
  - `ProductManagement.Application/Services/NotificationService.cs`

### 4. API Layer
- [ ] Update `Program.cs`:
  ```csharp
  // Register repository
  builder.Services.AddScoped<IBatchJobConfigRepository, BatchJobConfigRepository>();

  // Register jobs
  builder.Services.AddScoped<ConfigDrivenJobRunner>();

  // Register example services
  builder.Services.AddScoped<ReportService>();

  // Register hosted service
  builder.Services.AddHostedService<ConfigBatchJobScheduler>();
  ```

- [ ] Update `Controllers/JobsController.cs`:
  - เพิ่ม endpoint `POST /api/jobs/trigger-config/{batchCode}`
  - เพิ่ม endpoint `GET /api/jobs/configs` (list all configs)
  - เพิ่ม endpoint `POST /api/jobs/refresh-configs` (reload configs)

### 5. Advanced Features (Optional)

#### 5.1 Auto-reload configs
- [ ] สร้าง BackgroundService ที่ poll database ทุก X นาที
- [ ] เมื่อมี config เปลี่ยนแปลง ให้ AddOrUpdate recurring jobs ใหม่

#### 5.2 Parameter parsing
- [ ] Support JSON parameters
- [ ] Support multiple parameter types
- [ ] Parameter validation

#### 5.3 Job monitoring
- [ ] เก็บ job execution history ลง database
- [ ] Dashboard สำหรับดู job statistics
- [ ] Alert เมื่อ job ล้มเหลว

#### 5.4 Dependency injection improvements
- [ ] Cache service types for better performance
- [ ] Support multiple namespaces
- [ ] Support interface-based service resolution

## Key Code Snippets

### Dynamic Method Invocation
```csharp
private async Task InvokeMethodByReflection(string serviceName, string methodName, string param)
{
    // 1. Find service type
    var serviceType = FindServiceType(serviceName);

    // 2. Resolve from DI
    var serviceInstance = _serviceProvider.GetService(serviceType);

    // 3. Get method
    var method = serviceType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);

    // 4. Build parameters
    var parameters = BuildMethodParameters(method, param);

    // 5. Invoke
    var result = method.Invoke(serviceInstance, parameters);

    // 6. Await if Task
    if (result is Task task)
        await task.ConfigureAwait(false);
}
```

### Hosted Service Pattern
```csharp
public async Task StartAsync(CancellationToken cancellationToken)
{
    var configs = await _repo.GetActiveConfigsAsync(cancellationToken);

    foreach (var cfg in configs)
    {
        _recurringJobManager.AddOrUpdate<ConfigDrivenJobRunner>(
            cfg.BatchCode,
            job => job.Execute(cfg.BatchId),
            cfg.Crontab!,
            new RecurringJobOptions
            {
                QueueName = cfg.QueueName ?? "default",
                TimeZone = TimeZoneInfo.Utc
            });
    }
}
```

## Testing Plan
- [ ] Unit tests สำหรับ ConfigDrivenJobRunner
- [ ] Integration tests สำหรับ dynamic method invocation
- [ ] Test กับ service หลายแบบ
- [ ] Test error handling
- [ ] Test async/sync methods

## Documentation
- [ ] อธิบายวิธีเพิ่ม service ใหม่
- [ ] อธิบายวิธีเพิ่ม job config ใหม่ใน database
- [ ] อธิบาย cron expression format
- [ ] อธิบาย queue naming convention

## Performance Considerations
- ใช้ Reflection มี overhead ประมาณ 10-50x ช้ากว่า direct call
- สามารถ cache compiled expressions ได้ถ้าต้องการเร็วขึ้น
- ใช้ scoped services ตาม lifetime ที่เหมาะสม
- Monitor memory usage ถ้ามี jobs เยอะ

## Security Considerations
- ⚠️ Validate service names และ method names
- ⚠️ Whitelist allowed services/methods
- ⚠️ Sanitize parameters จาก database
- ⚠️ Implement proper authorization สำหรับ trigger endpoints

## Next Steps
1. สร้าง database table และ migration
2. Implement domain และ infrastructure layers
3. Implement ConfigDrivenJobRunner
4. Test กับ simple service ก่อน
5. เพิ่ม advanced features ทีละอย่าง

---
**Created:** 2025-12-26
**Status:** Pending
**Priority:** Medium
**Estimated Time:** 4-6 hours
