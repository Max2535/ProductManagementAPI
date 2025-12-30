using ProductManagement.Application.Services;

namespace ProductManagement.Application.Interfaces;

/// <summary>
/// Interface for message publishing operations
/// </summary>
public interface IMessagePublisher
{
    Task PublishOrderCreatedEventAsync(OrderCreatedEvent orderEvent);
    Task PublishStockReservationEventAsync(StockReservationEvent reservationEvent);
    Task PublishStockReleaseEventAsync(StockReleaseEvent releaseEvent);
    Task PublishProductUpdatedEventAsync(ProductUpdatedEvent productEvent);
}

/// <summary>
/// Product updated event
/// </summary>
public class ProductUpdatedEvent
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}
