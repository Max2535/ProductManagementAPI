using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;
using ProductManagement.UnitTests.Builders;
using System.Linq.Expressions;
using Xunit;

namespace ProductManagement.UnitTests.Application;

/// <summary>
/// Unit Tests for CategoryService
/// Follows AAA Pattern (Arrange-Act-Assert)
/// and uses Builder Pattern for test data
/// </summary>
public class CategoryServiceTests
{
    #region Test Fixtures Setup

    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<Category, Guid>> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CategoryService>> _loggerMock;
    private readonly CategoryService _sut; // System Under Test

    public CategoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<IRepository<Category, Guid>>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CategoryService>>();

        _unitOfWorkMock.Setup(x => x.Categories)
            .Returns(_categoryRepositoryMock.Object);

        _sut = new CategoryService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    #endregion

    #region CreateCategoryAsync Tests

    [Fact]
    public async Task CreateCategoryAsync_WithValidData_ShouldReturnSuccess()
    {
        // ============================================
        // ARRANGE - Setup test data and dependencies
        // ============================================
        var request = new CreateCategoryRequest
        {
            Name = "Electronics",
            Description = "Electronic devices and accessories",
            ParentCategoryId = null
        };

        var category = new CategoryBuilder()
            .WithName(request.Name)
            .WithDescription(request.Description)
            .Build();

        var categoryDto = new CategoryDetailDto
        {
            Name = category.Name,
            Description = category.Description
        };

        _categoryRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        _categoryRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<CategoryDetailDto>(It.IsAny<Category>()))
            .Returns(categoryDto);

        // ============================================
        // ACT - Execute the method being tested
        // ============================================
        var result = await _sut.CreateCategoryAsync(request, "TestUser");

        // ============================================
        // ASSERT - Verify the results
        // ============================================
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Electronics");

        // Verify interactions
        _categoryRepositoryMock.Verify(
            x => x.FindAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "Should check if category already exists");

        _categoryRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "Should add the new category");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "Should save changes to database");
    }

    [Fact]
    public async Task CreateCategoryAsync_WithExistingName_ShouldReturnFailure()
    {
        // ============================================
        // ARRANGE - Setup test data and dependencies
        // ============================================
        var request = new CreateCategoryRequest
        {
            Name = "Electronics",
            Description = "Electronic devices",
            ParentCategoryId = null
        };

        var existingCategory = new CategoryBuilder()
            .WithName("Electronics")
            .Build();

        _categoryRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category> { existingCategory });

        // ============================================
        // ACT - Execute the method being tested
        // ============================================
        var result = await _sut.CreateCategoryAsync(request, "TestUser");

        // ============================================
        // ASSERT - Verify the results
        // ============================================
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("already exists");

        // Verify that add was never called
        _categoryRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Should not add category if it already exists");

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "Should not save if validation fails");
    }

    [Fact]
    public async Task CreateCategoryAsync_OnDatabaseError_ShouldReturnFailure()
    {
        // ============================================
        // ARRANGE - Setup test data and dependencies
        // ============================================
        var request = new CreateCategoryRequest
        {
            Name = "Electronics",
            Description = "Electronic devices",
            ParentCategoryId = null
        };

        var databaseException = new InvalidOperationException("Database connection failed");

        _categoryRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(databaseException);

        // ============================================
        // ACT - Execute the method being tested
        // ============================================
        var result = await _sut.CreateCategoryAsync(request, "TestUser");

        // ============================================
        // ASSERT - Verify the results
        // ============================================
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Failed to create category");
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnCategory()
    {
        // ============================================
        // ARRANGE - Setup test data and dependencies
        // ============================================
        var categoryId = Guid.NewGuid();
        var category = new CategoryBuilder()
            .WithId(categoryId)
            .WithName("Electronics")
            .Build();

        var categoryDto = new CategoryDetailDto
        {
            Name = category.Name,
            Description = category.Description
        };

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mapperMock
            .Setup(x => x.Map<CategoryDetailDto>(It.IsAny<Category>()))
            .Returns(categoryDto);

        // ============================================
        // ACT - Execute the method being tested
        // ============================================
        var result = await _sut.GetByIdAsync(categoryId);

        // ============================================
        // ASSERT - Verify the results
        // ============================================
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNotFound()
    {
        // ============================================
        // ARRANGE - Setup test data and dependencies
        // ============================================
        var categoryId = Guid.NewGuid();

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // ============================================
        // ACT - Execute the method being tested
        // ============================================
        var result = await _sut.GetByIdAsync(categoryId);

        // ============================================
        // ASSERT - Verify the results
        // ============================================
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithMultipleCategories_ShouldReturnAll()
    {
        // ============================================
        // ARRANGE - Setup test data and dependencies
        // ============================================
        var categories = new List<Category>
        {
            new CategoryBuilder().WithName("Electronics").Build(),
            new CategoryBuilder().WithName("Furniture").Build(),
            new CategoryBuilder().WithName("Clothing").Build()
        };

        var categoryDtos = new List<CategoryDto>
        {
            new CategoryDto { Name = "Electronics" },
            new CategoryDto { Name = "Furniture" },
            new CategoryDto { Name = "Clothing" }
        };

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        _mapperMock
            .Setup(x => x.Map<List<CategoryDto>>(It.IsAny<IEnumerable<Category>>()))
            .Returns(categoryDtos);

        // ============================================
        // ACT - Execute the method being tested
        // ============================================
        var result = await _sut.GetAllAsync();

        // ============================================
        // ASSERT - Verify the results
        // ============================================
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(3);
        result.Data.Should().Contain(c => c.Name == "Electronics");
        result.Data.Should().Contain(c => c.Name == "Furniture");
    }

    [Fact]
    public async Task GetAllAsync_WithNoCategories_ShouldReturnEmpty()
    {
        // ============================================
        // ARRANGE - Setup test data and dependencies
        // ============================================
        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        _mapperMock
            .Setup(x => x.Map<List<CategoryDto>>(It.IsAny<IEnumerable<Category>>()))
            .Returns(new List<CategoryDto>());

        // ============================================
        // ACT - Execute the method being tested
        // ============================================
        var result = await _sut.GetAllAsync();

        // ============================================
        // ASSERT - Verify the results
        // ============================================
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    #endregion
}