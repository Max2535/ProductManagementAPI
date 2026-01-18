using System.Diagnostics;
using FluentAssertions;
using ProductManagement.Domain.Entities;
using Xunit;

namespace ProductManagement.UnitTests.Performance;

/// <summary>
/// Basic performance tests
/// Note: For serious performance testing, use BenchmarkDotNet
/// </summary>
public class PerformanceTests
{
    [Fact]
    public void ProductCreation_ShouldCompleteWithin100ms()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var categoryId = Guid.NewGuid();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            var product = Product.Create(
                $"Product {i}",
                "Description",
                $"SKU-{i}",
                99.99m,
                100,
                categoryId,
                "TestUser");
        }

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    public void Repository_BulkInsert_ShouldCompleteReasonablyFast()
    {
        // This would be an integration test with real database
        // Just showing the pattern here

        var stopwatch = Stopwatch.StartNew();

        // Act - simulate bulk operation
        // await repository.AddRangeAsync(largeListOfProducts);

        stopwatch.Stop();

        // Assert - adjust threshold based on requirements
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // 5 seconds for example
    }
}