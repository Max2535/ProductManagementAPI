using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Interfaces;
using System.Security.Claims;

namespace ProductManagement.API.Controllers;

/// <summary>
/// Orders API Controller
/// Manages order operations and integrates with RabbitMQ for event-driven communication
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="request">Order creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created order</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();

        _logger.LogInformation("Creating order for user: {UserId}", userId);

        // Override UserId with authenticated user
        request.UserId = userId;

        var result = await _orderService.CreateOrderAsync(request, userName, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = result.Data!.Id },
            ApiResponse<OrderDto>.SuccessResponse(result.Data, "Order created successfully"));
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetOrderByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.FailureResponse(result.Message));

        return Ok(ApiResponse<OrderDto>.SuccessResponse(result.Data!));
    }

    /// <summary>
    /// Get order by order number
    /// </summary>
    /// <param name="orderNumber">Order number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Order details</returns>
    [HttpGet("number/{orderNumber}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderByNumber(
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetOrderByOrderNumberAsync(orderNumber, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.FailureResponse(result.Message));

        return Ok(ApiResponse<OrderDto>.SuccessResponse(result.Data!));
    }

    /// <summary>
    /// Get all orders with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of orders</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
            return BadRequest(ApiResponse<object>.FailureResponse("Page number must be greater than 0"));

        if (pageSize < 1 || pageSize > 100)
            return BadRequest(ApiResponse<object>.FailureResponse("Page size must be between 1 and 100"));

        var result = await _orderService.GetPagedAsync(pageNumber, pageSize, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<PagedResult<OrderSummaryDto>>.SuccessResponse(result.Data!))
            : StatusCode(500, ApiResponse<PagedResult<OrderSummaryDto>>.FailureResponse(
                result.Message, result.Errors));
    }

    /// <summary>
    /// Get orders for current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's orders</returns>
    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.GetOrdersByUserIdAsync(userId, cancellationToken);

        return result.IsSuccess
            ? Ok(ApiResponse<IEnumerable<OrderSummaryDto>>.SuccessResponse(result.Data!))
            : StatusCode(500, ApiResponse<IEnumerable<OrderSummaryDto>>.FailureResponse(
                result.Message, result.Errors));
    }

    /// <summary>
    /// Update order status (confirm, pay, ship, deliver, cancel)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Status update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var userName = GetCurrentUserName();
        var result = await _orderService.UpdateOrderStatusAsync(id, request, userName, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return Ok(ApiResponse<OrderDto>.SuccessResponse(result.Data!, "Order status updated successfully"));
    }

    /// <summary>
    /// Add item to order (only for pending orders)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Order item request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order</returns>
    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrderItem(
        Guid id,
        [FromBody] OrderItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var userName = GetCurrentUserName();
        var result = await _orderService.AddOrderItemAsync(id, request, userName, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return Ok(ApiResponse<OrderDto>.SuccessResponse(result.Data!, "Order item added successfully"));
    }

    /// <summary>
    /// Update order item quantity (only for pending orders)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="request">Update quantity request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated order</returns>
    [HttpPatch("{id:guid}/items")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOrderItemQuantity(
        Guid id,
        [FromBody] UpdateOrderItemQuantityRequest request,
        CancellationToken cancellationToken = default)
    {
        var userName = GetCurrentUserName();
        var result = await _orderService.UpdateOrderItemQuantityAsync(id, request, userName, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.FailureResponse(result.Message, result.Errors));

        return Ok(ApiResponse<OrderDto>.SuccessResponse(result.Data!, "Order item updated successfully"));
    }

    /// <summary>
    /// Delete order (only pending or cancelled orders)
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userName = GetCurrentUserName();
        var result = await _orderService.DeleteOrderAsync(id, userName, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.FailureResponse(result.Message));

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Order deleted successfully"));
    }

    // Helper methods
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private string GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }
}
