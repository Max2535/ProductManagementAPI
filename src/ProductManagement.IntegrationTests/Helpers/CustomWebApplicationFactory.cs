using AspNetCoreRateLimit;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Services;
using ProductManagement.Infrastructure.Data;

namespace ProductManagement.IntegrationTests.Helpers;

/// <summary>
/// Custom WebApplicationFactory for integration testing
/// Uses in-memory database to avoid external dependencies
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove Redis cache and use in-memory cache
            services.RemoveAll(typeof(Microsoft.Extensions.Caching.Distributed.IDistributedCache));
            services.AddDistributedMemoryCache();

            // Remove Hangfire SQL Server storage and use in-memory storage
            services.RemoveAll(typeof(IGlobalConfiguration));
            services.RemoveAll(typeof(JobStorage));
            services.AddHangfire(config => config.UseMemoryStorage());

            // Remove RabbitMQ services and use test doubles
            services.RemoveAll<IMessagePublisher>();
            services.AddSingleton<IMessagePublisher, TestMessagePublisher>();

            // Remove the hosted RabbitMQ consumer service
            var hostedServiceDescriptor = services.FirstOrDefault(
                d => d.ImplementationType?.Name == "RabbitMQMessageConsumer");
            if (hostedServiceDescriptor != null)
            {
                services.Remove(hostedServiceDescriptor);
            }

            // Disable rate limiting for tests
            services.PostConfigure<IpRateLimitOptions>(options =>
            {
                options.EnableEndpointRateLimiting = false;
                options.GeneralRules = new List<RateLimitRule>();
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Seed test data after host is created
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
        SeedTestData(db);

        return host;
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Seed roles using entities directly (InMemory database doesn't support raw SQL)
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                ProductManagement.Domain.Entities.Role.Create("Admin", "Administrator", "System"),
                ProductManagement.Domain.Entities.Role.Create("Manager", "Manager", "System"),
                ProductManagement.Domain.Entities.Role.Create("Customer", "Customer", "System")
            );
            context.SaveChanges();
        }

        // Seed test category
        if (!context.Categories.Any())
        {
            var category = ProductManagement.Domain.Entities.Category.Create(
                "Test Category",
                "Test Description",
                null,
                "System");
            context.Categories.Add(category);
            context.SaveChanges();
        }
    }
}

/// <summary>
/// Test implementation of IMessagePublisher that does nothing
/// </summary>
public class TestMessagePublisher : IMessagePublisher
{
    public Task PublishOrderCreatedEventAsync(OrderCreatedEvent orderEvent) => Task.CompletedTask;
    public Task PublishStockReservationEventAsync(StockReservationEvent reservationEvent) => Task.CompletedTask;
    public Task PublishStockReleaseEventAsync(StockReleaseEvent releaseEvent) => Task.CompletedTask;
    public Task PublishProductUpdatedEventAsync(ProductUpdatedEvent productEvent) => Task.CompletedTask;
}
