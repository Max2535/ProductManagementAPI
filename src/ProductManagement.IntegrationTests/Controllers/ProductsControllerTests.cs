using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.IntegrationTests.Helpers;
using Xunit;

namespace ProductManagement.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for Products endpoints
/// </summary>
public class ProductsControllerTests : IntegrationTestBase
{
    public ProductsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProducts_ShouldReturn200OK()
    {
        // Act
        var response = await Client.GetAsync("/api/Products?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductSummaryDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var categoryId = CreateTestCategory();
        var request = TestDataGenerator.CreateValidProductRequest(categoryId);

        // Act
        var response = await Client.PostAsJsonAsync("/api/Products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(request.Name);
        result.Data.SKU.Should().Be(request.SKU);
        result.Data.Price.Should().Be(request.Price);

        // Verify Location header
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProduct_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Arrange
        var request = TestDataGenerator.CreateValidProductRequest();

        // Act
        var response = await Client.PostAsJsonAsync("/api/Products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ShouldReturn400BadRequest()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var request = TestDataGenerator.CreateValidProductRequest();
        request = request with { Name = "", Price = -10 }; // Invalid data

        // Act
        var response = await Client.PostAsJsonAsync("/api/Products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetProductById_WithExistingProduct_ShouldReturn200OK()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var categoryId = CreateTestCategory();
        var createRequest = TestDataGenerator.CreateValidProductRequest(categoryId);
        var createResponse = await Client.PostAsJsonAsync("/api/Products", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        var productId = createResult!.Data!.Id;

        // Act
        var response = await Client.GetAsync($"/api/Products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(productId);
    }

    [Fact]
    public async Task GetProductById_WithNonExistentProduct_ShouldReturn404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/Products/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldReturn200OK()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var categoryId = CreateTestCategory();
        var createRequest = TestDataGenerator.CreateValidProductRequest(categoryId);
        var createResponse = await Client.PostAsJsonAsync("/api/Products", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        var productId = createResult!.Data!.Id;

        var updateRequest = new UpdateProductRequest
        {
            Name = "Updated Product Name",
            Description = "Updated description",
            Price = 149.99m
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/Products/{productId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Updated Product Name");
        result.Data.Price.Should().Be(149.99m);
    }

    [Fact]
    public async Task DeleteProduct_WithExistingProduct_ShouldReturn200OK()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var categoryId = CreateTestCategory();
        var createRequest = TestDataGenerator.CreateValidProductRequest(categoryId);
        var createResponse = await Client.PostAsJsonAsync("/api/Products", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        var productId = createResult!.Data!.Id;

        // Act
        var response = await Client.DeleteAsync($"/api/Products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify product is deleted (soft delete)
        var getResponse = await Client.GetAsync($"/api/Products/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateStock_WithValidData_ShouldReturn200OK()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var categoryId = CreateTestCategory();
        var createRequest = TestDataGenerator.CreateValidProductRequest(categoryId);
        var createResponse = await Client.PostAsJsonAsync("/api/Products", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        var productId = createResult!.Data!.Id;

        var stockRequest = new UpdateStockRequest
        {
            Quantity = 50,
            Operation = StockOperation.Add
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/Products/{productId}/stock", stockRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.StockQuantity.Should().Be(150); // 100 + 50
    }

    [Fact]
    public async Task SearchProducts_WithSearchTerm_ShouldReturnMatchingProducts()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var categoryId = CreateTestCategory();

        // Create multiple products
        for (int i = 0; i < 3; i++)
        {
            var request = TestDataGenerator.CreateValidProductRequest(categoryId);
            request = request with { Name = $"iPhone {i + 15}" };
            await Client.PostAsJsonAsync("/api/Products", request);
        }

        // Act
        var response = await Client.GetAsync("/api/Products/search?searchTerm=iPhone&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ProductSummaryDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Items.Should().NotBeEmpty();
        result.Data.Items.Should().OnlyContain(p => p.Name.Contains("iPhone"));
    }

    [Fact]
    public async Task SetDiscount_WithValidData_ShouldReturn200OK()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var categoryId = CreateTestCategory();
        var createRequest = TestDataGenerator.CreateValidProductRequest(categoryId);
        createRequest = createRequest with { Price = 100m };
        var createResponse = await Client.PostAsJsonAsync("/api/Products", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        var productId = createResult!.Data!.Id;

        var discountRequest = new SetDiscountRequest
        {
            DiscountPrice = 80m
        };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/Products/{productId}/discount", discountRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.DiscountPrice.Should().Be(80m);
        result.Data.EffectivePrice.Should().Be(80m);
    }

    [Fact]
    public async Task ActivateProduct_WithStock_ShouldReturn200OK()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var categoryId = CreateTestCategory();
        var createRequest = TestDataGenerator.CreateValidProductRequest(categoryId);
        var createResponse = await Client.PostAsJsonAsync("/api/Products", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        var productId = createResult!.Data!.Id;

        // Act
        var response = await Client.PatchAsync($"/api/Products/{productId}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Status.Should().Be("Active");
    }
}