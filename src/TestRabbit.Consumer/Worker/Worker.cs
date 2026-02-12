using TestRabbit.Consumer.Services;

namespace TestRabbit.Consumer;

public class Worker(ILogger<Worker> logger, RabbitMqConsumer consumer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Order Processing Worker started at: {time}", DateTimeOffset.UtcNow);

        // Start consuming messages from RabbitMQ
        await consumer.StartConsumingAsync();

        logger.LogInformation("Worker is now listening for orders...");

        // Keep the worker running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        logger.LogInformation("Order Processing Worker stopping at: {time}", DateTimeOffset.UtcNow);
    }
}
