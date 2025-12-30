namespace ProductManagement.Domain.Entities;

/// <summary>
/// Order entity - Rich Domain Model
/// </summary>
public class Order : BaseEntity<Guid>
{
    private readonly List<OrderItem> _orderItems = new();

    public string OrderNumber { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal ShippingFee { get; private set; }
    public decimal TotalDiscount { get; private set; }
    public decimal GrandTotal { get; private set; }

    public string ShippingAddress { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    public DateTime? PaymentDate { get; private set; }
    public DateTime? ShippingDate { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
    public DateTime? CancelDate { get; private set; }

    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    // Factory Method Pattern
    public static Order Create(
        Guid userId,
        string shippingAddress,
        string? notes,
        string createdBy)
    {
        var orderNumber = GenerateOrderNumber();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            UserId = userId,
            Status = OrderStatus.Pending,
            ShippingAddress = shippingAddress,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            ShippingFee = 0,
            TotalDiscount = 0
        };

        return order;
    }

    // Business Logic Methods
    public void AddOrderItem(Guid productId, string productName, decimal price, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to a non-pending order");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

        var orderItem = OrderItem.Create(Id, productId, productName, price, quantity);
        _orderItems.Add(orderItem);

        RecalculateTotals();
    }

    public void RemoveOrderItem(Guid orderItemId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot remove items from a non-pending order");

        var item = _orderItems.FirstOrDefault(x => x.Id == orderItemId);
        if (item != null)
        {
            _orderItems.Remove(item);
            RecalculateTotals();
        }
    }

    public void UpdateOrderItemQuantity(Guid orderItemId, int newQuantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot update items in a non-pending order");

        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0", nameof(newQuantity));

        var item = _orderItems.FirstOrDefault(x => x.Id == orderItemId);
        if (item == null)
            throw new InvalidOperationException("Order item not found");

        item.UpdateQuantity(newQuantity);
        RecalculateTotals();
    }

    public void SetShippingFee(decimal fee)
    {
        if (fee < 0)
            throw new ArgumentException("Shipping fee cannot be negative", nameof(fee));

        ShippingFee = fee;
        RecalculateTotals();
    }

    public void ApplyDiscount(decimal discount)
    {
        if (discount < 0)
            throw new ArgumentException("Discount cannot be negative", nameof(discount));

        TotalDiscount = discount;
        RecalculateTotals();
    }

    public void Confirm(string confirmedBy)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        if (!_orderItems.Any())
            throw new InvalidOperationException("Cannot confirm order without items");

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = confirmedBy;
    }

    public void Pay(string paidBy)
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be paid");

        Status = OrderStatus.Paid;
        PaymentDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = paidBy;
    }

    public void Ship(string shippedBy)
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException("Only paid orders can be shipped");

        Status = OrderStatus.Shipped;
        ShippingDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = shippedBy;
    }

    public void Deliver(string deliveredBy)
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Only shipped orders can be delivered");

        Status = OrderStatus.Delivered;
        DeliveryDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = deliveredBy;
    }

    public void Cancel(string cancelledBy, string? reason)
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel delivered orders");

        Status = OrderStatus.Cancelled;
        CancelDate = DateTime.UtcNow;
        Notes = string.IsNullOrEmpty(Notes)
            ? $"Cancelled: {reason}"
            : $"{Notes}\nCancelled: {reason}";
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = cancelledBy;
    }

    private void RecalculateTotals()
    {
        TotalAmount = _orderItems.Sum(x => x.TotalPrice);
        GrandTotal = TotalAmount + ShippingFee - TotalDiscount;
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }
}

/// <summary>
/// Order status enumeration
/// </summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Paid = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}
