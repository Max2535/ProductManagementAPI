using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;

namespace ProductManagement.Application.Interfaces;

/// <summary>
/// Product Service Interface
/// Best Practice: Program to interface, not implementation
/// </summary>
public interface IProductService
{
    // Query operations
    Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ProductSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ProductSummaryDto>>> SearchAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken = default);
    Task<Result<List<ProductSummaryDto>>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);
    Task<Result<List<ProductSummaryDto>>> GetLowStockProductsAsync(
        CancellationToken cancellationToken = default);

    // Command operations
    Task<Result<ProductDto>> CreateAsync(
        CreateProductRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(
        Guid id,
        string deletedBy,
        CancellationToken cancellationToken = default);

    // Stock operations
    Task<Result<ProductDto>> UpdateStockAsync(
        Guid id,
        UpdateStockRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    // Discount operations
    Task<Result<ProductDto>> SetDiscountAsync(
        Guid id,
        SetDiscountRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> RemoveDiscountAsync(
        Guid id,
        string updatedBy,
        CancellationToken cancellationToken = default);

    // Status operations
    Task<Result<ProductDto>> ActivateAsync(
        Guid id,
        string updatedBy,
        CancellationToken cancellationToken = default);
    Task<Result<ProductDto>> DeactivateAsync(
        Guid id,
        string updatedBy,
        CancellationToken cancellationToken = default);
}