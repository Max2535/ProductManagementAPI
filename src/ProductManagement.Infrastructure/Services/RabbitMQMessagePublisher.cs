using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Models;
using ProductManagement.Application.Services;
using RabbitMQ.Client;

namespace ProductManagement.Infrastructure.Services;

/// <summary>
/// RabbitMQ message publisher implementation
/// </summary>
public class RabbitMQMessagePublisher : IMessagePublisher, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQMessagePublisher> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQMessagePublisher(
        IOptions<RabbitMQSettings> settings,
        ILogger<RabbitMQMessagePublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchanges
            _channel.ExchangeDeclare(
                exchange: _settings.OrderExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _channel.ExchangeDeclare(
                exchange: _settings.ProductExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // Declare queues
            _channel.QueueDeclare(
                queue: _settings.OrderCreatedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueDeclare(
                queue: _settings.StockReservationQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueDeclare(
                queue: _settings.StockReleaseQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueDeclare(
                queue: _settings.ProductUpdatedQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Bind queues to exchanges
            _channel.QueueBind(
                queue: _settings.OrderCreatedQueue,
                exchange: _settings.OrderExchange,
                routingKey: _settings.OrderCreatedRoutingKey);

            _channel.QueueBind(
                queue: _settings.StockReservationQueue,
                exchange: _settings.OrderExchange,
                routingKey: _settings.StockReservationRoutingKey);

            _channel.QueueBind(
                queue: _settings.StockReleaseQueue,
                exchange: _settings.OrderExchange,
                routingKey: _settings.StockReleaseRoutingKey);

            _channel.QueueBind(
                queue: _settings.ProductUpdatedQueue,
                exchange: _settings.ProductExchange,
                routingKey: _settings.ProductUpdatedRoutingKey);

            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ connection");
            throw;
        }
    }

    public Task PublishOrderCreatedEventAsync(OrderCreatedEvent orderEvent)
    {
        return PublishMessageAsync(
            _settings.OrderExchange,
            _settings.OrderCreatedRoutingKey,
            orderEvent);
    }

    public Task PublishStockReservationEventAsync(StockReservationEvent reservationEvent)
    {
        return PublishMessageAsync(
            _settings.OrderExchange,
            _settings.StockReservationRoutingKey,
            reservationEvent);
    }

    public Task PublishStockReleaseEventAsync(StockReleaseEvent releaseEvent)
    {
        return PublishMessageAsync(
            _settings.OrderExchange,
            _settings.StockReleaseRoutingKey,
            releaseEvent);
    }

    public Task PublishProductUpdatedEventAsync(ProductUpdatedEvent productEvent)
    {
        return PublishMessageAsync(
            _settings.ProductExchange,
            _settings.ProductUpdatedRoutingKey,
            productEvent);
    }

    private Task PublishMessageAsync<T>(string exchange, string routingKey, T message)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation(
                "Published message to exchange {Exchange} with routing key {RoutingKey}: {MessageType}",
                exchange, routingKey, typeof(T).Name);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to RabbitMQ");
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
