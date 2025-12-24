using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;
using ProductManagement.Infrastructure.Data;

namespace ProductManagement.Infrastructure.Repositories;

/// <summary>
/// Product Repository Implementation
/// ผสมผสานระหว่าง EF Core (CRUD) และ Dapper (Complex queries)
/// </summary>
public class ProductRepository : Repository<Product, Guid>, IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(
        ApplicationDbContext context,
        IConfiguration configuration)
        : base(context)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.SKU == sku, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId && p.Status == ProductStatus.Active)
            .Include(p => p.Images)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.StockQuantity <= p.MinimumStockLevel && p.Status == ProductStatus.Active)
            .Include(p => p.Category)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchAsync(
        string searchTerm,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        Guid? categoryId = null,
        ProductStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Text search
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchLower) ||
                p.Description.ToLower().Contains(searchLower) ||
                p.SKU.ToLower().Contains(searchLower));
        }

        // Price range filter
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice.Value);
        }

        // Category filter
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Status filter
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query
            .Include(p => p.Category)
            .Include(p => p.Images)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Complex query using Dapper for better performance
    /// เหมาะสำหรับ reporting และ analytics
    /// </summary>
    public async Task<IEnumerable<ProductSalesStatistics>> GetTopSellingProductsAsync(
        int topCount,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = new SqlConnection(_connectionString);

        var parameters = new DynamicParameters();
        parameters.Add("@TopCount", topCount);
        parameters.Add("@StartDate", startDate ?? DateTime.UtcNow.AddMonths(-1));
        parameters.Add("@EndDate", endDate ?? DateTime.UtcNow);

        // Using stored procedure
        var results = await connection.QueryAsync<ProductSalesStatistics>(
            "sp_GetTopSellingProducts",
            parameters,
            commandType: CommandType.StoredProcedure);

        return results;
    }
}