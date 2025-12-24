using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductManagement.API.Middleware;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Mappings;
using ProductManagement.Application.Services;
using ProductManagement.Application.Validators;
using ProductManagement.Domain.Interfaces;
using ProductManagement.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. Serilog Configuration
// ============================================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ProductManagementAPI")
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/productmanagement-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Product Management API");

    // ============================================
    // 2. Database Configuration
    // ============================================
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                sqlOptions.CommandTimeout(30);
            });

        // Enable sensitive data logging in development
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    // ============================================
    // 3. Register Repositories & Unit of Work
    // ============================================
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // ============================================
    // 4. Register Application Services
    // ============================================
    builder.Services.AddScoped<IProductService, ProductService>();

    // ============================================
    // 5. AutoMapper Configuration
    // ============================================
    builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

    // ============================================
    // 6. FluentValidation Configuration
    // ============================================
    builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();

    // ============================================
    // 7. HttpContextAccessor for audit fields
    // ============================================
    builder.Services.AddHttpContextAccessor();

    // ============================================
    // 8. CORS Configuration
    // ============================================
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });

        // Production CORS policy (more restrictive)
        options.AddPolicy("Production", policy =>
        {
            policy.WithOrigins(
                    "https://yourdomain.com",
                    "https://www.yourdomain.com")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    // ============================================
    // 9. API Controllers & JSON Configuration
    // ============================================
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Use camelCase for JSON serialization
            options.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase;

            // Include fields with null values
            options.JsonSerializerOptions.DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.Never;

            // Handle circular references
            options.JsonSerializerOptions.ReferenceHandler =
                System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    // ============================================
    // 10. API Documentation - Swagger/OpenAPI
    // ============================================
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Product Management API",
            Version = "v1",
            Description = "E-commerce Product Management System API",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Your Name",
                Email = "your.email@example.com"
            }
        });

        // Include XML comments for better documentation
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        // Add JWT authentication to Swagger
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // ============================================
    // 11. Health Checks
    // ============================================
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("database");

    // ============================================
    // Build Application
    // ============================================
    var app = builder.Build();

    // ============================================
    // 12. Middleware Pipeline
    // ============================================

    // Global exception handling
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    // Swagger UI (only in development)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Management API v1");
            options.RoutePrefix = string.Empty; // Set Swagger UI at app's root
        });
    }

    // HTTPS redirection
    app.UseHttpsRedirection();

    // CORS
    app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "Production");

    // Authentication & Authorization (will add in Day 3)
    // app.UseAuthentication();
    // app.UseAuthorization();

    // Map controllers
    app.MapControllers();

    // Health check endpoint
    app.MapHealthChecks("/health");

    // ============================================
    // 13. Database Migration (Development only)
    // ============================================
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Log.Information("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully");
    }

    // ============================================
    // 14. Run Application
    // ============================================
    Log.Information("Product Management API started successfully");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}