using ProductManagement.Domain.Entities;

namespace ProductManagement.Domain.Interfaces;

/// <summary>
/// Product-specific repository interface
/// เพิ่ม methods ที่เฉพาะเจาะจงสำหรับ Product
/// </summary>
public interface IProductRepository : IRepository<Product, Guid>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchAsync(
        string searchTerm,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        Guid? categoryId = null,
        ProductStatus? status = null,
        CancellationToken cancellationToken = default);

    // Using Dapper for complex queries (performance critical)
    Task<IEnumerable<ProductSalesStatistics>> GetTopSellingProductsAsync(
        int topCount,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
}

public class ProductSalesStatistics
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}