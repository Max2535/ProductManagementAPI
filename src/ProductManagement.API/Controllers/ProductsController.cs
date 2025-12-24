using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Interfaces;

namespace ProductManagement.API.Controllers;

/// <summary>
/// Products API Controller
/// RESTful API best practices:
/// - Proper HTTP verbs (GET, POST, PUT, DELETE)
/// - Appropriate status codes
/// - Consistent response format
/// - Async operations
/// - Request validation
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;
    private readonly IValidator<UpdateStockRequest> _stockValidator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator,
        IValidator<UpdateStockRequest> stockValidator,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _stockValidator = stockValidator;
        _logger = logger;
    }

    /// <summary>
    /// Get all products with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <returns>Paged list of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        if (pageNumber < 1)
            return BadRequest(ApiResponse<object>.FailureResponse("Page number must be greater than 0"));

        if (pageSize < 1 || pageSize > 100)
            return BadRequest(ApiResponse<object>.FailureResponse("Page size must be between 1 and 100"));

        var result = await _productService.GetPagedAsync(pageNumber, pageSize, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<PagedResult<ProductSummaryDto>>.SuccessResponse(result.Data!))
            : StatusCode(500, ApiResponse<PagedResult<ProductSummaryDto>>.FailureResponse(
                result.Message, result.Errors));
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return Ok(ApiResponse<ProductDto>.SuccessResponse(result.Data!));
    }

    /// <summary>
    /// Get product by SKU
    /// </summary>
    /// <param name="sku">Product SKU</param>
    /// <returns>Product details</returns>
    [HttpGet("by-sku/{sku}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductBySku(
        string sku,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetBySkuAsync(sku, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return Ok(ApiResponse<ProductDto>.SuccessResponse(result.Data!));
    }

    /// <summary>
    /// Search products with filters
    /// </summary>
    /// <param name="request">Search criteria</param>
    /// <returns>Filtered and paged products</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] ProductSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.SearchAsync(request, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<PagedResult<ProductSummaryDto>>.SuccessResponse(result.Data!))
            : StatusCode(500, ApiResponse<PagedResult<ProductSummaryDto>>.FailureResponse(
                result.Message, result.Errors));
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>List of products in the category</returns>
    [HttpGet("by-category/{categoryId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsByCategory(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetByCategoryAsync(categoryId, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return Ok(ApiResponse<List<ProductSummaryDto>>.SuccessResponse(result.Data!));
    }

    /// <summary>
    /// Get low stock products
    /// </summary>
    /// <returns>List of products with low stock</returns>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStockProducts(CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetLowStockProductsAsync(cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<List<ProductSummaryDto>>.SuccessResponse(result.Data!))
            : StatusCode(500, ApiResponse<List<ProductSummaryDto>>.FailureResponse(
                result.Message, result.Errors));
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="request">Product creation data</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate request
        var validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<object>.FailureResponse("Validation failed", errors));
        }

        // Get current user (simplified - in real app, get from JWT claims)
        var currentUser = User.Identity?.Name ?? "System";

        var result = await _productService.CreateAsync(request, currentUser, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = result.Data!.Id },
            ApiResponse<ProductDto>.SuccessResponse(result.Data, "Product created successfully"));
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Product update data</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<object>.FailureResponse("Validation failed", errors));
        }

        var currentUser = User.Identity?.Name ?? "System";

        var result = await _productService.UpdateAsync(id, request, currentUser, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return Ok(ApiResponse<ProductDto>.SuccessResponse(result.Data!, "Product updated successfully"));
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var result = await _productService.DeleteAsync(id, currentUser, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return Ok(ApiResponse<object>.SuccessResponse(null!, result.Message));
    }

    /// <summary>
    /// Update product stock
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Stock update data</param>
    /// <returns>Updated product</returns>
    [HttpPatch("{id:guid}/stock")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStock(
        Guid id,
        [FromBody] UpdateStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _stockValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<object>.FailureResponse("Validation failed", errors));
        }

        var currentUser = User.Identity?.Name ?? "System";

        var result = await _productService.UpdateStockAsync(id, request, currentUser, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return Ok(ApiResponse<ProductDto>.SuccessResponse(result.Data!, "Stock updated successfully"));
    }

    /// <summary>
    /// Set discount price for a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="request">Discount data</param>
    /// <returns>Updated product</returns>
    [HttpPatch("{id:guid}/discount")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetDiscount(
        Guid id,
        [FromBody] SetDiscountRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var result = await _productService.SetDiscountAsync(id, request, currentUser, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return Ok(ApiResponse<ProductDto>.SuccessResponse(result.Data!, "Discount set successfully"));
    }

    /// <summary>
    /// Remove discount from a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Updated product</returns>
    [HttpDelete("{id:guid}/discount")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveDiscount(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var result = await _productService.RemoveDiscountAsync(id, currentUser, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return Ok(ApiResponse<ProductDto>.SuccessResponse(result.Data!, "Discount removed successfully"));
    }

    /// <summary>
    /// Activate a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Updated product</returns>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var result = await _productService.ActivateAsync(id, currentUser, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return Ok(ApiResponse<ProductDto>.SuccessResponse(result.Data!, "Product activated successfully"));
    }

    /// <summary>
    /// Deactivate a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Updated product</returns>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var currentUser = User.Identity?.Name ?? "System";

        var result = await _productService.DeactivateAsync(id, currentUser, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return Ok(ApiResponse<ProductDto>.SuccessResponse(result.Data!, "Product deactivated successfully"));
    }
}