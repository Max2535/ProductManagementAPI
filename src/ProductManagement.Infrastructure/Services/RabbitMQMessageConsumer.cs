using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductManagement.Infrastructure.Services;

/// <summary>
/// Background service for consuming RabbitMQ messages
/// Listens to stock reservation and release events to update product stock
/// </summary>
public class RabbitMQMessageConsumer : BackgroundService
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQMessageConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQMessageConsumer(
        IOptions<RabbitMQSettings> settings,
        ILogger<RabbitMQMessageConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _settings = settings.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            InitializeRabbitMQ();

            // Consume stock reservation events
            ConsumeStockReservationEvents(stoppingToken);

            // Consume stock release events
            ConsumeStockReleaseEvents(stoppingToken);

            _logger.LogInformation("RabbitMQ consumer started successfully");

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RabbitMQ consumer");
        }
    }

    private void InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        _logger.LogInformation("RabbitMQ consumer connection established");
    }

    private void ConsumeStockReservationEvents(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var stockEvent = JsonSerializer.Deserialize<StockReservationEventDto>(message);

                if (stockEvent != null)
                {
                    _logger.LogInformation("Received stock reservation event for order: {OrderId}", stockEvent.OrderId);

                    using var scope = _serviceProvider.CreateScope();
                    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

                    foreach (var item in stockEvent.Items)
                    {
                        // Reserve stock (decrease quantity)
                        var updateRequest = new ProductManagement.Application.DTOs.UpdateStockRequest
                        {
                            Quantity = item.Quantity,
                            Operation = ProductManagement.Application.DTOs.StockOperation.Reduce
                        };
                        await productService.UpdateStockAsync(
                            item.ProductId,
                            updateRequest,
                            "System-StockReservation",
                            stoppingToken);
                    }

                    _channel?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Stock reservation processed successfully for order: {OrderId}", stockEvent.OrderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock reservation event");
                _channel?.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel?.BasicConsume(
            queue: _settings.StockReservationQueue,
            autoAck: false,
            consumer: consumer);
    }

    private void ConsumeStockReleaseEvents(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var stockEvent = JsonSerializer.Deserialize<StockReleaseEventDto>(message);

                if (stockEvent != null)
                {
                    _logger.LogInformation("Received stock release event for order: {OrderId}", stockEvent.OrderId);

                    using var scope = _serviceProvider.CreateScope();
                    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

                    foreach (var item in stockEvent.Items)
                    {
                        // Release stock (increase quantity)
                        var updateRequest = new ProductManagement.Application.DTOs.UpdateStockRequest
                        {
                            Quantity = item.Quantity,
                            Operation = ProductManagement.Application.DTOs.StockOperation.Add
                        };
                        await productService.UpdateStockAsync(
                            item.ProductId,
                            updateRequest,
                            "System-StockRelease",
                            stoppingToken);
                    }

                    _channel?.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("Stock release processed successfully for order: {OrderId}", stockEvent.OrderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock release event");
                _channel?.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel?.BasicConsume(
            queue: _settings.StockReleaseQueue,
            autoAck: false,
            consumer: consumer);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
    }
}

// DTOs for message consumption
public class StockReservationEventDto
{
    public Guid OrderId { get; set; }
    public List<StockItemDto> Items { get; set; } = new();
}

public class StockReleaseEventDto
{
    public Guid OrderId { get; set; }
    public List<StockItemDto> Items { get; set; } = new();
}

public class StockItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
