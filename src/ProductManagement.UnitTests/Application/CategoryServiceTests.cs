using FluentAssertions;
using Moq;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;
using Xunit;

namespace ProductManagement.UnitTests.Application;

/// <summary>
/// TDD Example: Category Service
/// Following Red-Green-Refactor cycle
/// </summary>
public class CategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        // Note: CategoryService doesn't exist yet - this will fail to compile!
        // This is the RED phase
    }

    [Fact]
    public async Task CreateCategoryAsync_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Electronics",
            Description = "Electronic devices and accessories"
        };

        var categoryRepoMock = new Mock<IRepository<Category, Guid>>();
        categoryRepoMock
            .Setup(x => x.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category c, CancellationToken ct) => c);

        _unitOfWorkMock.Setup(x => x.Categories).Returns(categoryRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateCategoryAsync(request, "TestUser");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Electronics");
    }
}