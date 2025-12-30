namespace ProductManagement.Application.Models;

/// <summary>
/// RabbitMQ configuration settings
/// </summary>
public class RabbitMQSettings
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    // Exchange names
    public string OrderExchange { get; set; } = "order.exchange";
    public string ProductExchange { get; set; } = "product.exchange";

    // Queue names
    public string OrderCreatedQueue { get; set; } = "order.created";
    public string StockReservationQueue { get; set; } = "stock.reservation";
    public string StockReleaseQueue { get; set; } = "stock.release";
    public string ProductUpdatedQueue { get; set; } = "product.updated";

    // Routing keys
    public string OrderCreatedRoutingKey { get; set; } = "order.created";
    public string StockReservationRoutingKey { get; set; } = "stock.reservation";
    public string StockReleaseRoutingKey { get; set; } = "stock.release";
    public string ProductUpdatedRoutingKey { get; set; } = "product.updated";
}
