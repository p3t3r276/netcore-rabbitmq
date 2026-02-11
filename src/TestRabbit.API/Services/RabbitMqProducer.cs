using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using TestRabbit.API.Models;

namespace TestRabbit.API.Services;

public class RabbitMqProducer(IConnection connection, IChannel channel, ILogger<RabbitMqProducer> logger) : IAsyncDisposable
{
    private readonly IConnection _connection = connection;
    private readonly IChannel _channel = channel;
    private readonly ILogger<RabbitMqProducer> _logger = logger;
    private const string ExchangeName = "orders_exchange";
    private const string QueueName = "orders_queue";
    private const string RoutingKey = "order.new";

    public static async Task<RabbitMqProducer> CreateAsync(
        ILogger<RabbitMqProducer> logger, 
        IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        // Declare exchange
        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        // Declare queue
        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Bind queue to exchange
        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey
        );

        logger.LogInformation("RabbitMQ Producer initialized. Exchange: {Exchange}, Queue: {Queue}", 
            ExchangeName, QueueName);

        return new RabbitMqProducer(connection, channel, logger);
    }

    public async Task PublishOrderAsync(Order order)
    {
        try
        {
            var message = JsonSerializer.Serialize(order);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties
            {
                Persistent = true, // Make message persistent
                ContentType = "application/json",
                MessageId = order.OrderId,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: RoutingKey,
                mandatory: false,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation(
                "Published order {OrderId} for customer {CustomerId}. Total: ${TotalAmount:F2}",
                order.OrderId, 
                order.CustomerId, 
                order.TotalAmount
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing order {OrderId}", order.OrderId);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();
        if (_connection != null)
            await _connection.CloseAsync();
        _logger.LogInformation("RabbitMQ Producer disposed");
    }
}
