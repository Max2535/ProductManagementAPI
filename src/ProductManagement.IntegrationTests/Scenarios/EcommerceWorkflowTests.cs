using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.IntegrationTests.Helpers;
using Xunit;

namespace ProductManagement.IntegrationTests.Scenarios;

/// <summary>
/// End-to-end scenario tests
/// Tests complete workflows as a user would experience
/// </summary>
public class EcommerceWorkflowTests : IntegrationTestBase
{
    public EcommerceWorkflowTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CompleteProductLifecycle_ShouldWorkEndToEnd()
    {
        // Scenario: Admin creates product, updates it, sets discount, and customer views it

        // 1. Register and login as admin
        var token = await GetAuthTokenAsync("admin@shop.com", "admin", "Admin@123456");
        SetAuthorizationHeader(token);

        // 2. Create category
        var categoryId = CreateTestCategory("Electronics");

        // 3. Create product
        var createRequest = new CreateProductRequest
        {
            Name = "iPhone 15 Pro Max",
            Description = "Latest flagship iPhone with A17 Pro chip",
            SKU = "IPHONE-15-PRO-MAX-256",
            Price = 1199.99m,
            StockQuantity = 50,
            CategoryId = categoryId,
            MinimumStockLevel = 10,
            ImageUrls = new List<string>
            {
                "https://example.com/iphone-front.jpg",
                "https://example.com/iphone-back.jpg"
            }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/Products", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        var productId = createResult!.Data!.Id;

        // 4. Activate product
        var activateResponse = await Client.PatchAsync($"/api/Products/{productId}/activate", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Set discount for promotion
        var discountRequest = new SetDiscountRequest { DiscountPrice = 999.99m };
        var discountResponse = await Client.PatchAsJsonAsync(
            $"/api/Products/{productId}/discount",
            discountRequest);
        discountResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Customer views product (no auth needed for viewing)
        Client.DefaultRequestHeaders.Authorization = null;
        var viewResponse = await Client.GetAsync($"/api/Products/{productId}");
        viewResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var viewResult = await viewResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        viewResult!.Data.Should().NotBeNull();
        viewResult.Data!.Name.Should().Be("iPhone 15 Pro Max");
        viewResult.Data.Price.Should().Be(1199.99m);
        viewResult.Data.DiscountPrice.Should().Be(999.99m);
        viewResult.Data.EffectivePrice.Should().Be(999.99m);
        viewResult.Data.Status.Should().Be("Active");
        viewResult.Data.IsInStock.Should().BeTrue();

        // 7. Simulate purchase - reduce stock
        SetAuthorizationHeader(token);
        var stockRequest = new UpdateStockRequest
        {
            Quantity = 1,
            Operation = StockOperation.Reduce
        };
        var stockResponse = await Client.PatchAsJsonAsync(
            $"/api/Products/{productId}/stock",
            stockRequest);
        stockResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var stockResult = await stockResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        stockResult!.Data!.StockQuantity.Should().Be(49);

        // 8. Search for product
        Client.DefaultRequestHeaders.Authorization = null;
        var searchResponse = await Client.GetAsync(
            "/api/Products/search?searchTerm=iPhone&minPrice=500&maxPrice=1500");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var searchResult = await searchResponse.Content
            .ReadFromJsonAsync<ApiResponse<PagedResult<ProductSummaryDto>>>();
        searchResult!.Data!.Items.Should().Contain(p => p.Id == productId);
    }

    [Fact]
    public async Task UserRegistrationAndProductManagement_ShouldWorkEndToEnd()
    {
        // Scenario: User registers, updates profile, creates product

        // 1. Register new user
        var registerRequest = new RegisterRequest
        {
            Email = "newuser@shop.com",
            UserName = "newshopuser",
            Password = "NewUser@123456",
            ConfirmPassword = "NewUser@123456",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+66812345678"
        };

        var registerResponse = await Client.PostAsJsonAsync("/api/Auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var authResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        authResult!.Data.Should().NotBeNull();
        authResult.Data!.User.Roles.Should().Contain("Customer");

        var token = authResult.Data.AccessToken;
        SetAuthorizationHeader(token);

        // 2. View profile
        var profileResponse = await Client.GetAsync("/api/Auth/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var profileResult = await profileResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        profileResult!.Data!.Email.Should().Be("newuser@shop.com");

        // 3. Update profile
        var updateProfileRequest = new UpdateProfileRequest
        {
            FirstName = "Jonathan",
            LastName = "Doe",
            PhoneNumber = "+66987654321"
        };

        var updateResponse = await Client.PutAsJsonAsync("/api/Auth/profile", updateProfileRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Create product
        var categoryId = CreateTestCategory();
        var createProductRequest = TestDataGenerator.CreateValidProductRequest(categoryId);

        var createProductResponse = await Client.PostAsJsonAsync("/api/Products", createProductRequest);
        createProductResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 5. Logout
        var logoutResponse = await Client.PostAsync("/api/Auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Clear authorization header and try to access protected endpoint (should fail)
        Client.DefaultRequestHeaders.Authorization = null;
        var protectedResponse = await Client.GetAsync("/api/Auth/profile");
        protectedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LowStockScenario_ShouldTriggerAlerts()
    {
        // Scenario: Product stock goes below minimum, should be flagged as low stock

        // 1. Setup
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var categoryId = CreateTestCategory();
        var uniqueId = Guid.NewGuid().ToString("N")[..8].ToUpper();

        // 2. Create product with stock at minimum threshold (will become low stock when reduced)
        var createRequest = new CreateProductRequest
        {
            Name = $"Limited Edition {uniqueId}",
            Description = "Only few left!",
            SKU = $"LIMITED-{uniqueId}",
            Price = 299.99m,
            StockQuantity = 20,
            CategoryId = categoryId,
            MinimumStockLevel = 20 // Equal to current stock - at the threshold
        };

        var createResponse = await Client.PostAsJsonAsync("/api/Products", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        var productId = createResult!.Data!.Id;

        // 3. Activate the product
        await Client.PatchAsync($"/api/Products/{productId}/activate", null);

        // 4. Reduce stock to trigger low stock alert
        var reduceStockRequest = new UpdateStockRequest
        {
            Quantity = 5,
            Operation = StockOperation.Reduce
        };
        await Client.PatchAsJsonAsync($"/api/Products/{productId}/stock", reduceStockRequest);

        // 5. Check product is flagged as low stock (15 < 20)
        var getResponse = await Client.GetAsync($"/api/Products/{productId}");
        var getResult = await getResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        getResult!.Data!.IsLowStock.Should().BeTrue();
        getResult.Data.StockQuantity.Should().Be(15);

        // 6. Get low stock products
        var lowStockResponse = await Client.GetAsync("/api/Products/low-stock");
        lowStockResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var lowStockResult = await lowStockResponse.Content
            .ReadFromJsonAsync<ApiResponse<List<ProductSummaryDto>>>();
        lowStockResult!.Data.Should().Contain(p => p.Id == productId);

        // 7. Add more stock to go above threshold
        var addStockRequest = new UpdateStockRequest
        {
            Quantity = 10,
            Operation = StockOperation.Add
        };

        await Client.PatchAsJsonAsync($"/api/Products/{productId}/stock", addStockRequest);

        // 8. Verify no longer in low stock (25 > 20)
        var updatedResponse = await Client.GetAsync($"/api/Products/{productId}");
        var updatedResult = await updatedResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        updatedResult!.Data!.IsLowStock.Should().BeFalse();
        updatedResult.Data.StockQuantity.Should().Be(25);
    }
}