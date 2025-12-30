using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;

namespace ProductManagement.Application.Interfaces;

/// <summary>
/// Service interface for Order operations
/// </summary>
public interface IOrderService
{
    Task<Result<OrderDto>> CreateOrderAsync(CreateOrderRequest request, string createdBy, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> GetOrderByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<OrderSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<OrderSummaryDto>>> GetOrdersByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, string updatedBy, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> AddOrderItemAsync(Guid orderId, OrderItemRequest request, string updatedBy, CancellationToken cancellationToken = default);
    Task<Result<OrderDto>> UpdateOrderItemQuantityAsync(Guid orderId, UpdateOrderItemQuantityRequest request, string updatedBy, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteOrderAsync(Guid orderId, string deletedBy, CancellationToken cancellationToken = default);
}
