
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TestRabbit.API.Models;

namespace TestRabbit.API.Services;

public class RabbitMqConsumer : IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly OrderProcessor _orderProcessor;
    private const string QueueName = "orders_queue";

    private RabbitMqConsumer(
        IConnection connection,
        IChannel channel,
        ILogger<RabbitMqConsumer> logger,
        OrderProcessor orderProcessor)
    {
        _connection = connection;
        _channel = channel;
        _logger = logger;
        _orderProcessor = orderProcessor;
    }

    public static async Task<RabbitMqConsumer> CreateAsync(
        ILogger<RabbitMqConsumer> logger, 
        IConfiguration configuration,
        OrderProcessor orderProcessor)
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

        // Ensure queue exists (idempotent)
        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Set prefetch count to process one message at a time
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

        logger.LogInformation("RabbitMQ Consumer initialized. Queue: {Queue}", QueueName);

        return new RabbitMqConsumer(connection, channel, logger, orderProcessor);
    }

    public async Task StartConsumingAsync()
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                _logger.LogInformation("Received message from queue");

                var order = JsonSerializer.Deserialize<Order>(message);
                
                if (order != null)
                {
                    // Process the order
                    await _orderProcessor.ProcessOrderAsync(order);

                    // Acknowledge the message
                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                    
                    _logger.LogInformation(
                        "Successfully processed and acknowledged order {OrderId}",
                        order.OrderId
                    );
                }
                else
                {
                    _logger.LogWarning("Deserialized order is null");
                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing message. Message will be rejected.");
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message. Message will be requeued.");
                // Requeue the message for retry
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false, // Manual acknowledgment
            consumer: consumer
        );

        _logger.LogInformation("Started consuming messages from queue {Queue}", QueueName);
    }



    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();
        if (_connection != null)
            await _connection.CloseAsync();
        _logger.LogInformation("RabbitMQ Consumer disposed");
    }
}
