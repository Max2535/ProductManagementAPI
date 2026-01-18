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

    [Theory]
    [InlineData(-10)]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Create_WithInvalidPrice_ShouldThrowException(decimal invalidPrice)
    {
        // Act
        Action act = () => Product.Create(
            "Product",
            "Description",
            "SKU-001",
            invalidPrice,
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

    [Theory]
    [InlineData(-10)]       // Negative
    [InlineData(-1)]        // Minimum negative
    public void UpdateStock_WithNegativeQuantity_ShouldThrowException(int negativeQuantity)
    {
        // Arrange
        var product = new ProductBuilder().Build();

        // Act
        Action act = () => product.UpdateStock(negativeQuantity, "TestUser");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Stock quantity cannot be negative");
    }

    [Theory]
    [InlineData(0)]         // Boundary: zero stock
    [InlineData(1)]         // Boundary: minimum valid
    [InlineData(10000)]     // High value
    public void UpdateStock_WithBoundaryQuantities_ShouldUpdateStock(int quantity)
    {
        // Arrange
        var product = new ProductBuilder().Build();

        // Act
        product.UpdateStock(quantity, "TestUser");

        // Assert
        product.StockQuantity.Should().Be(quantity);
    }

    [Theory]
    [InlineData(1, 101)]    // AddStock: minimum valid
    [InlineData(50, 150)]   // AddStock: normal case
    [InlineData(100, 200)]  // AddStock: double stock
    [InlineData(1000, 1100)]// AddStock: large value
    public void AddStock_WithValidQuantity_ShouldIncreaseStock(int amountToAdd, int expectedStock)
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(100)
            .Build();

        // Act
        product.AddStock(amountToAdd, "TestUser");

        // Assert
        product.StockQuantity.Should().Be(expectedStock);
    }

    [Theory]
    [InlineData(0)]         // AddStock: zero not allowed
    [InlineData(-1)]        // AddStock: negative not allowed
    [InlineData(-100)]      // AddStock: large negative
    public void AddStock_WithInvalidQuantity_ShouldThrowException(int invalidQuantity)
    {
        // Arrange
        var product = new ProductBuilder().Build();

        // Act
        Action act = () => product.AddStock(invalidQuantity, "TestUser");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Quantity must be positive");
    }

    [Theory]
    [InlineData(1, 99)]     // ReduceStock: minimum reduce
    [InlineData(30, 70)]    // ReduceStock: normal case
    [InlineData(50, 50)]    // ReduceStock: half stock
    [InlineData(100, 0)]    // ReduceStock: all stock (boundary)
    public void ReduceStock_WithValidQuantity_ShouldDecreaseStock(int amountToReduce, int expectedStock)
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(100)
            .Build();

        // Act
        product.ReduceStock(amountToReduce, "TestUser");

        // Assert
        product.StockQuantity.Should().Be(expectedStock);
    }

    [Theory]
    [InlineData(0)]         // ReduceStock: zero not allowed
    [InlineData(-1)]        // ReduceStock: negative not allowed
    [InlineData(-100)]      // ReduceStock: large negative
    public void ReduceStock_WithInvalidQuantity_ShouldThrowException(int invalidQuantity)
    {
        // Arrange
        var product = new ProductBuilder().Build();

        // Act
        Action act = () => product.ReduceStock(invalidQuantity, "TestUser");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Quantity must be positive");
    }

    [Theory]
    [InlineData(101)]       // Exceeds stock by 1
    [InlineData(200)]       // Double the stock
    [InlineData(int.MaxValue)]  // Maximum value
    public void ReduceStock_WithQuantityGreaterThanStock_ShouldThrowException(int quantityToReduce)
    {
        // Arrange
        var product = new ProductBuilder()
            .WithStock(100)
            .Build();

        // Act
        Action act = () => product.ReduceStock(quantityToReduce, "TestUser");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient stock");
    }

    [Theory]
    [InlineData(0.01, 100)]      // SetDiscount: minimum valid price
    [InlineData(50, 100)]        // SetDiscount: 50% discount
    [InlineData(99.99, 100)]     // SetDiscount: near full price
    [InlineData(1, 100)]         // SetDiscount: minimal discount
    public void SetDiscount_WithValidPrice_ShouldSetDiscountPrice(decimal discountPrice, decimal regularPrice)
    {
        // Arrange
        var product = new ProductBuilder()
            .WithPrice(regularPrice)
            .Build();

        // Act
        product.SetDiscount(discountPrice, "TestUser");

        // Assert
        product.DiscountPrice.Should().Be(discountPrice);
        product.EffectivePrice.Should().Be(discountPrice);
    }

    [Theory]
    [InlineData(100, 100)]       // SetDiscount: equal to regular price
    [InlineData(120, 100)]       // SetDiscount: greater than regular price
    [InlineData(1000, 100)]      // SetDiscount: much greater
    public void SetDiscount_WithInvalidPrice_ShouldThrowException(decimal discountPrice, decimal regularPrice)
    {
        // Arrange
        var product = new ProductBuilder()
            .WithPrice(regularPrice)
            .Build();

        // Act
        Action act = () => product.SetDiscount(discountPrice, "TestUser");

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