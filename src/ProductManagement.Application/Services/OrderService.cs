using AutoMapper;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Common;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Interfaces;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Interfaces;

namespace ProductManagement.Application.Services;

/// <summary>
/// Order Service Implementation
/// Handles business logic for order operations
/// </summary>
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;
    private readonly IMessagePublisher _messagePublisher;

    public OrderService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OrderService> logger,
        IMessagePublisher messagePublisher)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _messagePublisher = messagePublisher;
    }

    public async Task<Result<OrderDto>> CreateOrderAsync(
        CreateOrderRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating order for user: {UserId}", request.UserId);

            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
                return Result<OrderDto>.Failure("User not found");

            // Create order
            var order = Order.Create(
                request.UserId,
                request.ShippingAddress,
                request.Notes,
                createdBy);

            // Add order items and validate products
            foreach (var item in request.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
                if (product == null)
                    return Result<OrderDto>.Failure($"Product with ID {item.ProductId} not found");

                if (!product.IsInStock)
                    return Result<OrderDto>.Failure($"Product '{product.Name}' is out of stock");

                if (product.StockQuantity < item.Quantity)
                    return Result<OrderDto>.Failure($"Insufficient stock for product '{product.Name}'");

                order.AddOrderItem(
                    product.Id,
                    product.Name,
                    product.EffectivePrice,
                    item.Quantity);
            }

            // Save order
            await _unitOfWork.Orders.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order created successfully: {OrderNumber}", order.OrderNumber);

            // Publish event to RabbitMQ
            await _messagePublisher.PublishOrderCreatedEventAsync(new OrderCreatedEvent
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                TotalAmount = order.GrandTotal,
                Items = order.OrderItems.Select(oi => new OrderItemEvent
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList(),
                CreatedAt = order.CreatedAt
            });

            var orderDto = await GetOrderByIdAsync(order.Id, cancellationToken);
            return orderDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return Result<OrderDto>.Failure("An error occurred while creating the order", ex.Message);
        }
    }

    public async Task<Result<OrderDto>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting order with ID: {OrderId}", orderId);

            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId, cancellationToken);
            if (order == null)
                return Result<OrderDto>.Failure("Order not found");

            var orderDto = _mapper.Map<OrderDto>(order);
            return Result<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order with ID: {OrderId}", orderId);
            return Result<OrderDto>.Failure("An error occurred while retrieving the order", ex.Message);
        }
    }

    public async Task<Result<OrderDto>> GetOrderByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting order with number: {OrderNumber}", orderNumber);

            var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber, cancellationToken);
            if (order == null)
                return Result<OrderDto>.Failure("Order not found");

            var orderDto = _mapper.Map<OrderDto>(order);
            return Result<OrderDto>.Success(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order with number: {OrderNumber}", orderNumber);
            return Result<OrderDto>.Failure("An error occurred while retrieving the order", ex.Message);
        }
    }

    public async Task<Result<PagedResult<OrderSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting paged orders - Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

            var pagedData = await _unitOfWork.Orders.GetPagedAsync(
                pageNumber,
                pageSize);
            var orders = pagedData.Item1;
            var totalCount = pagedData.Item2;

            var orderDtos = _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);

            var pagedResult = new PagedResult<OrderSummaryDto>
            {
                Items = orderDtos.ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Result<PagedResult<OrderSummaryDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged orders");
            return Result<PagedResult<OrderSummaryDto>>.Failure("An error occurred while retrieving orders", ex.Message);
        }
    }

    public async Task<Result<IEnumerable<OrderSummaryDto>>> GetOrdersByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting orders for user: {UserId}", userId);

            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId, cancellationToken);
            var orderDtos = _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);

            return Result<IEnumerable<OrderSummaryDto>>.Success(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for user: {UserId}", userId);
            return Result<IEnumerable<OrderSummaryDto>>.Failure("An error occurred while retrieving orders", ex.Message);
        }
    }

    public async Task<Result<OrderDto>> UpdateOrderStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating order status: {OrderId} to {Status}", orderId, request.Status);

            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId, cancellationToken);
            if (order == null)
                return Result<OrderDto>.Failure("Order not found");

            // Update status based on request
            switch (request.Status.ToLower())
            {
                case "confirmed":
                    order.Confirm(updatedBy);
                    break;
                case "paid":
                    order.Pay(updatedBy);
                    // Publish stock reservation event
                    await _messagePublisher.PublishStockReservationEventAsync(new StockReservationEvent
                    {
                        OrderId = order.Id,
                        Items = order.OrderItems.Select(oi => new StockReservationItem
                        {
                            ProductId = oi.ProductId,
                            Quantity = oi.Quantity
                        }).ToList()
                    });
                    break;
                case "shipped":
                    order.Ship(updatedBy);
                    break;
                case "delivered":
                    order.Deliver(updatedBy);
                    break;
                case "cancelled":
                    order.Cancel(updatedBy, request.Reason);
                    // Publish stock release event
                    await _messagePublisher.PublishStockReleaseEventAsync(new StockReleaseEvent
                    {
                        OrderId = order.Id,
                        Items = order.OrderItems.Select(oi => new StockReleaseItem
                        {
                            ProductId = oi.ProductId,
                            Quantity = oi.Quantity
                        }).ToList()
                    });
                    break;
                default:
                    return Result<OrderDto>.Failure("Invalid status");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order status updated: {OrderNumber}", order.OrderNumber);

            var orderDto = await GetOrderByIdAsync(order.Id, cancellationToken);
            return orderDto;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating order status");
            return Result<OrderDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status");
            return Result<OrderDto>.Failure("An error occurred while updating order status", ex.Message);
        }
    }

    public async Task<Result<OrderDto>> AddOrderItemAsync(
        Guid orderId,
        OrderItemRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Adding item to order: {OrderId}", orderId);

            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId, cancellationToken);
            if (order == null)
                return Result<OrderDto>.Failure("Order not found");

            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
                return Result<OrderDto>.Failure("Product not found");

            order.AddOrderItem(
                product.Id,
                product.Name,
                product.EffectivePrice,
                request.Quantity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var orderDto = await GetOrderByIdAsync(order.Id, cancellationToken);
            return orderDto;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when adding order item");
            return Result<OrderDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to order");
            return Result<OrderDto>.Failure("An error occurred while adding order item", ex.Message);
        }
    }

    public async Task<Result<OrderDto>> UpdateOrderItemQuantityAsync(
        Guid orderId,
        UpdateOrderItemQuantityRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating order item quantity: {OrderId}", orderId);

            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(orderId, cancellationToken);
            if (order == null)
                return Result<OrderDto>.Failure("Order not found");

            order.UpdateOrderItemQuantity(request.OrderItemId, request.Quantity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var orderDto = await GetOrderByIdAsync(order.Id, cancellationToken);
            return orderDto;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating order item");
            return Result<OrderDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order item quantity");
            return Result<OrderDto>.Failure("An error occurred while updating order item", ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteOrderAsync(
        Guid orderId,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting order: {OrderId}", orderId);

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
            if (order == null)
                return Result<bool>.Failure("Order not found");

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Cancelled)
                return Result<bool>.Failure("Only pending or cancelled orders can be deleted");

            await _unitOfWork.Orders.DeleteAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order deleted: {OrderId}", orderId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order");
            return Result<bool>.Failure("An error occurred while deleting the order", ex.Message);
        }
    }
}

// Event DTOs for RabbitMQ messaging
public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemEvent> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class OrderItemEvent
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class StockReservationEvent
{
    public Guid OrderId { get; set; }
    public List<StockReservationItem> Items { get; set; } = new();
}

public class StockReservationItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class StockReleaseEvent
{
    public Guid OrderId { get; set; }
    public List<StockReleaseItem> Items { get; set; } = new();
}

public class StockReleaseItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
