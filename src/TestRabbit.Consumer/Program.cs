using TestRabbit.Consumer;
using TestRabbit.Consumer.Services;

var builder = Host.CreateApplicationBuilder(args);

// Register services
builder.Services.AddSingleton<OrderProcessor>();

// Register RabbitMQ Consumer using factory pattern
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RabbitMqConsumer>>();
    var configuration = sp.GetRequiredService<IConfiguration>();
    var orderProcessor = sp.GetRequiredService<OrderProcessor>();
    return RabbitMqConsumer.CreateAsync(logger, configuration, orderProcessor).GetAwaiter().GetResult();
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
