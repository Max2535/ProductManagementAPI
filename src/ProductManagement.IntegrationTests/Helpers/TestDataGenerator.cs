using ProductManagement.Application.DTOs;

namespace ProductManagement.IntegrationTests.Helpers;

/// <summary>
/// Helper class to generate test data
/// </summary>
public static class TestDataGenerator
{
    public static CreateProductRequest CreateValidProductRequest(Guid? categoryId = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8].ToUpper();
        return new CreateProductRequest
        {
            Name = $"Test Product {uniqueId}",
            Description = "Test product description",
            SKU = $"TEST-{uniqueId}",
            Price = 99.99m,
            StockQuantity = 100,
            CategoryId = categoryId ?? Guid.Parse("99999999-9999-9999-9999-999999999999"),
            MinimumStockLevel = 10,
            ImageUrls = new List<string>
            {
                "https://example.com/image1.jpg",
                "https://example.com/image2.jpg"
            }
        };
    }

    public static RegisterRequest CreateValidRegisterRequest(string? email = null)
    {
        var uniqueId = Guid.NewGuid().ToString()[..8];
        return new RegisterRequest
        {
            Email = email ?? $"user{uniqueId}@example.com",
            UserName = $"user{uniqueId}",
            Password = "Test@123456",
            ConfirmPassword = "Test@123456",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "+66812345678"
        };
    }

    public static LoginRequest CreateValidLoginRequest(
        string emailOrUsername = "test@example.com",
        string password = "Test@123456")
    {
        return new LoginRequest
        {
            EmailOrUsername = emailOrUsername,
            Password = password
        };
    }
}