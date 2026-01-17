using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Mappings;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;
using ProductManagement.UnitTests.Builders;
using Xunit;

namespace ProductManagement.UnitTests.Application;

/// <summary>
/// Unit tests for ProductService
/// Uses Moq to mock dependencies
/// </summary>
public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _sut; // System Under Test

    public ProductServiceTests()
    {
        // Setup mocks
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProductService>>();

        // Setup real mapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        // Create service instance
        _sut = new ProductService(
            _unitOfWorkMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingProduct_ShouldReturnProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductBuilder()
            .WithId(productId)
            .WithName("Test Product")
            .Build();

        var productRepositoryMock = new Mock<IProductRepository>();
        productRepositoryMock
            .Setup(x => x.GetByIdWithIncludesAsync(
                productId,
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);

        // Act
        var result = await _sut.GetByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(productId);
        result.Data.Name.Should().Be("Test Product");

        // Verify repository was called
        productRepositoryMock.Verify(
            x => x.GetByIdWithIncludesAsync(
                productId,
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>[]>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingProduct_ShouldReturnFailure()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var productRepositoryMock = new Mock<IProductRepository>();
        productRepositoryMock
            .Setup(x => x.GetByIdWithIncludesAsync(
                productId,
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>[]>()))
            .ReturnsAsync((Product?)null);

        _unitOfWorkMock
            .Setup(x => x.Products)
            .Returns(productRepositoryMock.Object);

        // Act
        var result = await _sut.GetByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Product not found");
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreateProduct()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new CreateProductRequest
        {
            Name = "New Product",
            Description = "Description",
            SKU = "NEW-001",
            Price = 99.99m,
            StockQuantity = 100,
            CategoryId = categoryId,
            MinimumStockLevel = 10
        };

        var category = Category.Create("Category", "Description", null, "TestUser");

        var productRepositoryMock = new Mock<IProductRepository>();
        productRepositoryMock
            .Setup(x => x.GetBySkuAsync(request.SKU, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        productRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken ct) => p);

        productRepositoryMock
            .Setup(x => x.GetByIdWithIncludesAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>[]>()))
            .ReturnsAsync((Guid id, CancellationToken ct,
                System.Linq.Expressions.Expression<Func<Product, object>>[] includes) =>
            {
                var product = new ProductBuilder()
                    .WithId(id)
                    .WithName(request.Name)
                    .WithSKU(request.SKU)
                    .WithPrice(request.Price)
                    .WithStock(request.StockQuantity)
                    .Build();
                return product;
            });

        var categoryRepositoryMock = new Mock<IRepository<Category, Guid>>();
        categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _unitOfWorkMock.Setup(x => x.Products).Returns(productRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Categories).Returns(categoryRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(request, "TestUser");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(request.Name);
        result.Data.SKU.Should().Be(request.SKU);

        // Verify calls
        productRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateSKU_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "New Product",
            SKU = "EXISTING-SKU",
            Price = 99.99m,
            StockQuantity = 100,
            CategoryId = Guid.NewGuid()
        };

        var existingProduct = new ProductBuilder()
            .WithSKU(request.SKU)
            .Build();

        var productRepositoryMock = new Mock<IProductRepository>();
        productRepositoryMock
            .Setup(x => x.GetBySkuAsync(request.SKU, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _unitOfWorkMock.Setup(x => x.Products).Returns(productRepositoryMock.Object);

        // Act
        var result = await _sut.CreateAsync(request, "TestUser");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("already exists");

        // Verify AddAsync was never called
        productRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateStockAsync_WithValidQuantity_ShouldUpdateStock()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductBuilder()
            .WithId(productId)
            .WithStock(100)
            .Build();

        var request = new UpdateStockRequest
        {
            Quantity = 50,
            Operation = StockOperation.Add
        };

        var productRepositoryMock = new Mock<IProductRepository>();
        productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        productRepositoryMock
            .Setup(x => x.GetByIdWithIncludesAsync(
                productId,
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(product);

        _unitOfWorkMock.Setup(x => x.Products).Returns(productRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.UpdateStockAsync(productId, request, "TestUser");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        product.StockQuantity.Should().Be(150);

        productRepositoryMock.Verify(
            x => x.UpdateAsync(product, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingProduct_ShouldSoftDelete()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductBuilder()
            .WithId(productId)
            .Build();

        var productRepositoryMock = new Mock<IProductRepository>();
        productRepositoryMock
            .Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock.Setup(x => x.Products).Returns(productRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.DeleteAsync(productId, "TestUser");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        productRepositoryMock.Verify(
            x => x.DeleteAsync(product, It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}