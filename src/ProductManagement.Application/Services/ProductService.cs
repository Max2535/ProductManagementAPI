using AutoMapper;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Exceptions;
using ProductManagement.Application.Interfaces;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;

namespace ProductManagement.Application.Services;

/// <summary>
/// Product Service Implementation
/// ประกอบด้วย:
/// - Business logic
/// - Transaction management
/// - Error handling
/// - Logging
/// - Mapping between entities and DTOs
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region Query Operations

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);

            var product = await _unitOfWork.Products.GetByIdWithIncludesAsync(
                id,
                cancellationToken,
                p => p.Category,
                p => p.Images,
                p => p.Reviews);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return Result<ProductDto>.Failure("Product not found");
            }

            var productDto = _mapper.Map<ProductDto>(product);
            return Result<ProductDto>.Success(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with ID: {ProductId}", id);
            return Result<ProductDto>.Failure("An error occurred while retrieving the product", ex.Message);
        }
    }

    public async Task<Result<ProductDto>> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting product with SKU: {SKU}", sku);

            var product = await _unitOfWork.Products.GetBySkuAsync(sku, cancellationToken);

            if (product == null)
            {
                _logger.LogWarning("Product with SKU {SKU} not found", sku);
                return Result<ProductDto>.Failure("Product not found");
            }

            var productDto = _mapper.Map<ProductDto>(product);
            return Result<ProductDto>.Success(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with SKU: {SKU}", sku);
            return Result<ProductDto>.Failure("An error occurred while retrieving the product", ex.Message);
        }
    }

    public async Task<Result<PagedResult<ProductSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting paged products - Page: {PageNumber}, Size: {PageSize}",
                pageNumber, pageSize);

            var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(
                pageNumber,
                pageSize,
                filter: p => p.Status == ProductStatus.Active,
                orderBy: q => q.OrderBy(p => p.Name),
                cancellationToken);

            var productDtos = _mapper.Map<List<ProductSummaryDto>>(items);

            var pagedResult = new PagedResult<ProductSummaryDto>
            {
                Items = productDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Result<PagedResult<ProductSummaryDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged products");
            return Result<PagedResult<ProductSummaryDto>>.Failure(
                "An error occurred while retrieving products", ex.Message);
        }
    }

    public async Task<Result<PagedResult<ProductSummaryDto>>> SearchAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching products with term: {SearchTerm}", request.SearchTerm);

            // Parse status if provided
            ProductStatus? status = null;
            if (!string.IsNullOrWhiteSpace(request.Status) &&
                Enum.TryParse<ProductStatus>(request.Status, true, out var parsedStatus))
            {
                status = parsedStatus;
            }

            var products = await _unitOfWork.Products.SearchAsync(
                request.SearchTerm ?? string.Empty,
                request.MinPrice,
                request.MaxPrice,
                request.CategoryId,
                status,
                cancellationToken);

            var productList = products.ToList();
            var totalCount = productList.Count;

            // Apply pagination
            var pagedProducts = productList
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var productDtos = _mapper.Map<List<ProductSummaryDto>>(pagedProducts);

            var pagedResult = new PagedResult<ProductSummaryDto>
            {
                Items = productDtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            return Result<PagedResult<ProductSummaryDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return Result<PagedResult<ProductSummaryDto>>.Failure(
                "An error occurred while searching products", ex.Message);
        }
    }

    public async Task<Result<List<ProductSummaryDto>>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting products for category: {CategoryId}", categoryId);

            // Verify category exists
            var categoryExists = await _unitOfWork.Categories.AnyAsync(
                c => c.Id == categoryId,
                cancellationToken);

            if (!categoryExists)
            {
                return Result<List<ProductSummaryDto>>.Failure("Category not found");
            }

            var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId, cancellationToken);
            var productDtos = _mapper.Map<List<ProductSummaryDto>>(products);

            return Result<List<ProductSummaryDto>>.Success(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by category: {CategoryId}", categoryId);
            return Result<List<ProductSummaryDto>>.Failure(
                "An error occurred while retrieving products", ex.Message);
        }
    }

    public async Task<Result<List<ProductSummaryDto>>> GetLowStockProductsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting low stock products");

            var products = await _unitOfWork.Products.GetLowStockProductsAsync(cancellationToken);
            var productDtos = _mapper.Map<List<ProductSummaryDto>>(products);

            return Result<List<ProductSummaryDto>>.Success(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock products");
            return Result<List<ProductSummaryDto>>.Failure(
                "An error occurred while retrieving low stock products", ex.Message);
        }
    }

    #endregion

    #region Command Operations

    public async Task<Result<ProductDto>> CreateAsync(
        CreateProductRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating product with SKU: {SKU}", request.SKU);

            // Check if SKU already exists
            var existingProduct = await _unitOfWork.Products.GetBySkuAsync(request.SKU, cancellationToken);
            if (existingProduct != null)
            {
                return Result<ProductDto>.Failure($"Product with SKU '{request.SKU}' already exists");
            }

            // Verify category exists
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
            {
                return Result<ProductDto>.Failure("Category not found");
            }

            // Create product using factory method (Domain-driven design)
            var product = Product.Create(
                request.Name,
                request.Description,
                request.SKU,
                request.Price,
                request.StockQuantity,
                request.CategoryId,
                createdBy,
                request.MinimumStockLevel);

            // Add images if provided
            if (request.ImageUrls != null && request.ImageUrls.Any())
            {
                for (var i = 0; i < request.ImageUrls.Count; i++)
                {
                    product.AddImage(request.ImageUrls[i], i == 0, createdBy);
                }
            }

            // Save to database
            await _unitOfWork.Products.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);

            // Reload with includes for mapping
            var createdProduct = await _unitOfWork.Products.GetByIdWithIncludesAsync(
                product.Id,
                cancellationToken,
                p => p.Category,
                p => p.Images);

            var productDto = _mapper.Map<ProductDto>(createdProduct);
            return Result<ProductDto>.Success(productDto, "Product created successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating product");
            return Result<ProductDto>.Failure("Validation error", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return Result<ProductDto>.Failure("An error occurred while creating the product", ex.Message);
        }
    }

    public async Task<Result<ProductDto>> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating product with ID: {ProductId}", id);

            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return Result<ProductDto>.Failure("Product not found");
            }

            // Update using domain method
            product.UpdateInfo(request.Name, request.Description, request.Price, updatedBy);

            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product updated successfully: {ProductId}", id);

            // Reload with includes
            var updatedProduct = await _unitOfWork.Products.GetByIdWithIncludesAsync(
                id,
                cancellationToken,
                p => p.Category,
                p => p.Images);

            var productDto = _mapper.Map<ProductDto>(updatedProduct);
            return Result<ProductDto>.Success(productDto, "Product updated successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating product: {ProductId}", id);
            return Result<ProductDto>.Failure("Validation error", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product: {ProductId}", id);
            return Result<ProductDto>.Failure("An error occurred while updating the product", ex.Message);
        }
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);

            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return Result.Failure("Product not found");
            }

            // Soft delete (handled by DbContext)
            await _unitOfWork.Products.DeleteAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product deleted successfully: {ProductId}", id);
            return Result.Success("Product deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product: {ProductId}", id);
            return Result.Failure("An error occurred while deleting the product", ex.Message);
        }
    }

    #endregion

    #region Stock Operations

    public async Task<Result<ProductDto>> UpdateStockAsync(
        Guid id,
        UpdateStockRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Updating stock for product {ProductId} - Operation: {Operation}, Quantity: {Quantity}",
                id, request.Operation, request.Quantity);

            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return Result<ProductDto>.Failure("Product not found");
            }

            // Apply stock operation using domain methods
            switch (request.Operation)
            {
                case StockOperation.Set:
                    product.UpdateStock(request.Quantity, updatedBy);
                    break;
                case StockOperation.Add:
                    product.AddStock(request.Quantity, updatedBy);
                    break;
                case StockOperation.Reduce:
                    product.ReduceStock(request.Quantity, updatedBy);
                    break;
                default:
                    return Result<ProductDto>.Failure("Invalid stock operation");
            }

            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Stock updated successfully for product: {ProductId}", id);

            // Reload with includes
            var updatedProduct = await _unitOfWork.Products.GetByIdWithIncludesAsync(
                id,
                cancellationToken,
                p => p.Category,
                p => p.Images);

            var productDto = _mapper.Map<ProductDto>(updatedProduct);
            return Result<ProductDto>.Success(productDto, "Stock updated successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating stock for product: {ProductId}", id);
            return Result<ProductDto>.Failure("Validation error", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for product: {ProductId}", id);
            return Result<ProductDto>.Failure("An error occurred while updating stock", ex.Message);
        }
    }

    #endregion

    #region Discount Operations

    public async Task<Result<ProductDto>> SetDiscountAsync(
        Guid id,
        SetDiscountRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Setting discount for product {ProductId}: {DiscountPrice}",
                id, request.DiscountPrice);

            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return Result<ProductDto>.Failure("Product not found");
            }

            product.SetDiscount(request.DiscountPrice, updatedBy);

            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Discount set successfully for product: {ProductId}", id);

            var updatedProduct = await _unitOfWork.Products.GetByIdWithIncludesAsync(
                id,
                cancellationToken,
                p => p.Category,
                p => p.Images);

            var productDto = _mapper.Map<ProductDto>(updatedProduct);
            return Result<ProductDto>.Success(productDto, "Discount set successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error setting discount for product: {ProductId}", id);
            return Result<ProductDto>.Failure("Validation error", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting discount for product: {ProductId}", id);
            return Result<ProductDto>.Failure("An error occurred while setting discount", ex.Message);
        }
    }

    public async Task<Result<ProductDto>> RemoveDiscountAsync(
        Guid id,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Removing discount for product: {ProductId}", id);

            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return Result<ProductDto>.Failure("Product not found");
            }

            product.RemoveDiscount(updatedBy);

            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Discount removed successfully for product: {ProductId}", id);

            var updatedProduct = await _unitOfWork.Products.GetByIdWithIncludesAsync(
                id,
                cancellationToken,
                p => p.Category,
                p => p.Images);

            var productDto = _mapper.Map<ProductDto>(updatedProduct);
            return Result<ProductDto>.Success(productDto, "Discount removed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing discount for product: {ProductId}", id);
            return Result<ProductDto>.Failure("An error occurred while removing discount", ex.Message);
        }
    }

    #endregion

    #region Status Operations

    public async Task<Result<ProductDto>> ActivateAsync(
        Guid id,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Activating product: {ProductId}", id);

            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return Result<ProductDto>.Failure("Product not found");
            }

            product.Activate(updatedBy);

            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product activated successfully: {ProductId}", id);

            var updatedProduct = await _unitOfWork.Products.GetByIdWithIncludesAsync(
                id,
                cancellationToken,
                p => p.Category,
                p => p.Images);

            var productDto = _mapper.Map<ProductDto>(updatedProduct);
            return Result<ProductDto>.Success(productDto, "Product activated successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error activating product: {ProductId}", id);
            return Result<ProductDto>.Failure("Validation error", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating product: {ProductId}", id);
            return Result<ProductDto>.Failure("An error occurred while activating the product", ex.Message);
        }
    }

    public async Task<Result<ProductDto>> DeactivateAsync(
        Guid id,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deactivating product: {ProductId}", id);

            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return Result<ProductDto>.Failure("Product not found");
            }

            product.Deactivate(updatedBy);

            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product deactivated successfully: {ProductId}", id);

            var updatedProduct = await _unitOfWork.Products.GetByIdWithIncludesAsync(
                id,
                cancellationToken,
                p => p.Category,
                p => p.Images);

            var productDto = _mapper.Map<ProductDto>(updatedProduct);
            return Result<ProductDto>.Success(productDto, "Product deactivated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating product: {ProductId}", id);
            return Result<ProductDto>.Failure("An error occurred while deactivating the product", ex.Message);
        }
    }

    #endregion
}