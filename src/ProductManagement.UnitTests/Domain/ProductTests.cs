using FluentAssertions;
using ProductManagement.Domain.Entities;
using ProductManagement.UnitTests.Builders;
using Xunit;

namespace ProductManagement.UnitTests.Domain;

/// <summary>
/// Unit tests for Product entity
/// Tests business logic and domain rules
/// </summary>
public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var name = "iPhone 15 Pro";
        var sku = "IPHONE-15-PRO";
        var price = 999.99m;
        var stock = 100;

        // Act
        var product = Product.Create(
            name,
            "Latest iPhone",
            sku,
            price,
            stock,
            categoryId,
            "TestUser",
            10);

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be(name);
        product.SKU.Should().Be(sku);
        product.Price.Should().Be(price);
        product.StockQuantity.Should().Be(stock);
        product.CategoryId.Should().Be(categoryId);
        product.Status.Should().Be(ProductStatus.Draft);
        product.CreatedBy.Should().Be("TestUser");
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowException(string invalidName)
    {
        // Arrange & Act
        Action act = () => Product.Create(
            invalidName,
            "Description",
            "SKU-001",
            99.99m,
            100,
            Guid.NewGuid(),
            "TestUser");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Product name is required");
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrowException()
    {
        // Act
        Action act = () => Product.Create(
            "Product",
            "Description",
            "SKU-001",
            -10m,
            100,
            Guid.NewGuid(),
            "TestUser");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Price must be greater than zero");
    }

    [Fact]
    public void UpdateStock_WithValidQuantity_ShouldUpdateStock()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(100)
            .Build();

        // Act
        product.UpdateStock(150, "TestUser");

        // Assert
        product.StockQuantity.Should().Be(150);
        product.UpdatedBy.Should().Be("TestUser");
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateStock_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange
        var product = new ProductBuilder().Build();

        // Act
        Action act = () => product.UpdateStock(-10, "TestUser");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Stock quantity cannot be negative");
    }

    [Fact]
    public void AddStock_WithValidQuantity_ShouldIncreaseStock()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(100)
            .Build();

        // Act
        product.AddStock(50, "TestUser");

        // Assert
        product.StockQuantity.Should().Be(150);
    }

    [Fact]
    public void ReduceStock_WithValidQuantity_ShouldDecreaseStock()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(100)
            .Build();

        // Act
        product.ReduceStock(30, "TestUser");

        // Assert
        product.StockQuantity.Should().Be(70);
    }

    [Fact]
    public void ReduceStock_WithQuantityGreaterThanStock_ShouldThrowException()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(50)
            .Build();

        // Act
        Action act = () => product.ReduceStock(100, "TestUser");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient stock");
    }

    [Fact]
    public void SetDiscount_WithValidPrice_ShouldSetDiscountPrice()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithPrice(100m)
            .Build();

        // Act
        product.SetDiscount(80m, "TestUser");

        // Assert
        product.DiscountPrice.Should().Be(80m);
        product.EffectivePrice.Should().Be(80m);
    }

    [Fact]
    public void SetDiscount_WithPriceGreaterThanRegularPrice_ShouldThrowException()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithPrice(100m)
            .Build();

        // Act
        Action act = () => product.SetDiscount(120m, "TestUser");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Discount price must be less than regular price");
    }

    [Fact]
    public void RemoveDiscount_ShouldSetDiscountPriceToNull()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithPrice(100m)
            .Build();
        product.SetDiscount(80m, "TestUser");

        // Act
        product.RemoveDiscount("TestUser");

        // Assert
        product.DiscountPrice.Should().BeNull();
        product.EffectivePrice.Should().Be(100m);
    }

    [Fact]
    public void Activate_WithStock_ShouldSetStatusToActive()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(100)
            .Build();

        // Act
        product.Activate("TestUser");

        // Assert
        product.Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public void Activate_WithNoStock_ShouldThrowException()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithNoStock()
            .Build();

        // Act
        Action act = () => product.Activate("TestUser");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot activate product with zero stock");
    }

    [Fact]
    public void Deactivate_ShouldSetStatusToInactive()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        product.Activate("TestUser");

        // Act
        product.Deactivate("TestUser");

        // Assert
        product.Status.Should().Be(ProductStatus.Inactive);
    }

    [Fact]
    public void IsLowStock_WhenStockBelowMinimum_ShouldReturnTrue()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithLowStock()
            .Build();

        // Assert
        product.IsLowStock.Should().BeTrue();
    }

    [Fact]
    public void IsLowStock_WhenStockAboveMinimum_ShouldReturnFalse()
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(100)
            .Build();

        // Assert
        product.IsLowStock.Should().BeFalse();
    }

    [Fact]
    public void AddImage_WithPrimaryFlag_ShouldSetOtherImagesAsNonPrimary()
    {
        // Arrange
        var product = new ProductBuilder().Build();
        product.AddImage("image1.jpg", true, "TestUser");

        // Act
        product.AddImage("image2.jpg", true, "TestUser");

        // Assert
        product.Images.Should().HaveCount(2);
        product.Images.Count(img => img.IsPrimary).Should().Be(1);
        product.Images.First(img => img.ImageUrl == "image2.jpg")
            .IsPrimary.Should().BeTrue();
    }
}