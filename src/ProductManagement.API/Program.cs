using System.Reflection;
using System.Text;
using AspNetCoreRateLimit;
using FluentValidation;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductManagement.API.Authorization;
using ProductManagement.API.Middleware;
using ProductManagement.Application.BackgroundJobs;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Mappings;
using ProductManagement.Application.Models;
using ProductManagement.Application.Services;
using ProductManagement.Application.Validators;
using ProductManagement.Domain.Interfaces;
using ProductManagement.Infrastructure.Data;
using ProductManagement.Infrastructure.Repositories;
using ProductManagement.Infrastructure.Services;
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
    // 2. Configuration Settings
    // ============================================
    builder.Services.Configure<JwtSettings>(
        builder.Configuration.GetSection(JwtSettings.SectionName));

    builder.Services.Configure<EmailSettings>(
        builder.Configuration.GetSection(EmailSettings.SectionName));

    var jwtSettings = builder.Configuration
        .GetSection(JwtSettings.SectionName)
        .Get<JwtSettings>() ?? throw new InvalidOperationException("JWT settings not configured");

    // ============================================
    // 3. Redis Cache Configuration
    // ============================================
    var redisConnection = builder.Configuration.GetConnectionString("Redis");
    if (!string.IsNullOrEmpty(redisConnection))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "ProductManagement_";
        });
        builder.Services.AddScoped<ICacheService, RedisCacheService>();
    }
    else
    {
        // Fallback to in-memory cache
        builder.Services.AddMemoryCache();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddScoped<ICacheService, RedisCacheService>();
        Log.Warning("Redis not configured, using in-memory cache");
    }

    // ============================================
    // 4. Rate Limiting Configuration
    // ============================================
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(
        builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.Configure<IpRateLimitPolicies>(
        builder.Configuration.GetSection("IpRateLimitPolicies"));
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    // ============================================
    // 5. Database Configuration
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

        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    // ============================================
    // 6. Hangfire Configuration
    // ============================================
    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"),
            new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

    builder.Services.AddHangfireServer();

    // ============================================
    // 7. Register Repositories & Unit of Work
    // ============================================
    builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // ============================================
    // 8. Register Application Services
    // ============================================
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

    // Register background jobs
    builder.Services.AddScoped<ProductStockAlertJob>();
    builder.Services.AddScoped<CleanupExpiredTokensJob>();

    // ============================================
    // 9. AutoMapper Configuration
    // ============================================
    builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

    // ============================================
    // 10. FluentValidation Configuration
    // ============================================
    builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
    builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

    // ============================================
    // 11. HttpContextAccessor
    // ============================================
    builder.Services.AddHttpContextAccessor();

    // ============================================
    // 12. JWT Authentication
    // ============================================
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Debug("Token validated for user: {User}",
                    context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

    // ============================================
    // 13. Authorization Policies
    // ============================================
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy("ManagerOrAdmin", policy =>
            policy.RequireRole("Admin", "Manager"));

        options.AddPolicy("Authenticated", policy =>
            policy.RequireAuthenticatedUser());
    });

    builder.Services.AddSingleton<IAuthorizationHandler, RoleRequirementHandler>();

    // ============================================
    // 14. CORS Configuration
    // ============================================
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });

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
    // 15. API Controllers & JSON Configuration
    // ============================================
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.Never;
            options.JsonSerializerOptions.ReferenceHandler =
                System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });

    // ============================================
    // 16. Swagger/OpenAPI Configuration
    // ============================================
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Product Management API",
            Version = "v1",
            Description = "E-commerce Product Management System API with Advanced Features",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Your Name",
                Email = "your.email@example.com"
            }
        });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header using the Bearer scheme.
                          Enter 'Bearer' [space] and then your token in the text input below.
                          Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
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
    // 17. Health Checks
    // ============================================
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("database");

    var app = builder.Build();

    // ============================================
    // 18. Middleware Pipeline
    // ============================================
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Rate Limiting
    app.UseIpRateLimiting();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Management API v1");
            options.RoutePrefix = string.Empty;
        });
    }

    // Hangfire Dashboard
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });

    app.UseHttpsRedirection();
    app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "Production");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    // ============================================
    // 19. Database Migration & Seeding
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
    // 20. Schedule Recurring Jobs
    // ============================================
    using (var scope = app.Services.CreateScope())
    {
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        // Check low stock every day at 9 AM
        recurringJobManager.AddOrUpdate<ProductStockAlertJob>(
            "check-low-stock",
            job => job.Execute(),
            "0 9 * * *"); // Cron: Every day at 9 AM

        // Cleanup expired tokens every day at 2 AM
        recurringJobManager.AddOrUpdate<CleanupExpiredTokensJob>(
            "cleanup-expired-tokens",
            job => job.Execute(),
            "0 2 * * *"); // Cron: Every day at 2 AM

        Log.Information("Recurring jobs scheduled successfully");
    }

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

// ============================================
// Hangfire Authorization Filter
// ============================================
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In production, implement proper authorization
        // For now, allow all in development
        return true;
    }
}