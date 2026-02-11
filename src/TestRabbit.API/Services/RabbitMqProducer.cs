using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace TestRabbit.API.Services;

public class RabbitMqProducer(ILogger<RabbitMqProducer> logger, IConfiguration configuration)
{
    private readonly IConnection _connection;
    private readonly IConfiguration _configuration = configuration;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqProducer> _logger = logger;
    private const string ExchangeName = "orders_exchange";
    private const string _queueName = "orders_queue";
    private const string RoutingKey = "order.new";

    public async Task PublishOrderAsync(object order)
    {
        // 1. Create a connection factory
        var factory = new ConnectionFactory { HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"};
        
        // 2. Open connection and channel (Modern .NET 9 uses Async methods)
        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        // 3. Declare the queue (Ensure it exists)
        await channel.QueueDeclareAsync(queue: _queueName, durable: false, 
                    exclusive: false, autoDelete: false);

        // 4. Serialize and Publish
        var json = JsonSerializer.Serialize(order);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(exchange: string.Empty, 
                    routingKey: _queueName, 
                    body: body);
        
        Console.WriteLine($" [x] Sent Order: {json}");
    }
}
