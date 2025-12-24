namespace ProductManagement.Domain.Entities;

/// <summary>
/// Product entity - ตัวอย่าง Rich Domain Model
/// มี business logic และ validation อยู่ใน entity เอง
/// </summary>
public class Product : BaseEntity<Guid>
{
    private readonly List<ProductImage> _images = new();
    private readonly List<Review> _reviews = new();

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public decimal? DiscountPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public int MinimumStockLevel { get; private set; }
    public ProductStatus Status { get; private set; }

    // Navigation Properties
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;

    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    // Computed Properties
    public decimal EffectivePrice => DiscountPrice ?? Price;
    public bool IsLowStock => StockQuantity <= MinimumStockLevel;
    public bool IsInStock => StockQuantity > 0 && Status == ProductStatus.Active;
    public double AverageRating => _reviews.Any() ? _reviews.Average(r => r.Rating) : 0;

    // Factory Method Pattern
    public static Product Create(
        string name,
        string description,
        string sku,
        decimal price,
        int stockQuantity,
        Guid categoryId,
        string createdBy,
        int minimumStockLevel = 10)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            SKU = sku,
            Price = price,
            StockQuantity = stockQuantity,
            CategoryId = categoryId,
            MinimumStockLevel = minimumStockLevel,
            Status = ProductStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        product.Validate();
        return product;
    }

    // Business Logic Methods
    public void UpdateInfo(string name, string description, decimal price, string updatedBy)
    {
        Name = name;
        Description = description;
        Price = price;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;

        Validate();
    }

    public void UpdateStock(int quantity, string updatedBy)
    {
        if (quantity < 0)
            throw new InvalidOperationException("Stock quantity cannot be negative");

        StockQuantity = quantity;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;

        // Auto-update status based on stock
        if (StockQuantity == 0 && Status == ProductStatus.Active)
            Status = ProductStatus.OutOfStock;
        else if (StockQuantity > 0 && Status == ProductStatus.OutOfStock)
            Status = ProductStatus.Active;
    }

    public void AddStock(int quantity, string updatedBy)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be positive");

        UpdateStock(StockQuantity + quantity, updatedBy);
    }

    public void ReduceStock(int quantity, string updatedBy)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be positive");

        if (StockQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock");

        UpdateStock(StockQuantity - quantity, updatedBy);
    }

    public void SetDiscount(decimal discountPrice, string updatedBy)
    {
        if (discountPrice >= Price)
            throw new InvalidOperationException("Discount price must be less than regular price");

        if (discountPrice < 0)
            throw new InvalidOperationException("Discount price cannot be negative");

        DiscountPrice = discountPrice;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void RemoveDiscount(string updatedBy)
    {
        DiscountPrice = null;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Activate(string updatedBy)
    {
        if (StockQuantity == 0)
            throw new InvalidOperationException("Cannot activate product with zero stock");

        Status = ProductStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Deactivate(string updatedBy)
    {
        Status = ProductStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void AddImage(string url, bool isPrimary, string addedBy)
    {
        if (isPrimary)
        {
            // Set all existing images as non-primary
            foreach (var img in _images)
                img.SetAsNonPrimary();
        }

        var image = ProductImage.Create(Id, url, isPrimary, addedBy);
        _images.Add(image);
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException("Product name is required");

        if (Name.Length > 200)
            throw new InvalidOperationException("Product name cannot exceed 200 characters");

        if (string.IsNullOrWhiteSpace(SKU))
            throw new InvalidOperationException("SKU is required");

        if (Price <= 0)
            throw new InvalidOperationException("Price must be greater than zero");

        if (StockQuantity < 0)
            throw new InvalidOperationException("Stock quantity cannot be negative");
    }
}

public enum ProductStatus
{
    Draft = 0,
    Active = 1,
    Inactive = 2,
    OutOfStock = 3,
    Discontinued = 4
}