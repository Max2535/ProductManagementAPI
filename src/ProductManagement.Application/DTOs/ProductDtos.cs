namespace ProductManagement.Application.DTOs;

/// <summary>
/// DTOs สำหรับ API responses
/// Best Practice: แยก DTOs จาก Domain entities
/// </summary>

// Response DTOs
public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal? DiscountPrice { get; init; }
    public decimal EffectivePrice { get; init; }
    public int StockQuantity { get; init; }
    public bool IsLowStock { get; init; }
    public bool IsInStock { get; init; }
    public string Status { get; init; } = string.Empty;
    public CategoryDto Category { get; init; } = null!;
    public List<ProductImageDto> Images { get; init; } = new();
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ProductSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal? DiscountPrice { get; init; }
    public int StockQuantity { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? PrimaryImageUrl { get; init; }
}

public record ProductImageDto
{
    public Guid Id { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
    public int DisplayOrder { get; init; }
}

public record CategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
}

// Request DTOs
public record CreateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public Guid CategoryId { get; init; }
    public int MinimumStockLevel { get; init; } = 10;
    public List<string>? ImageUrls { get; init; }
}

public record UpdateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
}

public record UpdateStockRequest
{
    public int Quantity { get; init; }
    public StockOperation Operation { get; init; }
}

public enum StockOperation
{
    Set,
    Add,
    Reduce
}

public record SetDiscountRequest
{
    public decimal DiscountPrice { get; init; }
}

public record ProductSearchRequest
{
    public string? SearchTerm { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

// Pagination wrapper
public record PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}