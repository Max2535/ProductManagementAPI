using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Infrastructure.Data.Configurations;

/// <summary>
/// Fluent API Configuration สำหรับ Product
/// Best Practice: แยก configuration ออกจาก DbContext
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Table configuration
        builder.ToTable("Products");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.DiscountPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.Status)
            .HasConversion<string>() // Store as string in DB
            .HasMaxLength(20);

        // Indexes for performance
        builder.HasIndex(p => p.SKU)
            .IsUnique()
            .HasDatabaseName("IX_Products_SKU");

        builder.HasIndex(p => p.Name)
            .HasDatabaseName("IX_Products_Name");

        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("IX_Products_CategoryId");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Products_Status");

        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("IX_Products_CreatedAt");

        // Composite index for common queries
        builder.HasIndex(p => new { p.CategoryId, p.Status, p.Price })
            .HasDatabaseName("IX_Products_CategoryId_Status_Price");

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

        builder.HasMany(p => p.Images)
            .WithOne(pi => pi.Product)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Reviews)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore computed properties (not mapped to DB)
        builder.Ignore(p => p.EffectivePrice);
        builder.Ignore(p => p.IsLowStock);
        builder.Ignore(p => p.IsInStock);
        builder.Ignore(p => p.AverageRating);

        // Seed data
        builder.HasData(GetSeedData());
    }

    private static IEnumerable<object> GetSeedData()
    {
        var categoryId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        return new[]
        {
            new
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Sample Product",
                Description = "This is a sample product",
                SKU = "SAMPLE-001",
                Price = 99.99m,
                DiscountPrice = (decimal?)null,
                StockQuantity = 100,
                MinimumStockLevel = 10,
                Status = ProductStatus.Active,
                CategoryId = categoryId,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "System",
                IsDeleted = false
            }
        };
    }
}